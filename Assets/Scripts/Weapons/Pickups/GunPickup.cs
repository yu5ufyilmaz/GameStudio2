using UnityEngine;
namespace DotGalacticos.Guns.Demo
{
public class GunPickup : MonoBehaviour
{
   public GunScriptableObject Gun;
   public Vector3 SpinDirection = Vector3.up;

       void Update()
        {
            //transform.Rotate(SpinDirection);
        }

    }
}

