using System;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
namespace DotGalacticos.Guns.Modifiers
{
    public abstract class AbstractValueModifier<T> : IModifiers
    {
        public string AttributeName;
        public T Amount;
        public abstract void Apply(GunScriptableObject Gun);

        protected FieldType GetAttribute<FieldType>(
            GunScriptableObject Gun,
            out object TargetObject,
            out FieldInfo Field)
        {
            string[] paths = AttributeName.Split('/');
            string attribute = paths[paths.Length - 1];

            Type type = Gun.GetType();
            object target = Gun;

            for (int i = 0; i < paths.Length - 1; i++)
            {
                FieldInfo field = type.GetField(paths[i]);
                if (field == null)
                {
                    UnityEngine.Debug.LogError($"Field {paths[i]} not found in {type.Name}");
                    throw new InvalidPathSpecifiedException(AttributeName);
                }
                else
                {
                    target = field.GetValue(target);
                    type = target.GetType();
                }
            }

            FieldInfo attributeField = type.GetField(attribute);
            if (attributeField == null)
            {
                UnityEngine.Debug.LogError($"Field {AttributeName} not found in {type.Name}");
                throw new InvalidPathSpecifiedException(AttributeName);
            }

            Field = attributeField;
            TargetObject = target;
            return (FieldType)attributeField.GetValue(target);
        }

    }
}
