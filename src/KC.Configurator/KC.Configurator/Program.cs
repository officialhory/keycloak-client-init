using KC.Configurator.Exceptions;
using KC.Configurator.KeycloakResponses;
using KC.Configurator.Models;
using KC.Configurator.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KC.Configurator
{
    class Program
    {
        private static IConfiguration _config;
        private static List<IOptions> _options;
        private static HttpClient _httpClient;
        private static List<KcConfigurationObject> _configObjects;
        static async Task Main(string[] args)
        {
            await Initialization();
            Console.WriteLine((_options[0] as KeycloakOption).Url);
            var asd = (_options.Where(x => x.GetType() == typeof(KeycloakOption)).First() as KeycloakOption);
            FetchAppDefs(asd);
            await CreateClients(asd);
        }

        private static async Task Initialization()
        {
            ConfigBuilder();
            _options = new List<IOptions>();
            GetOptionsFromConfig();
            var kcOpt = (_options.Where(x => x.GetType() == typeof(KeycloakOption)).First() as KeycloakOption);

            SetUpHttpClient(kcOpt);
            await Authentication(_httpClient, kcOpt);

            _configObjects = new List<KcConfigurationObject>();
            await Task.CompletedTask;
        }

        private static void ConfigBuilder()
        {
            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", false, true);
            _config = builder.Build();
        }

        private static void GetOptionsFromConfig()
        {
            var kcConfig = new KeycloakOption();
            _config.GetSection(KeycloakOption.Keyclaok).Bind(kcConfig);
            _options.Add(kcConfig);

            var appConfig = new AppConfigOption();
            _config.GetSection(AppConfigOption.AppConfig).Bind(appConfig);
            _options.Add(appConfig);
        }

        private static void SetUpHttpClient(KeycloakOption kcOpt)
        {
            _httpClient = new HttpClient();

            _httpClient.BaseAddress = kcOpt.Url ??
                throw new OptionsNotFoundException("Keycloak options were not configured correctly");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static async Task<KcAuthRespons> Authentication(HttpClient client, KeycloakOption kcOpt)
        {
            KcAuthRespons authResp = null;
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
            authResp = JsonConvert.DeserializeObject<KcAuthRespons>(cont);


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResp.AccessToken);
            return authResp;
        }

        private static async Task GetClients(KeycloakOption kcOpt)
        {
            var response = await _httpClient.GetAsync($"auth/admin/realms/{kcOpt.Realm}/clients");
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task CreateClients(KeycloakOption kcOpt)
        {
            foreach (var cf in _configObjects)
            {
                var jsonStr = JsonConvert.SerializeObject(cf.AppDefinition.ClientDefinition);
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync($"auth/admin/realms/{kcOpt.Realm}/clients", content);
                result.EnsureSuccessStatusCode();
            }

        }

        private static void FetchAppDefs(KeycloakOption kcOpt)
        {
            var config = (_options.Where(x => x.GetType() == typeof(AppConfigOption)).First() as AppConfigOption);

            foreach (string file in Directory.EnumerateFiles(config.FolderPath, "*.json"))
            {
                string contents = File.ReadAllText(file);
                var res = JsonConvert.DeserializeObject<KcConfigurationObject>(contents);
                _configObjects.Add(res);
            }
        }
    }
}
