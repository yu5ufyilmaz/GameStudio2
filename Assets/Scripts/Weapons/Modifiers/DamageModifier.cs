using System.Reflection;
using UnityEngine;
using static UnityEngine.ParticleSystem;
namespace DotGalacticos.Guns.Modifiers
{
    public class DamageModifier : AbstractValueModifier<float>
    {
        public override void Apply(GunScriptableObject Gun)
        {
            try
            {
                MinMaxCurve damageCurve = GetAttribute<MinMaxCurve>(Gun, out object target, out FieldInfo field);
                switch (damageCurve.mode)
                {
                    case UnityEngine.ParticleSystemCurveMode.TwoConstants:
                        damageCurve.constantMin += Amount;
                        damageCurve.constantMax += Amount;
                        break;
                    case UnityEngine.ParticleSystemCurveMode.TwoCurves:
                        damageCurve.curveMultiplier *= Amount;
                        break;
                    case UnityEngine.ParticleSystemCurveMode.Curve:
                        damageCurve.curveMultiplier *= Amount;
                        break;
                    case UnityEngine.ParticleSystemCurveMode.Constant:
                        damageCurve.constant *= Amount;
                        break;

                }
                field.SetValue(target, damageCurve);
            }
            catch (InvalidPathSpecifiedException)
            { }
        }
    }
}