using ProjetaUpdate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjetaUpdate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BorderArrasto_Pressionado(object sender, MouseButtonEventArgs e)
        {
            // Verifica se o botão esquerdo do mouse está pressionado
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                // Permite arrastar a janela
                this.DragMove();
            }
        }

        private void FecharApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ProjetaHDR_MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var buttonProjetaHDR = this.ProjetaHDR_Expanded;

            if (buttonProjetaHDR == null)
                return;

            var addinName = "ProjetaHDR";

            buttonProjetaHDR.Visibility = Visibility.Visible;

            var versionService = new VersionService(addinName);
            if (versionService.VerificarInstalacao() == true)
            {
                var InstalledVersion = versionService.VerificarVersao();
                this.InstalledVersionText.Text = InstalledVersion;
            }

            var onlineVersionService = new OnlineVersionService(addinName);
            if (versionService.VerificarInstalacao() == true)
            {
                var latestVersion = await onlineVersionService.ObterVersaoMaisRecenteAsync();
                this.LatestVersionText.Text = latestVersion;
            }
        }

        private void ProjetaHDR_MenuButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ProjetaHDR_Expanded.Visibility = Visibility.Hidden;
        }

        private async void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            var installer = new AddinInstaller();

            await installer.InstalarAddinAsync();
        }
    }
}
