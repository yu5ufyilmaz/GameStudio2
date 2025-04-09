using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
namespace DotGalacticos.Guns
{
    public class Utilities
    {
        public static void CopyValues<T>(T Base, T Copy)
        {
            Type type = Base.GetType();

            foreach (FieldInfo field in type.GetFields())
            {
                field.SetValue(Copy, field.GetValue(Base));
            }
        }
    }
}
