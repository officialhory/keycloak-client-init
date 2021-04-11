using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Options
{
    public class Access
    {
        public bool view { get; set; }
        public bool configure { get; set; }
        public bool manage { get; set; }
    }

    public class Attributes
    {
    }

    public class AuthenticationFlowBindingOverrides
    {
    }

    public class ClientRepresentation
    {
        public string clientId { get; set; }
        public string name { get; set; }
        public string adminUrl { get; set; }
        public bool alwaysDisplayInConsole { get; set; }
        public Access access { get; set; }
        public Attributes attributes { get; set; }
        public AuthenticationFlowBindingOverrides authenticationFlowBindingOverrides { get; set; }
        public bool authorizationServicesEnabled { get; set; }
        public bool bearerOnly { get; set; }
        public bool directAccessGrantsEnabled { get; set; }
        public bool enabled { get; set; }
        public string protocol { get; set; }
        public string description { get; set; }
        public string rootUrl { get; set; }
        public string baseUrl { get; set; }
        public bool surrogateAuthRequired { get; set; }
        public string clientAuthenticatorType { get; set; }
        public List<string> defaultRoles { get; set; }
        public List<string> redirectUris { get; set; }
        public List<object> webOrigins { get; set; }
        public int notBefore { get; set; }
        public bool consentRequired { get; set; }
        public bool standardFlowEnabled { get; set; }
        public bool implicitFlowEnabled { get; set; }
        public bool serviceAccountsEnabled { get; set; }
        public bool publicClient { get; set; }
        public bool frontchannelLogout { get; set; }
        public bool fullScopeAllowed { get; set; }
        public int nodeReRegistrationTimeout { get; set; }
        public List<string> defaultClientScopes { get; set; }
        public List<string> optionalClientScopes { get; set; }
    }
}
