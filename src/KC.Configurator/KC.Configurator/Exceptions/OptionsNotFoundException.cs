using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Exceptions
{
    class OptionsNotFoundException : Exception
    {
        public OptionsNotFoundException() {}

        public OptionsNotFoundException(string message) : base(message) {}

        public OptionsNotFoundException(string message, Exception inner) : base(message, inner) {}
    }
}
