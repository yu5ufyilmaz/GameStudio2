using System.Collections;
using DotGalacticos.Guns.Demo;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace DotGalacticos
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("Hareket girişini yumuşatma hızını ayarlayın")]
        public float SmoothInputSpeed = 0.2f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Range(0, 1)]
        public float DieAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Header("Death Video")]
        [Tooltip("GameObject with VideoPlayer component for death sequence")]
        public GameObject deathVideoObject;

        [Tooltip("UI RawImage to display the death video (optional)")]
        public UnityEngine.UI.RawImage deathVideoRawImage;

        private VideoPlayer videoPlayer;

        [Space(10)]
        [Tooltip(
            "Time required to pass before being able to jump again. Set to 0f to instantly jump again"
        )]
        public float JumpTimeout = 0.50f;

        [Tooltip(
            "Time required to pass before entering the fall state. Useful for walking down stairs"
        )]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip(
            "If the character is grounded or not. Not part of the CharacterController built in grounded check"
        )]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip(
            "The radius of the grounded check. Should match the radius of the CharacterController"
        )]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip(
            "The follow target set in the Cinemachine Virtual Camera that the camera will follow"
        )]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip(
            "Additional degress to override the camera. Useful for fine tuning camera position when locked"
        )]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [SerializeField]
        AnimationCurve dodgeCurve;

        [SerializeField]
        AnimationCurve jumpAwayCurve;

        [SerializeField]
        AudioClip dodgeAuido;

        [SerializeField]
        AudioClip[] DieAudioClips;

        // bool isDodging;
        bool isJumpAway;
        float dodgeTimer;
        public bool isRunning => _input != null && _input.sprint && _input.move != Vector2.zero;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;

        // private float _animationBlendZ;
        // private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeedX;
        private int _animIDSpeedZ;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        //Door Interaction
        public float interactionRadius = 2f; // Oyuncunun etkileşim alanı
        private Transform currentDoor; // Şu an etkileşimde olduğumuz kapı
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        public StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private ShootController _shootController;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        private bool _isDying = false;

        public bool IsAiming { get; set; }

        //Animation Rig Variables
        private Rig _aimRig;
        private float targetWeight; // Hedef ağırlık
        private float currentWeight; // Mevcut ağırlık
        private float weightChangeRate = 5f; // Ağırlık değişim hızı

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _shootController = GetComponent<ShootController>();
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _aimRig = GameObject.Find("Aim Rig").GetComponent<Rig>();

            // Initialize VideoPlayer reference if deathVideoObject is assigned
            if (deathVideoObject != null)
            {
                videoPlayer = deathVideoObject.GetComponent<VideoPlayer>();
                if (videoPlayer != null)
                {
                    // Add completion event listener
                    videoPlayer.loopPointReached += OnVideoFinished;

                    // Initially make sure the video is not playing
                    videoPlayer.Stop();

                    // Configure the RawImage as the target texture display if available
                    if (deathVideoRawImage != null)
                    {
                        // Set the video to render to the RawImage texture
                        videoPlayer.targetTexture = new RenderTexture(
                            (int)videoPlayer.clip.width,
                            (int)videoPlayer.clip.height,
                            24
                        );
                        deathVideoRawImage.texture = videoPlayer.targetTexture;

                        // Hide RawImage initially
                        deathVideoRawImage.gameObject.SetActive(false);
                    }
                }
            }

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError(
                "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it"
            );
