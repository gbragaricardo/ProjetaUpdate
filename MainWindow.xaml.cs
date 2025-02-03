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
        public VersionService VService { get; set; }
        public OnlineVersionService OnlineVService { get; set; }
        public string SelectedComboBoxVersion { get; set; }

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

            buttonProjetaHDR.Visibility = Visibility.Visible;

            var addinName = "ProjetaHDR"; /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            VService = new VersionService(addinName);
            OnlineVService = new OnlineVersionService(addinName);
            this.DataContext = OnlineVService;

            if (VService.VerificarInstalacao() == true)
            {
                var InstalledVersion = VService.VerificarVersao();
                this.InstalledVersionText.Text = InstalledVersion;

                if (InstalledVersion == "-")
                {
                    this.ButtonAtt.IsEnabled = false;
                }

            }
            else
            {
                this.InstalledVersionText.Text = "-";
                VService.TypeVInstalled = new Version("0.0.0.0");
                this.ButtonAtt.IsEnabled = false;
            }

            
            if (VService.VerificarInstalacao() == true)
            {
                var (latestVersion, lastFourVersions) = await OnlineVService.ObterVersoesAsync();
                this.LatestVersionText.Text = latestVersion;

                if (lastFourVersions != null && lastFourVersions.Count > 0)
                {
                    VersionsComboBox.ItemsSource = lastFourVersions;
                }

                if (latestVersion == "-")
                {
                    this.ButtonInstall.IsEnabled = false;
                    this.ButtonAtt.IsEnabled = false;
                    this.VersionsComboBox.IsEnabled = false;
                }

            }
            else
            {
                this.LatestVersionText.Text = "-";
                OnlineVService.TypeVLatest = new Version("0.0.0.0");
                this.ButtonAtt.IsEnabled = false;
                
            }

            await OnlineVService.VersionCompare(VService.TypeVInstalled, OnlineVService.TypeVLatest);

        }

        private void ProjetaHDR_MenuButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ProjetaHDR_Expanded.Visibility = Visibility.Hidden;
        }

        private async void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            SelectedComboBoxVersion = this.VersionsComboBox.SelectedItem as string;
            await OnlineVService.InstalarAddinAsync(SelectedComboBoxVersion);

            if (VService.VerificarInstalacao() == true)
            {
                var InstalledVersion = VService.VerificarVersao();
                this.InstalledVersionText.Text = InstalledVersion;
            }


            if (VService.VerificarInstalacao() == true)
            {
                var (latestVersion, lastFourVersions) = await OnlineVService.ObterVersoesAsync();
                this.LatestVersionText.Text = latestVersion;

                if (lastFourVersions != null && lastFourVersions.Count > 0)
                    VersionsComboBox.ItemsSource = lastFourVersions;
            }

            await OnlineVService.VersionCompare(VService.TypeVInstalled, OnlineVService.TypeVLatest);

        }

        private async void ButtonAtt_Click(object sender, RoutedEventArgs e)
        {
            await OnlineVService.InstalarAddinAsync("latest");

            if (VService.VerificarInstalacao() == true)
            {
                var InstalledVersion = VService.VerificarVersao();
                this.InstalledVersionText.Text = InstalledVersion;
            }


            if (VService.VerificarInstalacao() == true)
            {
                var (latestVersion, lastFourVersions) = await OnlineVService.ObterVersoesAsync();
                this.LatestVersionText.Text = latestVersion;

                if (lastFourVersions != null && lastFourVersions.Count > 0)
                    VersionsComboBox.ItemsSource = lastFourVersions;
            }

            await OnlineVService.VersionCompare(VService.TypeVInstalled, OnlineVService.TypeVLatest);
        }

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

       
    }
}
