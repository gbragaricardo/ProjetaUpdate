using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetaUpdate
{
    internal class OnlineVersionService
    {

        private readonly string _githubApiUrl;
        private readonly string _addinName;

        public OnlineVersionService(string AddinName)
        {
            _addinName = AddinName;

            _githubApiUrl = $"https://api.github.com/repos/gbragaricardo/{_addinName}/releases/latest";
        }

        public async Task<(string latestVersion, List<string> lastFourVersions)> ObterVersoesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0"); // GitHub exige um User-Agent

                try
                {
                    string response = await client.GetStringAsync(_githubApiUrl);
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        List<string> versoes = new List<string>();
                        string latestVersion = null;

                        foreach (JsonElement release in doc.RootElement.EnumerateArray())
                        {
                            if (release.TryGetProperty("tag_name", out JsonElement tag))
                            {
                                string version = tag.GetString();
                                if (latestVersion == null)
                                {
                                    latestVersion = version; // Primeira versão encontrada é a mais recente
                                }

                                versoes.Add(version);
                            }

                            // Pegar apenas as últimas 4 versões
                            if (versoes.Count >= 4)
                                break;
                        }

                        return (latestVersion, versoes);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao buscar versões: {ex.Message}");
                    return (null, new List<string>()); // Retorna null e lista vazia em caso de erro
                }
            }
        }

        public async Task<string> ObterVersaoMaisRecenteAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                try
                {
                    string response = await client.GetStringAsync(_githubApiUrl);
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        return doc.RootElement.GetProperty("tag_name").GetString();
                    }
                }
                catch
                {
                    return "Erro ao obter versão";
                }
            }
        }

    }
}