#endif

            AssignAnimationIDs();
            Keyframe dodge_lastFrame = dodgeCurve[dodgeCurve.length - 1];
            dodgeTimer = dodge_lastFrame.time;
            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Time.timeScale = 0f;
        }

        private void Update()
        {
            // Don't process movement if dying
            if (_isDying)
                return;

            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            if (isJumpAway == false)
                Move();

            //Dodge();
            OpenDoor();

            UpdateAimRigWeight();
        }

        private void LateUpdate()
        {
            // Don't process camera rotation if dying
            if (_isDying)
                return;

            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeedX = Animator.StringToHash("Speed X");
            _animIDSpeedZ = Animator.StringToHash("Speed Z");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );
            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(
                _cinemachineTargetYaw,
                float.MinValue,
                float.MaxValue
            );
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private float _animationBlendSmoothVelocity; // Animasyon blend yumuşatma için değişken
        private float _speedSmoothVelocity; // Hız yumuşatma için değişken
        private float _rotationSmoothVelocity; // Dönüş yumuşatma için değişken

        private void Move()
        {
            // Hedef hız hesaplama - Nişan alırken koşma hızı uygulanmaz
            float targetSpeed =
                (_input.sprint && !_shootController.isAiming) ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero)
                targetSpeed = 0.0f;

            // Input miktarını hesapla
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // Hız yumuşatma
            _speed = Mathf.SmoothDamp(
                _speed,
                targetSpeed * inputMagnitude,
                ref _speedSmoothVelocity,
                SpeedChangeRate * Time.deltaTime
            );

            // Animasyon blend yumuşatma
            _animationBlend = Mathf.SmoothDamp(
                _animationBlend,
                targetSpeed,
                ref _animationBlendSmoothVelocity,
                SmoothInputSpeed * Time.deltaTime
            );

            // Hareket yönünü hesapla (Top-down kontroller - kamera bağımsız)
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // İlerleme yönü kontrolü
            if (inputDirection.magnitude > 0)
            {
                // Top-down hareket için kamera bağımsız - HATRED tarzı
                Vector3 moveDirection = Vector3.zero;

                // WASD her zaman aynı dünya yönlerini kontrol eder
                moveDirection.x = _input.move.x; // A-D sağ-sol
                moveDirection.z = _input.move.y; // W-S ileri-geri
                moveDirection.Normalize();

                // Hareket vektörünü hesapla
                Vector3 moveVector =
                    moveDirection * (_speed * Time.deltaTime)
                    + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

                _controller.Move(moveVector);

                // Karakter dönüşünü yönet
                float targetRotation = 0f;

                if (_shootController.isAiming)
                {
                    // Nişan alıyorsa nişan açısını kullan
                    targetRotation = _shootController._lastAimAngle;
                }
                else
                {
                    // Nişan almıyorsa hareket yönüne dön
                    targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                }

                // Dönüş yumuşatma
                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetRotation,
                    ref _rotationSmoothVelocity,
                    RotationSmoothTime
                );

                // Dönüşü uygula
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                // Animator güncelleme
                if (_hasAnimator)
                {
                    Vector3 localVelocity = transform.InverseTransformDirection(moveDirection);
                    float speedX = localVelocity.x * _speed;
                    float speedZ = localVelocity.z * _speed;

                    _animator.SetFloat("Speed X", speedX);
                    _animator.SetFloat("Speed Z", speedZ);
                    _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                }
            }
            else
            {
                // Hareket yoksa sadece yerinde dönüş yapabilir (nişan alırken)
                if (_shootController.isAiming)
                {
                    float rotation = Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        _shootController._lastAimAngle,
                        ref _rotationSmoothVelocity,
                        RotationSmoothTime * 0.5f // Yerinde dönüş için daha hızlı
                    );

                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                // Yerçekimi etkisi
                Vector3 moveVector = new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
                _controller.Move(moveVector);

                // Animator sıfırlama
                if (_hasAnimator)
                {
                    _animator.SetFloat("Speed X", 0);
                    _animator.SetFloat("Speed Z", 0);
                    _animator.SetFloat(_animIDMotionSpeed, 0);
                }
            }
        }

        private void UpdateAimRigWeight()
        {
            // Koşma durumuna göre Aim Rig ağırlığını güncelle
            if (isRunning)
            {
                // Koşarken ağırlığı 0'a doğru azalt
                targetWeight = Mathf.Lerp(currentWeight, 0f, Time.deltaTime * weightChangeRate);
            }
            else
            {
                // Koşmadığında ağırlığı 1'e doğru artır
                targetWeight = Mathf.Lerp(currentWeight, 1f, Time.deltaTime * weightChangeRate);
            }

            // Ağırlığı güncelle
            currentWeight = targetWeight;
            _aimRig.weight = currentWeight;
        }

        private void OpenDoor()
        {
            // Oyuncunun etrafındaki kapıları kontrol et
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius);
            currentDoor = null; // Her frame'de kapıyı sıfırla

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Door"))
                {
                    currentDoor = hitCollider.transform; // Kapıyı kaydet
                    break; // İlk kapıyı bulduktan sonra döngüyü kır
                }
            }

            // Eğer bir kapı varsa ve E tuşuna basıldıysa
            if (currentDoor != null && Input.GetKeyDown(KeyCode.E))
            {
                ToggleDoor(currentDoor); // Kapıyı aç/kapa
            }
        }

        private void ToggleDoor(Transform door)
        {
            DoorController doorController = door.GetComponent<DoorController>();
            if (doorController != null)
            {
                doorController.ToggleDoor(transform.position); // Oyuncunun pozisyonunu geçir
            }
        }

        IEnumerator JumpAwayRoutine(bool isRight)
        {
            FootstepAudioVolume = PlayerPrefs.GetFloat("SFXVolume") * 0.5f;
            if (isJumpAway)
                yield break; // Eğer zaten jumping durumundaysak, işlemi durdur

            if (isRight)
            {
                _animator.SetTrigger("JumpAwayRight");
                _controller.center = new Vector3(0, 0.25f, 0f);
                _controller.height = 0.72f;
            }
            else
            {
                _animator.SetTrigger("JumpAwayLeft");
                _controller.center = new Vector3(0, 0.25f, 0f);
                _controller.height = 0.72f;
            }
            isJumpAway = true;

            _shootController.DodgeIK(0f);

            float timer = 0;
            AudioSource.PlayClipAtPoint(
                dodgeAuido,
                transform.TransformPoint(_controller.center),
                FootstepAudioVolume
            );

            while (timer < dodgeTimer)
            {
                float curveSpeed = jumpAwayCurve.Evaluate(timer);

                Vector3 dir = (isRight ? transform.right : -transform.right);

                Vector3 moveVector =
                    dir * (_speed * Time.deltaTime * curveSpeed)
                    + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
                _controller.Move(moveVector);
                timer += Time.deltaTime;
                yield return null;
            }

            // Atlamadan sonra durumu sıfırla
        }

        public void SetJumpingBool()
        {
            if (isJumpAway == true)
            {
                _shootController.DodgeIK(1f);
                _controller.height = 1.8f;
                _controller.center = new Vector3(0f, 0.9f, 0f);

                isJumpAway = false;
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;
            if (lfAngle > 360f)
                lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded)
                Gizmos.color = transparentGreen;
            else
                Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }

        private void OnFootStep(AnimationEvent animationEvent)
        {
            FootstepAudioVolume = PlayerPrefs.GetFloat("SFXVolume") * 0.5f;
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(
                        FootstepAudioClips[index],
                        transform.TransformPoint(_controller.center),
                        FootstepAudioVolume
                    );
                }
            }
        }

        private void OnDieSound(AnimationEvent animationEvent)
        {
            DieAudioVolume = PlayerPrefs.GetFloat("SFXVolume") * 0.5f;
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (DieAudioClips.Length > 0)
                {
                    var index = Random.Range(0, DieAudioClips.Length);
                    AudioSource.PlayClipAtPoint(
                        DieAudioClips[index],
                        transform.TransformPoint(_controller.center),
                        DieAudioVolume
                    );
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            FootstepAudioVolume = PlayerPrefs.GetFloat("SFXVolume") * 0.5f;
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(
                    LandingAudioClip,
                    transform.TransformPoint(_controller.center),
                    FootstepAudioVolume
                );
            }
        }

        // Video bittiğinde çağrılacak metod
        private void OnVideoFinished(VideoPlayer vp)
        {
            // Video tamamlandığında StartMenu sahnesine geç
            if (_isDying)
            {
                // RawImage'i gizle (eğer kullanılıyorsa)
                if (deathVideoRawImage != null)
                {
                    deathVideoRawImage.gameObject.SetActive(false);
                }

                SceneManager.LoadScene("StartMenu");
                Cursor.visible = true;
            }
        }

        // Ölüm actions burada oluyor.
        public void Die()
        {
            // Video yoksa doğrudan Dying metodunu çağır
            Invoke("Dying", 1f);
        }

        public void Dying()
        {
            // Eğer zaten ölüyorsa işlem yapma
            if (_isDying)
                return;

            _isDying = true;

            // Eğer video player varsa
            if (videoPlayer != null)
            {
                // Input'u devre dışı bırak
                if (_input != null)
                {
                    _input.enabled = false;
                }

                // Karakter kontrollerini devre dışı bırak
                if (_controller != null)
                {
                    _controller.enabled = false;
                }

                // VideoPlayer'ı görünür ve aktif hale getir (gerekiyorsa)
                if (deathVideoObject != null && !deathVideoObject.activeSelf)
                {
                    deathVideoObject.SetActive(true);
                }

                // RawImage'i etkinleştir (eğer kullanılıyorsa)
                if (deathVideoRawImage != null)
                {
                    deathVideoRawImage.gameObject.SetActive(true);
                }

                // Videoyu oynat
                videoPlayer.Play();

                // Fare imlecini görünür yap
                Cursor.visible = true;
            }
        }
    }
}
