﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using IWshRuntimeLibrary;
using Squirrel;

namespace ProjetaUpdate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool updated = await CheckForUpdatesAtStartup();
        }

        private async Task<bool> CheckForUpdatesAtStartup()
        {
            try
            {
                string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProjetaUpdater.lnk");
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Setup.ico");

                using (var updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/gbragaricardo/ProjetaUpdate"))
                {
                    var updateInfo = await updateManager.CheckForUpdate();

                    if (updateInfo.ReleasesToApply.Count > 0)
                    {
                        MessageBox.Show("Nova versão disponível! O aplicativo será atualizado e reiniciado.", "Atualização Disponível", MessageBoxButton.OK, MessageBoxImage.Information);

                        await updateManager.UpdateApp();

                        SetupShortcut();

                        UpdateManager.RestartApp();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar atualizações: {ex.Message}", "Erro de Atualização", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false; // Nenhuma atualização foi aplicada
        }

        private static void SetupShortcut()
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Projeta\nUpdate.exe");
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Setup.ico");

            // Criar atalho com nome formatado "Projeta\nUpdate"
            ShortcutHelper.CreateShortcut("Projeta Update", exePath, iconPath);
        }
    }
}
