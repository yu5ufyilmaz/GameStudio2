using UnityEngine;
[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Gun Trail Configuration", order = 4)]
public class TrailScriptableObject : ScriptableObject
{
    public Material Material;
    public AnimationCurve WidthCurve;
    public float Duration;
    public float MinVertexDistance = 0.1f;
    public Gradient Color;


    public float MissDistance = 100f;
    public float SimulationSpeed = 100f;
}
