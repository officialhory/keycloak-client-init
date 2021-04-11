using KC.Configurator.Exceptions;
using KC.Configurator.KeycloakResponses;
using KC.Configurator.Models;
using KC.Configurator.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator
{
    class Program
    {
        static IConfiguration _config;
        static List<IOptions> _options;
        static HttpClient _httpClient;
        static List<KcConfigurationObject> _configObjects;
        static Logger _logger;
        static List<KeyValuePair<string, string>> _clientIdsWithGuids;

        static async Task Main()
        {
            await Initialization();
            var kcOpt = _options.Where(x => x.GetType() == typeof(KeycloakOption)).First() as KeycloakOption;
            FetchAppDefs(kcOpt);
            await CreateClients(kcOpt);
            await GetClientSecrets(kcOpt);
            await CreateProtocolMappers(kcOpt);
        }

        static async Task Initialization()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .CreateLogger();

            ConfigBuilder();
            _options = new List<IOptions>();
            GetOptionsFromConfig();

            var kcOpt = _options.Where(x => x.GetType() == typeof(KeycloakOption)).First() as KeycloakOption;
            SetUpHttpClient(kcOpt);
            await Authentication(_httpClient, kcOpt);

            _configObjects = new List<KcConfigurationObject>();
            _clientIdsWithGuids = new List<KeyValuePair<string, string>>();
        }

        static void ConfigBuilder()
        {
            _logger.Information("[INFO] Building configuration...");
            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", false, true);
            _config = builder.Build();
        }

        static void GetOptionsFromConfig()
        {
            _logger.Information("[INFO] Fetching application configuration...");
            var appConfig = new AppConfigOption();
            _config.GetSection(AppConfigOption.AppConfig).Bind(appConfig);
            _options.Add(appConfig);

            _logger.Information("[INFO] Fetching Keycloak related configuration...");
            var kcConfig = new KeycloakOption();
            _config.GetSection(KeycloakOption.Keyclaok).Bind(kcConfig);
            _options.Add(kcConfig);
        }

        static void SetUpHttpClient(KeycloakOption kcOpt)
        {
            _logger.Information("[INFO] Setting up HTTP Client ...");
            _httpClient = new HttpClient();

            _httpClient.BaseAddress = kcOpt.Url ??
                throw new OptionsNotFoundException("Keycloak options were not configured correctly");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        static async Task<KcAuthRespons> Authentication(HttpClient client, KeycloakOption kcOpt)
        {
            _logger.Information("[INFO] Getting auth token...");
            var body = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("username", kcOpt.Username),
                new KeyValuePair<string,string>("password", kcOpt.Password),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", "admin-cli")
            };

            using var content = new FormUrlEncodedContent(body);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var response = await client.PostAsync($"auth/realms/{kcOpt.Realm}/protocol/openid-connect/token", content);

            response.EnsureSuccessStatusCode();
            var cont = await response.Content.ReadAsStringAsync();
            _logger.Debug("{json}", JToken.Parse(cont).ToString(Formatting.Indented));
            KcAuthRespons authResp = JsonConvert.DeserializeObject<KcAuthRespons>(cont);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResp.AccessToken);
            return authResp;
        }

        static async Task GetClients(KeycloakOption kcOpt)
        {
            var response = await _httpClient.GetAsync($"auth/admin/realms/{kcOpt.Realm}/clients");
            response.EnsureSuccessStatusCode();
        }

        static async Task CreateClients(KeycloakOption kcOpt)
        {
            _logger.Information("[INFO] Creating Clients...");
            foreach (var cf in _configObjects)
            {
                var jsonStr = JsonConvert.SerializeObject(cf.AppDefinition.ClientDefinition);
                var clientId = cf.AppDefinition.ClientDefinition.ClientId;
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                _logger.Information("[INFO] Creating {client}", cf.AppDefinition.ClientDefinition.ClientId);
                var result = await _httpClient.PostAsync($"auth/admin/realms/{kcOpt.Realm}/clients", content);

                try
                {
                    if (result.StatusCode == HttpStatusCode.Conflict)
                    {
                        _logger.Warning("[WARNING] Client already exists ({client}), Skipping ...", clientId);
                        continue;
                    }
                    else
                    {
                        result.EnsureSuccessStatusCode();
                        _clientIdsWithGuids.Add(new KeyValuePair<string, string>(clientId, result.Headers.Location.Segments.Last()));
                        _logger.Information("[Success] Client {client} added, with GUID: {guid}", clientId, result.Headers.Location.Segments.Last());
                    }
                }
                catch
                {
                    _logger.Error("[ERROR] Something went wrong while adding client {client}, error code: {errorCode} ", clientId, result.StatusCode);
                }
            }
        }

        static void FetchAppDefs(KeycloakOption kcOpt)
        {
            _logger.Information("[INFO] Collecting application definitions from files...");
            var config = (_options.Where(x => x.GetType() == typeof(AppConfigOption)).First() as AppConfigOption);

            foreach (string file in Directory.EnumerateFiles(config.FolderPath, "*.json"))
            {
                string contents = File.ReadAllText(file);
                var res = JsonConvert.DeserializeObject<KcConfigurationObject>(contents);
                _configObjects.Add(res);
            }
        }

        static async Task GetClientSecrets(KeycloakOption kcOpt)
        {
            if (_clientIdsWithGuids.Count > 0)
            {
                _logger.Information("[INFO] Collecting client secrets...");
                foreach (var client in _clientIdsWithGuids)
                {
                    var response = await _httpClient.GetAsync($"auth/admin/realms/{kcOpt.Realm}/clients/{client.Value}/client-secret");
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    var parsedRes = JsonConvert.DeserializeObject<KcSecretResponse>(result);
                    _logger.Information("[SECRET] {clientId} secret: *** {secret} ***", client.Key, parsedRes.Value);
                }
            }
            else
            {
                _logger.Information("[INFO] No added clients were found, Skipping secret collection ...");
            }

        }

        static async Task GetClientData(KeycloakOption kcOpt)
        {
            foreach (var client in _clientIdsWithGuids)
            {
                var response = await _httpClient.GetAsync($"auth/admin/realms/{kcOpt.Realm}/clients/{client.Value}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                _logger.Debug("{json}", JToken.Parse(result).ToString(Formatting.Indented));
            }
        }

        static async Task CreateProtocolMappers(KeycloakOption kcOpt)
        {
            if (_clientIdsWithGuids.Count > 0)
            {
                _logger.Information("[INFO] Adding protocol mappers...");
                foreach (var client in _clientIdsWithGuids)
                {
                    var cf = _configObjects.Where(x => x.AppDefinition.ClientDefinition.ClientId == client.Key).FirstOrDefault();

                    if (cf.AppDefinition.ProtocolMapperDefinition != null)
                    {
                        var jsonStr = JsonConvert.SerializeObject(cf.AppDefinition.ProtocolMapperDefinition);
                        var clientId = cf.AppDefinition.ClientDefinition.ClientId;
                        var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync($"auth/admin/realms/{kcOpt.Realm}/clients/{client.Value}/protocol-mappers/models", content);

                        try
                        {
                            response.EnsureSuccessStatusCode();
                            _logger.Information("[Success] Protocol mapper: {mapper} added to {client} client", cf.AppDefinition.ProtocolMapperDefinition.Name, client.Key);
                        }
                        catch (System.Exception)
                        {

                            throw;
                        }

                    }
                }
            } 
            else
            {
                _logger.Information("[INFO] No added clients were found, Skipping mapper addition ...");
            }
        }
    }
}
