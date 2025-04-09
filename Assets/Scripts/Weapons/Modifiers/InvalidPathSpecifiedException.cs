using System;
using UnityEngine;

namespace DotGalacticos.Guns
{
    public class InvalidPathSpecifiedException : Exception
    {
        public InvalidPathSpecifiedException(string Attribute)
            : base($"{Attribute} does not exist") { }
    }
}
