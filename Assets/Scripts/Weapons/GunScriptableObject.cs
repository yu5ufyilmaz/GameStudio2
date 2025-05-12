using System;
using System.Collections;
using System.Collections.Generic;
using DotGalacticos.Guns.ImpactEffects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace DotGalacticos.Guns
{
    [CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
    public class GunScriptableObject : ScriptableObject, ICloneable
    {
        public GunType Type;
        public GunPlace Place;
        public string Name;
        public GameObject ModelPrefab;
        public GameObject PickupPrefab;
        public GameObject impactPrefab;
        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;

        public Vector3 SecondHandPositionOffset;
        public Vector3 SecondHandRotationOffset;

        public DamageConfigScriptableObject DamageConfig;
        public AmmoScriptableObject AmmoConfig;
        public ShootScriptableObject ShootConfig;
        public TrailScriptableObject TrailConfig;
        public AudioScriptableObject AudioConfig;

        public ICollisionHandler[] BulletImpactEffects = new ICollisionHandler[0];

        private MonoBehaviour ActiveMonoBehaviour;
        public GameObject Model;
        private AudioSource modelAudioSource;
        public GameObject secondHandTarget;

        private float LastShootTime;
        private float InitialClickTime;
        private float StopShootingTime;
        private bool LastFrameWantedToShoot;

        public bool canShoot = true;

        private ParticleSystem ShootSystem;
        private ObjectPool<TrailRenderer> TrailPool;

        [SerializeField]
        private AudioClip shootSound;

        [Range(0, 1)]
        public float shootAudioVolume = 0.5f;

        public Dictionary<string, int> AmmoStorage = new Dictionary<string, int>();

        public void Spawn(
            Transform Parent,
            MonoBehaviour ActiveMonoBehaviour,
            Camera ActiveCamera = null
        )
        {
            this.ActiveMonoBehaviour = ActiveMonoBehaviour;
            /*
            LastShootTime = 0;
            
            AmmoConfig.CurrentClipAmmo = AmmoConfig.ClipSize;
            AmmoConfig.CurrentAmmo = AmmoConfig.MaxAmmo;*/

            TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

            Model = Instantiate(ModelPrefab);
            Model.transform.SetParent(Parent, false);
            Model.transform.localPosition = SpawnPosition;
            Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

            secondHandTarget = Model.transform.GetChild(0).gameObject;

            ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
            modelAudioSource = Model.GetComponentInChildren<AudioSource>();

            SoundManager.instance.RegisterAudioSource(modelAudioSource);
        }

        public void Despawn()
        {
            // We do a bunch of other stuff on the same frame, so we really want it to be immediately destroyed, not at Unity's convenience.
            SetActiveModel(false);

            Debug.Log($"Destroying {Model.name}");
            Destroy(Model);
            TrailPool.Clear();

            SoundManager.instance.UnregisterAudioSource(modelAudioSource);

            modelAudioSource = null;
            ShootSystem = null;
        }

        public void SetActiveModel(bool active)
        {
            Model.SetActive(active);
        }

        public bool CanReload()
        {
            return AmmoConfig.CanReload();
        }

        public void EndReload()
        {
            AmmoConfig.Reload();
            canShoot = true;
        }

        public void StartReloading()
        {
            AudioConfig.PlayReloadClip(modelAudioSource);
            canShoot = false;
        }

        public void SaveAmmo(string gunName, int currentClipAmmo, int currentAmmo)
        {
            AmmoStorage[gunName] = currentClipAmmo;
            AmmoStorage[gunName + "_Total"] = currentAmmo;
        }

        public int GetClipAmmo(string gunName)
        {
            return AmmoStorage.ContainsKey(gunName)
                ? AmmoStorage[gunName]
                : AmmoConfig.CurrentClipAmmo;
        }

        public int GetTotalAmmo(string gunName)
        {
            return AmmoStorage.ContainsKey(gunName + "_Total")
                ? AmmoStorage[gunName + "_Total"]
                : AmmoConfig.CurrentAmmo;
        }

        private bool hasPlayedOutOfAmmoClip = false;

        public void Shoot()
        {
            if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
            {
                float lastDuration = Mathf.Clamp(
                    0,
                    (StopShootingTime - InitialClickTime),
                    ShootConfig.MaxSpreadTime
                );

                float lerpTime =
                    (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                    / ShootConfig.RecoilRecoverySpeed;

                InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
            }
            if (Time.time > ShootConfig.FireRate + LastShootTime)
            {
                if (AmmoConfig.CurrentClipAmmo > 0 && canShoot == true)
                {
                    LastShootTime = Time.time;
                    CameraShake.Instance.ShakeCamera(
                        ShootConfig.ShakeIntensity,
                        ShootConfig.ShakeTime
                    );
                    ShootSystem.Play();
                    AudioConfig.PlayShotingClip(modelAudioSource, AmmoConfig.CurrentClipAmmo == 1);

                    // Nişan alma hedef pozisyonunu al
                    AmmoConfig.CurrentClipAmmo--;
                    hasPlayedOutOfAmmoClip = false;
                    for (int i = 0; i < ShootConfig.BulletPerShoot; i++)
                    {
                        Vector3 spreadAmount = ShootConfig.GetSpread();

                        Model.transform.forward += Model.transform.TransformDirection(spreadAmount);

                        Vector3 shootDirection = -Model.transform.forward + spreadAmount;

                        // Raycast ile merminin gideceği yönü kontrol et
                        if (
                            Physics.Raycast(
                                ShootSystem.transform.position,
                                shootDirection,
                                out RaycastHit hit,
                                float.MaxValue,
                                ShootConfig.HitMask
                            )
                        )
                        {
                            ActiveMonoBehaviour.StartCoroutine(
                                PlayTrail(ShootSystem.transform.position, hit.point, hit)
                            );
                        }
                        else
                        {
                            ActiveMonoBehaviour.StartCoroutine(
                                PlayTrail(
                                    ShootSystem.transform.position,
                                    ShootSystem.transform.position
                                        + (shootDirection * TrailConfig.MissDistance),
                                    new RaycastHit()
                                )
                            );
                        }
                    }
                }
                else
                {
                    if (!hasPlayedOutOfAmmoClip)
                    {
                        Debug.Log("Playing out of ammo clip."); // Debug mesajı
                        AudioConfig.PlayOutOfAmmoClip(modelAudioSource);
                        hasPlayedOutOfAmmoClip = true; // Ses çaldı, tekrar çalmaması için değişkeni güncelle
                    }
                }
            }
        }

        public void Tick(bool WantsToShoot)
        {
            Model.transform.localRotation = Quaternion.Lerp(
                Model.transform.localRotation,
                Quaternion.Euler(SpawnRotation),
                Time.deltaTime * ShootConfig.RecoilRecoverySpeed
            );
            if (WantsToShoot)
            {
                LastFrameWantedToShoot = true;
                Shoot();
            }
            if (LastFrameWantedToShoot)
            {
                StopShootingTime = Time.time;
                LastFrameWantedToShoot = false;
            }
        }

        public Vector3 GetRaycastOrigin()
        {
            Vector3 origin = ShootSystem.transform.position;
            return origin;
        }

        public Vector3 GetGunForward()
        {
            return Model.transform.forward;
        }

        private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
        {
            TrailRenderer instance = TrailPool.Get();
            instance.gameObject.SetActive(true);
            instance.transform.position = StartPoint;
            yield return null;

            instance.emitting = true;

            float distance = Vector3.Distance(StartPoint, EndPoint);
            float remainingDistance = distance;
            while (remainingDistance > 0)
            {
                instance.transform.position = Vector3.Lerp(
                    StartPoint,
                    EndPoint,
                    Mathf.Clamp01(1 - (remainingDistance / distance))
                );
                remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;
                yield return null;
            }
            instance.transform.position = EndPoint;

            if (Hit.collider != null)
            {
                if (Hit.collider.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.TakeDamage(DamageConfig.GetDamage(distance), Hit.point); // Vurulduğu nokta
                }
                else
                {
                    if (impactPrefab != null)
                    {
                        Instantiate(impactPrefab, Hit.point, Quaternion.identity);
                        //Mermi yere vurma sesi vuraya gelicek
                    }
                }
            }

            yield return new WaitForSeconds(TrailConfig.Duration);
            yield return null;
            instance.emitting = false;
            instance.gameObject.SetActive(false);
            TrailPool.Release(instance);
        }

        private TrailRenderer CreateTrail()
        {
            GameObject Instance = new GameObject("Bullet Trail");
            TrailRenderer trail = Instance.AddComponent<TrailRenderer>();
            trail.colorGradient = TrailConfig.Color;
            trail.material = TrailConfig.Material;
            trail.widthCurve = TrailConfig.WidthCurve;
            trail.time = TrailConfig.Duration;
            trail.minVertexDistance = TrailConfig.MinVertexDistance;

            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return trail;
        }

        public object Clone()
        {
            GunScriptableObject config = CreateInstance<GunScriptableObject>();
            config.Type = Type;
            config.Place = Place;
            config.Name = Name;
            config.name = name;

            config.DamageConfig = DamageConfig.Clone() as DamageConfigScriptableObject;
            config.ShootConfig = ShootConfig.Clone() as ShootScriptableObject;
            config.AmmoConfig = AmmoConfig.Clone() as AmmoScriptableObject;
            config.TrailConfig = TrailConfig.Clone() as TrailScriptableObject;
            config.AudioConfig = AudioConfig.Clone() as AudioScriptableObject;

            config.impactPrefab = impactPrefab;
            config.ModelPrefab = ModelPrefab;
            config.PickupPrefab = PickupPrefab;
            config.SpawnPosition = SpawnPosition;
            config.SpawnRotation = SpawnRotation;

            return config;
        }
    }
}
