using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundEngine
{
    public class ServiceDependencyAttribute : Attribute
    {
        public bool Optional = false;
    }
}
