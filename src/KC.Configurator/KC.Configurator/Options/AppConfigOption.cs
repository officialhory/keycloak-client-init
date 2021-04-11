using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator.Options
{
    class AppConfigOption : IOptions
    {
        public const string AppConfig = "AppConfig";
        public string FolderPath { get; set; }
    }
}
