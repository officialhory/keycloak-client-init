using KC.Configurator.Exceptions;
using KC.Configurator.KeycloakResponses;
using KC.Configurator.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KC.Configurator
{
    class Program
    {
        private static IConfiguration _config;
        private static List<IOptions> _options;
        private static HttpClient _httpClient;
        static async Task Main(string[] args)
        {
            await Initialization();
            Console.WriteLine((_options[0] as KeycloakOption).Url);
        }

        private static async Task Initialization()
        {
            ConfigBuilder();
            _options = new List<IOptions>();
            GetOptionsFromConfig();
            var kcOpt = (_options.Where(x => x.GetType() == typeof(KeycloakOption)).First() as KeycloakOption);

            SetUpHttpClient(kcOpt);
            await Authenticate(_httpClient, kcOpt);

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

        private static async Task<KcAuthRespons> Authenticate(HttpClient client, KeycloakOption kcOpt)
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
            

            var response = await client.PostAsync("auth/realms/master/protocol/openid-connect/token", content);
            
            if (response.IsSuccessStatusCode)
            {
                var cont = await response.Content.ReadAsStringAsync();
                Console.WriteLine(cont);
                authResp = JsonConvert.DeserializeObject<KcAuthRespons>(cont);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResp.AccessToken);
            return authResp;
        }
    }
}
