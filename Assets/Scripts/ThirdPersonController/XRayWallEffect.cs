using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class XRayWallEffect : MonoBehaviour
{
    public LayerMask wallLayer;
    public Color silhouetteColor = new Color(0, 0, 1, 1); // Parlak mavi
    public float emissionIntensity = 5f;

    private Camera mainCamera;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material silhouetteMaterial;
    private bool isOccluded = false;

    void Start()
    {
        mainCamera = Camera.main;
        renderers = GetComponentsInChildren<Renderer>();

        // Orijinal materyalleri sakla
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }

        // HDRP Unlit shader kullanarak silüet materyali oluştur
        silhouetteMaterial = new Material(Shader.Find("HDRP/Unlit"));
        SetupSilhouetteMaterial();
    }

    void SetupSilhouetteMaterial()
    {
        if (silhouetteMaterial != null)
        {
            // Ana renk ve özellikler
            silhouetteMaterial.SetColor("_UnlitColor", silhouetteColor);

            // Emisyon (parlaklık) ayarları
            silhouetteMaterial.EnableKeyword("_EMISSION");
            silhouetteMaterial.SetColor("_EmissiveColor", silhouetteColor * emissionIntensity);

            // Duvar arkasında görünme ayarları
            silhouetteMaterial.SetInt(
                "_ZTestDepthEqualForOpaque",
                (int)UnityEngine.Rendering.CompareFunction.Always
            );
            silhouetteMaterial.SetInt("_ZWrite", 0);

            // Yüksek render önceliği
            silhouetteMaterial.renderQueue = 3100;

            // Debug için log
            Debug.Log("Silüet materyali başarıyla oluşturuldu. Renk: " + silhouetteColor);
        }
        else
        {
            Debug.LogError("HDRP/Unlit shader bulunamadı!");
        }
    }

    void Update()
    {
        CheckIfBehindWall();
    }

    void CheckIfBehindWall()
    {
        bool wasOccluded = isOccluded;

        // Kameradan oyuncuya raycast
        Vector3 directionToPlayer = transform.position - mainCamera.transform.position;
        float distance = directionToPlayer.magnitude;

        // Duvar tespiti
        RaycastHit hit;
        if (
            Physics.Raycast(
                mainCamera.transform.position,
                directionToPlayer.normalized,
                out hit,
                distance,
                wallLayer
            )
        )
        {
            // Çarpışan nesne oyuncu değilse duvar olarak kabul et
            if (hit.collider.gameObject != this.gameObject)
            {
                isOccluded = true;
                Debug.Log("Silüet olmama sebebi: " + hit.collider.gameObject.name); // Silüet olmama sebebini logla
            }
            else
            {
                isOccluded = false;
            }
        }
        else
        {
            isOccluded = false;
        }

        // Durum değiştiyse materyalleri güncelle
        if (wasOccluded != isOccluded)
        {
            ApplyMaterials();
        }
    }

    void ApplyMaterials()
    {
        foreach (Renderer rend in renderers)
        {
            if (isOccluded)
            {
                // Duvarın arkasında - silüet materyali
                rend.material = silhouetteMaterial;
                Debug.Log("Silüet materyali uygulandı: " + silhouetteColor);
            }
            else
            {
                // Normal görünüm - orijinal materyal
                int index = System.Array.IndexOf(renderers, rend);
                rend.material = originalMaterials[index];
            }
        }
    }

    // Debug için
    void OnDrawGizmos()
    {
        if (isOccluded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
