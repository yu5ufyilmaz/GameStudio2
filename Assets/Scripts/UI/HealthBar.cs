using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider = GameObject.Find("HealthBar").GetComponent<Slider>();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
