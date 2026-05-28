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
    public class OnlineVersionService
    {

        private string _githubApiUrl;
        private string _revitAddinPath;
        public string SelectedVersionTag { get; set; }
        public Version LatestTypeOfVersion { get; set; }
        public string LatestVersion { get; set; }
        public string GitHubReleases { get; set; }
        public string MyAddinPath { get; set; }
        public string VersionTag { get; set; }
        public string AddinName { get; set; }
        public string SelectedRevitVersion { get; set; }

        public List<string> LastVersions { get; set; }

        public OnlineVersionService(string addinName, string revitVersion)
        {
            SelectedRevitVersion = revitVersion;
            AddinName = addinName;
            UpdateProps();

        }



        public void UpdateProps()
        {
            _revitAddinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "Revit", "Addins", SelectedRevitVersion);
            MyAddinPath = Path.Combine(_revitAddinPath, AddinName);
            GitHubReleases = $"https://api.github.com/repos/gbragaricardo/{AddinName}/releases";
        }


        public async Task<(string latestVersion, List<string> lastFourVersions)> ObterVersoesAsync(IProgress<string> statusProgress = null)
        {
            UpdateProps();

            using (HttpClient client = new HttpClient())
            {
                await DelayMessage(statusProgress, "Buscando Versoes");

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0"); // GitHub exige um User-Agent
                client.Timeout = TimeSpan.FromMinutes(10);

                try
                {
                    string response = await client.GetStringAsync(GitHubReleases);
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        LastVersions = new List<string>();
                        LatestVersion = null;
                        
                        foreach (JsonElement release in doc.RootElement.EnumerateArray())
                        {
                            if (release.TryGetProperty("tag_name", out JsonElement tag))
                            {
                                string version = tag.GetString();
                                 if (LatestVersion == null)
                                {
                                    LatestVersion = version; // Primeira versão encontrada é a mais recente
                                }

                                LastVersions.Add(version);
                            }

                            // Pegar apenas as últimas 4 versões
                            if (LastVersions.Count >= 4)
                                break;
                        }

                        if (LatestVersion != null)
                        {
                            LatestTypeOfVersion = new Version(LatestVersion);
                        }
                        else
                        {
                            LatestTypeOfVersion = new Version("0.0.0.0");
                        }
                        

                        return (LatestVersion, LastVersions);
                    }
                }

                catch (Exception ex)
                {
                    LatestTypeOfVersion = new Version("0.0.0.0");
                    Debug.WriteLine($"Erro ao buscar versões: {ex.Message}");
                    await DelayMessage(statusProgress, "Erro ao buscar versoes");
                    return ("-", new List<string>()); // Retorna null e lista vazia em caso de erro
                }
            }
        }

        public async Task InstalarAddinAsync(string versionTag, IProgress<string> statusProgress = null)
        {
            try
            {
                VersionTag = versionTag;

                if (VersionTag == null)
                    return;

                // Define a URL da API para obter informações da versão
                _githubApiUrl = VersionTag.ToLower() == "latest" ? $"{GitHubReleases}/latest"
                                                                 : $"{GitHubReleases}/tags/{VersionTag}";

                string versionUrl = null;

                string tempZipPath = Path.Combine(Path.GetTempPath(), $"{AddinName}.zip");
                // Verifica se o arquivo ZIP já existe no diretório temporário e o exclui
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }

                string tempExtractPath = Path.Combine(Path.GetTempPath(), $"{AddinName}_temp");

                Debug.WriteLine("Obtendo URL da última/Tag versão...");
                await DelayMessage(statusProgress, "Sincronizando versão...");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    client.Timeout = TimeSpan.FromMinutes(10);

                    string jsonResponse = await client.GetStringAsync(_githubApiUrl);
                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        JsonElement root = doc.RootElement;

                        // Busca no array "assets" um arquivo .zip que corresponda à versão do Revit
                        if (root.TryGetProperty("assets", out JsonElement assetsArray))
                        {
                            var assets = assetsArray.EnumerateArray().ToList();
                            
                            // 1. Tenta encontrar um ZIP que contenha o nome do addin e a versão do Revit (ex: ProjetaHDR_2026.zip)
                            JsonElement? targetAsset = assets.Cast<JsonElement?>().FirstOrDefault(a => 
                                a.Value.TryGetProperty("name", out JsonElement name) &&
                                name.GetString().EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                                name.GetString().Contains(SelectedRevitVersion));

                            // 2. Fallback: se não encontrar, pega o primeiro ZIP que encontrar (comportamento antigo)
                            if (targetAsset == null)
                            {
                                targetAsset = assets.Cast<JsonElement?>().FirstOrDefault(a =>
                                    a.Value.TryGetProperty("name", out JsonElement name) &&
                                    name.GetString().EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
                            }

                            if (targetAsset != null)
                            {
                                versionUrl = targetAsset.Value.GetProperty("browser_download_url").GetString();
                            }
                        }
                    }

                    // Verifica se conseguiu obter o link do ZIP
                    if (string.IsNullOrEmpty(versionUrl))
                    {
                        Debug.WriteLine("Nenhum arquivo ZIP compatível encontrado na release.");
                        await DelayMessage(statusProgress, "Erro: Arquivo ZIP não encontrado.");
                        return;
                    }

                    Debug.WriteLine("Baixando arquivo ZIP...");
                    await DelayMessage(statusProgress, "Baixando arquivos...");

                    byte[] zipData = await client.GetByteArrayAsync(versionUrl);
                    File.WriteAllBytes(tempZipPath, zipData);

                    await DelayMessage(statusProgress, "Download concluído");
                }

                // Remove pasta temporária se já existir
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }

                Debug.WriteLine("Extraindo arquivos...");
                await DelayMessage(statusProgress, "Extraindo arquivos...");

                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

                // --- Lógica de Busca Inteligente ---
                // 1. Tenta encontrar uma pasta com o nome da versão do Revit (ex: "2026") em qualquer lugar do ZIP
                string versionDir = Directory.GetDirectories(tempExtractPath, SelectedRevitVersion, SearchOption.AllDirectories).FirstOrDefault();
                
                // Define onde vamos começar a procurar os arquivos do addin
                // Se achou a pasta "2026", procuramos dentro dela. Se não, procuramos no ZIP todo.
                string searchRoot = versionDir ?? tempExtractPath;

                // 2. Encontra a pasta do Addin (ex: "ProjetaHDR")
                string extractedAddinPath = Directory.GetDirectories(searchRoot, AddinName, SearchOption.AllDirectories).FirstOrDefault();

                // 3. Localiza o arquivo .addin (Prioriza o que tem o nome do addin)
                string addinFile = Directory.GetFiles(searchRoot, $"{AddinName}.addin", SearchOption.AllDirectories).FirstOrDefault();
                
                // Fallback: se não encontrar com o nome exato, pega o primeiro .addin que aparecer na pasta da versão
                if (addinFile == null)
                {
                    addinFile = Directory.GetFiles(searchRoot, "*.addin", SearchOption.AllDirectories).FirstOrDefault();
                }

                // Caso especial: Se a pasta da versão (ex: "2026") contiver os arquivos diretamente 
                // e não uma subpasta com o nome do addin
                if (extractedAddinPath == null && versionDir != null)
                {
                    extractedAddinPath = versionDir;
                }

                if (extractedAddinPath == null)
                {
                    Debug.WriteLine($"Pasta {AddinName} não encontrada no ZIP extraído.");
                    await DelayMessage(statusProgress, $"Erro: Pasta {AddinName} não encontrada.");
                    return;
                }

                // Se a versão antiga existir, Excluir"
                if (Directory.Exists(MyAddinPath))
                {
                    Console.WriteLine("Excluindo Versão Anterior...");
                    await DelayMessage(statusProgress, "Substituindo arquivos...");

                    Directory.Delete(MyAddinPath, true);
                }

                // Se o addin antiga existir, Excluir"
                string expectedAddinFileName = $"{AddinName}.addin";
                string existingAddinFilePath = Path.Combine(_revitAddinPath, expectedAddinFileName);

                if (File.Exists(existingAddinFilePath))
                {
                    Console.WriteLine("Excluindo Addin Anterior...");
                    await DelayMessage(statusProgress, "Substituindo arquivos...");
                    File.Delete(existingAddinFilePath);
                }

                if (addinFile == null)
                {
                    Console.WriteLine("Arquivo .addin nao encontrado");
                    await DelayMessage(statusProgress, "Erro: Arquivo .addin não encontrado.");
                    return;
                }
                
                // Move o arquivo .addin diretamente para a pasta raiz de add-ins do Revit
                File.Move(addinFile, Path.Combine(_revitAddinPath, Path.GetFileName(addinFile)));
                

                // Move a nova versão para o diretório do Revit Addins
                Console.WriteLine("Instalando nova versão...");
                await DelayMessage(statusProgress, "Instalando nova versão...");
                
                // Se a pasta extraída não tiver o nome correto (ex: era a pasta "2026"), 
                // o Directory.Move vai renomeá-la para o nome do Addin ao mover
                Directory.Move(extractedAddinPath, MyAddinPath);

                // Limpeza de arquivos temporários
                File.Delete(tempZipPath);
                Directory.Delete(tempExtractPath, true);

                Console.WriteLine("Instalação concluída com sucesso.");
                await DelayMessage(statusProgress, "Instalação concluída com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na instalação: {ex.Message}");
                await DelayMessage(statusProgress, $"Erro na instalação: {ex.Message}");
            }
        }

        public async Task<int> VersionCompare(Version VInstall, Version VLatest, IProgress<string> statusProgress = null)
        {
            int compareResult;

            try
            {

                if ((VLatest == null && VInstall == null) && (VLatest.ToString() == "0.0.0.0" && VInstall.ToString() == "0.0.0.0"))
                {
                     await DelayMessage(statusProgress, "Bad Request");
                    compareResult = 0;
                    return compareResult;
                }

                else if (VLatest.ToString() == "0.0.0.0" || VLatest == null)
                {
                     await DelayMessage(statusProgress, "Erro ao obter nova versao");
                    compareResult = 1;
                    return compareResult;
                }

                else if (VInstall == null || VInstall.ToString() == "-" || VInstall.ToString() == "0.0.0.0")
                {

                    await DelayMessage(statusProgress, "Download disponivel");
                    compareResult = 2;
                    return compareResult;
                }

                else
                {
                    int compare = VInstall.CompareTo(VLatest);
                    if (compare == 0)
                    {
                        await DelayMessage(statusProgress, "Versão atualizada!");
                        compareResult = 3;
                    }
                    else if (compare < 0)
                    {
                        await DelayMessage(statusProgress, $"Nova versão disponível: {VLatest}");
                        compareResult = 4;
                    }
                    else // comparacao > 0
                    {
                        await DelayMessage(statusProgress, "Versão Beta");
                        compareResult = 5;
                    }

                    return compareResult;
                }
            }
            catch (Exception ex)
            {
                await DelayMessage(statusProgress, $"Error: {ex.Message}");
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
                case 4:
                    buttonInstall = true;
                    buttonAtt = true;
                    break;

                case 5: buttonInstall = true; break;

                default: break;
            }


            return (buttonInstall, buttonAtt);
        }


        private async Task DelayMessage(IProgress<string> progress, string mensagem, int delayMs = 500)
        {
            progress?.Report(mensagem);
            await Task.Delay(delayMs);
        }
    }
}
