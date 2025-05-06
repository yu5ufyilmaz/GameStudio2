using UnityEngine;

namespace DotGalacticos.Guns
{
    [CreateAssetMenu(
        fileName = "Trail Config",
        menuName = "Guns/Gun Trail Configuration",
        order = 4
    )]
    public class TrailScriptableObject : ScriptableObject, System.ICloneable
    {
        public Material Material;
        public AnimationCurve WidthCurve;
        public float Duration;
        public float MinVertexDistance = 0.1f;
        public Gradient Color;

        public float MissDistance = 100f;
        public float SimulationSpeed = 100f;

        public object Clone()
        {
            TrailScriptableObject config = CreateInstance<TrailScriptableObject>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}
