using UnityEngine;

[System.Serializable]
public class AmmoDrop
{
    public GameObject ammoPrefab; // Düşürülecek mermi prefab'ı

    [Range(0, 100)]
    public float dropChance; // Düşme olasılığı
}
