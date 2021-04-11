using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Options
{
    class KeycloakOption : IOptions
    {
        public const string Keyclaok = "Keycloak";
        public Uri Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Realm { get; set; }
    }
}
