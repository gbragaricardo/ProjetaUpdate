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
