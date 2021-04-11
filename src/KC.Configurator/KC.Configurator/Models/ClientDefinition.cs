using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Models
{
    class ClientDefinition
    {
        public string ClientId { get; set; }
        public bool Enabled { get; set; }
        public bool ConsentRequired { get; set; } = false;
        public string Name { get; set; }
        public string Description { get; set; }
        public string RootUrl { get; set; }
        public string BaseUrl { get; set; } = "/";
        public string AdminUrl { get; set; }
        public List<string> RedirectUris { get; set; }
        public List<object> WebOrigins { get; set; }
        public bool PublicClient { get; set; } = false;
        public bool BearerOnly { get; set; } = false;
        public string Protocol { get; set; }
        public bool DirectAccessGrantsEnabled { get; set; }
        public bool AuthorizationServicesEnabled { get; set; }
        public bool StandardFlowEnabled { get; set; }
        public bool ImplicitFlowEnabled { get; set; }
        public bool ServiceAccountsEnabled { get; set; }
        public List<string> DefaultRoles { get; set; }
    }
}
