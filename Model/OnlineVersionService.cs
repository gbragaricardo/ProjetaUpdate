using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Windows.Controls;
using System.Linq;

namespace ProjetaUpdate
{
    public class OnlineVersionService : INotifyPropertyChanged
    {

        public Version TypeVLatest { get; set; }

        private readonly string _gitubRealeses;
        private string _githubApiUrl;
        private readonly string _addinName;
        private readonly string _myAddinPath;
        private readonly string _revitAddinPath;
        string _versionTag;
        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OnlineVersionService(string AddinName)
        {
            _addinName = AddinName;
            _revitAddinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "Revit", "Addins", "2024");
            _myAddinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Autodesk","Revit","Addins","2024",_addinName);
            _gitubRealeses = $"https://api.github.com/repos/gbragaricardo/{_addinName}/releases";
        }



        public async Task<(string latestVersion, List<string> lastFourVersions)> ObterVersoesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                await AtualizarStatusComDelay("Buscando Versoes");

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0"); // GitHub exige um User-Agent
                client.Timeout = TimeSpan.FromMinutes(10);

                try
                {
                    string response = await client.GetStringAsync(_gitubRealeses);
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

                        if (latestVersion != null)
                        {
                            TypeVLatest = new Version(latestVersion);
                        }
                        else
                        {
                            TypeVLatest = new Version("0.0.0.0");
                        }
                        

                        return (latestVersion, versoes);
                    }
                }

                catch (Exception ex)
                {
                    TypeVLatest = new Version("0.0.0.0");
                    Debug.WriteLine($"Erro ao buscar versões: {ex.Message}");
                    await AtualizarStatusComDelay("Erro ao buscar versoes");

                    return ("-", new List<string>()); // Retorna null e lista vazia em caso de erro
                }
            }
        }

        public async Task InstalarAddinAsync(string VersionTag)
        {
            try
            {
                _versionTag = VersionTag;

                if (_versionTag == null)
                    return;

                // Define a URL da API para obter informações da versão
                _githubApiUrl = _versionTag.ToLower() == "latest" ? $"https://api.github.com/repos/gbragaricardo/{_addinName}/releases/latest"
                                                                  : $"https://api.github.com/repos/gbragaricardo/{_addinName}/releases/tags/{_versionTag}";

                string versionUrl = null;
                string tempZipPath = Path.Combine(Path.GetTempPath(), $"{_addinName}.zip");
                string tempExtractPath = Path.Combine(Path.GetTempPath(), $"{_addinName}_temp");

                Debug.WriteLine("Obtendo URL da última/Tag versão...");
                await AtualizarStatusComDelay("Sincronizando versão...");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    client.Timeout = TimeSpan.FromMinutes(10);

                    string jsonResponse = await client.GetStringAsync(_githubApiUrl);
                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        JsonElement root = doc.RootElement;

                        // Busca no array "assets" um arquivo .zip
                        if (root.TryGetProperty("assets", out JsonElement assetsArray))
                        {
                            foreach (JsonElement asset in assetsArray.EnumerateArray())
                            {
                                if (asset.TryGetProperty("name", out JsonElement name) &&
                                    name.GetString().EndsWith(".zip")) // Filtra apenas arquivos ZIP
                                {
                                    versionUrl = asset.GetProperty("browser_download_url").GetString();
                                    break; // Para no primeiro ZIP encontrado
                                }
                            }
                        }
                    }

                    // Verifica se conseguiu obter o link do ZIP
                    if (string.IsNullOrEmpty(versionUrl))
                    {
                        Debug.WriteLine("Nenhum arquivo ZIP encontrado na release.");
                        await AtualizarStatusComDelay("Erro: Nenhum arquivo ZIP encontrado.");
                        return;
                    }

                    Debug.WriteLine("Baixando arquivo ZIP...");
                    await AtualizarStatusComDelay("Baixando arquivos...");

                    byte[] zipData = await client.GetByteArrayAsync(versionUrl);
                    File.WriteAllBytes(tempZipPath, zipData);

                    await AtualizarStatusComDelay("Download concluído");
                }

                // Remove pasta temporária se já existir
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }

                Debug.WriteLine("Extraindo arquivos...");
                await AtualizarStatusComDelay("Extraindo arquivos...");

                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

                // Encontra a pasta correta dentro do ZIP extraído
                string[] extractedDirs = Directory.GetDirectories(tempExtractPath);

                string extractedAddinPath = Array.Find(extractedDirs, dir => Path.GetFileName(dir) == _addinName);

                if (extractedAddinPath == null)
                {
                    Debug.WriteLine($"Pasta {_addinName} não encontrada no ZIP extraído.");
                    await AtualizarStatusComDelay($"Erro: Pasta {_addinName} não encontrada.");
                    return;
                }

                // Se a versão antiga existir, movê-la para a pasta "VersaoAnterior"
                if (Directory.Exists(_myAddinPath))
                {
                    Console.WriteLine("Excluindo Versão Anterior...");
                    await AtualizarStatusComDelay("Substituindo arquivos...");

                    Directory.Delete(_myAddinPath, true);
                }

                // Move a nova versão para o diretório do Revit Addins
                string addinFile = Directory.GetFiles(extractedAddinPath, "*.addin").FirstOrDefault();

                if (addinFile == null)
                {
                    Console.WriteLine($"Falha ao encontrar {_addinName}.addin");
                    return;
                }
                
                // Move o arquivo .addin diretamente para a pasta raiz de add-ins do Revit
                File.Move(addinFile, Path.Combine(_revitAddinPath, Path.GetFileName(addinFile)));          

                Console.WriteLine("Instalando nova versão...");
                await AtualizarStatusComDelay("Instalando nova versão...");
                Directory.Move(extractedAddinPath, _myAddinPath);

                // Limpeza de arquivos temporários
                File.Delete(tempZipPath);
                Directory.Delete(tempExtractPath, true);

                Console.WriteLine("Instalação concluída com sucesso.");
                await AtualizarStatusComDelay("Instalação concluída com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na instalação: {ex.Message}");
                await AtualizarStatusComDelay($"Erro na instalação: {ex.Message}");
            }
        }

        public async Task<int> VersionCompare(Version VInstall, Version VLatest)
        {
            int compareResult;

            try
            {
                
                if ((VLatest == null && VInstall == null) && (VLatest.ToString() == "0.0.0.0" && VInstall.ToString() == "0.0.0.0"))
                {
                    await AtualizarStatusComDelay("Bad Request");
                    compareResult = 0;
                    return compareResult;
                }

                else if (VLatest.ToString() == "0.0.0.0" || VLatest == null)
                {
                    await AtualizarStatusComDelay("Erro ao obter nova versao");
                    compareResult = 1;
                    return compareResult;
                }

                else if (VInstall == null || VInstall.ToString() == "0.0.0.0")
                {

                    await AtualizarStatusComDelay("Download disponivel");
                    compareResult = 2;
                    return compareResult;
                }

                else
                {
                    int compare = VInstall.CompareTo(VLatest);
                    if (compare == 0)
                    {
                        await AtualizarStatusComDelay("Versão atualizada!");
                        compareResult = 3;
                    }
                    else if (compare < 0)
                    {
                        await AtualizarStatusComDelay($"Nova versão disponível: {VLatest}");
                        compareResult = 4;
                    }
                    else // comparacao > 0
                    {
                        await AtualizarStatusComDelay("Versão Beta");
                        compareResult = 5;
                    }

                    return compareResult;
                }
            }
            catch (Exception ex)
            {
                await AtualizarStatusComDelay($"Error: {ex.Message}");
                compareResult = 0;
                return compareResult;
            }
        }

        public async Task<(bool, bool)> CompareResult(Version VInstall, Version VLatest)
        {
            bool buttonInstall = false;
            bool buttonAtt = false;

            switch (await VersionCompare(VInstall, VLatest))
            {
                case 0: break;
                case 1: break;
                case 2: buttonInstall = true; break;
                case 3: buttonInstall = true; break;
                case 4: buttonInstall = true;
                        buttonAtt = true;
                        break;

                case 5: buttonInstall = true; break;

                default: break;   
            }


            return(buttonInstall, buttonAtt);
        }

        private async Task AtualizarStatusComDelay(string novaMensagem, int delayMs = 500)
        {
            await Task.Delay(delayMs); // Aguarda o tempo definido sem bloquear a UI
            StatusMessage = novaMensagem; // Atualiza a mensagem após o tempo de espera
        }
    }
}
