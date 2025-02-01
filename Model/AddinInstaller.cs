using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetaUpdate.Model
{
    internal class AddinInstaller
    {
        private readonly string _addinName;
        private readonly string _addinPath;
        private readonly string _backupPath;

        public AddinInstaller(string addinName = "ProjetaUpdate")
        {
            _addinName = addinName;

            // Caminho onde o add-in será instalado
            _addinPath = 
                Path.Combine(Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Autodesk",
                "Revit",
                "Addins",
                "2024",
                _addinName);

            // Caminho para armazenar versões antigas
            _backupPath = Path.Combine(_addinPath, "VersaoAnterior");
        }

        /// <summary>
        /// Obtém a URL do arquivo ZIP da última release no GitHub.
        /// </summary>
        private async Task<string> ObterUrlUltimaVersaoAsync()
        {
            string githubApiUrl = $"https://api.github.com/repos/gbragaricardo/{_addinName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                string response = await client.GetStringAsync(githubApiUrl);
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    return doc.RootElement.GetProperty("zipball_url").GetString();
                }
            }
        }

        /// <summary>
        /// Faz o download do ZIP da release e extrai apenas a pasta do add-in.
        /// </summary>
        public async Task InstalarAddinAsync()
        {
            try
            {
                Console.WriteLine("Obtendo URL da última versão...");
                string zipUrl = await ObterUrlUltimaVersaoAsync();
                string tempZipPath = Path.Combine(Path.GetTempPath(), $"{_addinName}.zip");
                string tempExtractPath = Path.Combine(Path.GetTempPath(), $"{_addinName}_temp");

                Console.WriteLine("Baixando arquivo ZIP...");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    byte[] zipData = await client.GetByteArrayAsync(zipUrl);
                    File.WriteAllBytes(tempZipPath, zipData);
                }

                // Remove pasta temporária se já existir
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }

                Console.WriteLine("Extraindo arquivos...");
                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath);

                // Encontra a pasta correta dentro do ZIP extraído
                string[] extractedDirs = Directory.GetDirectories(tempExtractPath);
                // Verifica se existe um diretório dentro do primeiro nível da extração
                if (extractedDirs.Length == 1)
                {
                    string mainExtractedPath = extractedDirs[0]; // Primeiro diretório encontrado
                    extractedDirs = Directory.GetDirectories(mainExtractedPath); // Agora pegamos os subdiretórios dentro dele
                }

                string extractedAddinPath = Array.Find(extractedDirs, dir => Path.GetFileName(dir) == _addinName);

                if (extractedAddinPath == null)
                {
                    Console.WriteLine($"Pasta {_addinName} não encontrada no ZIP extraído.");
                    return;
                }

                // Se a versão antiga existir, movê-la para a pasta "VersaoAnterior"
                if (Directory.Exists(_addinPath))
                {
                    Console.WriteLine("Movendo versão anterior...");

                    // Se a pasta "VersaoAnterior" já existir, apague-a antes de mover os arquivos antigos
                    if (Directory.Exists(_backupPath))
                    {
                        Directory.Delete(_backupPath, true);
                    }
                    Directory.CreateDirectory(_backupPath);

                    Directory.Move(_addinPath, _backupPath);
                }

                // Move a nova versão para o diretório do Revit Addins
                Console.WriteLine("Instalando nova versão...");
                Directory.Move(extractedAddinPath, _addinPath);

                // Limpeza de arquivos temporários
                File.Delete(tempZipPath);
                Directory.Delete(tempExtractPath, true);

                Console.WriteLine("Instalação concluída com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na instalação: {ex.Message}");
            }
        }
    }
}
