using DotGalacticos.Guns.Demo;
using UnityEditor;
using UnityEngine;

namespace DotGalacticos.EditorSettings
{
    [CustomEditor(typeof(GunPickup))]
    public class GunPickupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GunPickup gunPickup = (GunPickup)target;

            // Gun referansı varsa, ClipSize ve MaxAmmo değerlerini göster
            if (gunPickup.Gun != null)
            {
                EditorGUILayout.LabelField("Current Clip Ammo", $"{gunPickup.CurrentClipAmmo} / {gunPickup.Gun.AmmoConfig.ClipSize}");
                EditorGUILayout.LabelField("Current Ammo", $"{gunPickup.CurrentAmmo} / {gunPickup.Gun.AmmoConfig.MaxAmmo}");
            }
            else
            {
                EditorGUILayout.LabelField("Current Clip Ammo", $"{gunPickup.CurrentClipAmmo}");
                EditorGUILayout.LabelField("Current Ammo", $"{gunPickup.CurrentAmmo}");
            }
            // Diğer alanları çiz
            DrawDefaultInspector();
        }
    }
}
