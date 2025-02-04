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
<<<<<<< Updated upstream
=======

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

            }
            else
            {
                this.InstalledVersionText.Text = "-";
                VService.TypeVInstalled = new Version("0.0.0.0");
            }

            var (latestVersion, lastFourVersions) = await OnlineVService.ObterVersoesAsync();
            this.LatestVersionText.Text = latestVersion;

            if (latestVersion != "-")
            {
                VersionsComboBox.ItemsSource = lastFourVersions;

                //this.ButtonInstall.IsEnabled = true;
                //this.VersionsComboBox.IsEnabled = true;
            }                    
            else
            {
                OnlineVService.TypeVLatest = new Version("0.0.0.0");
            }
            
            await OnlineVService.VersionCompare(VService.TypeVInstalled, OnlineVService.TypeVLatest);
            var(boolInstall, boolAtt) =  await OnlineVService.CompareResult(VService.TypeVInstalled, OnlineVService.TypeVLatest);

            this.ButtonInstall.IsEnabled = boolInstall;
            this.VersionsComboBox.IsEnabled = boolInstall;
            this.ButtonAtt.IsEnabled = boolAtt;

        }

        private void ProjetaHDR_MenuButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ProjetaHDR_Expanded.Visibility = Visibility.Hidden;
            this.ButtonAtt.IsEnabled = false;
            this.ButtonInstall.IsEnabled = false;
        }

        private async void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            SelectedComboBoxVersion = this.VersionsComboBox.SelectedItem as string;
            await OnlineVService.InstalarAddinAsync(SelectedComboBoxVersion);

            await Task.Delay(2000);

            if (VService.VerificarInstalacao() == true)
            {
                var InstalledVersion = VService.VerificarVersao();
                this.InstalledVersionText.Text = InstalledVersion;

            }
            else
            {
                this.InstalledVersionText.Text = "-";
                VService.TypeVInstalled = new Version("0.0.0.0");
            }

            var (latestVersion, lastFourVersions) = await OnlineVService.ObterVersoesAsync();
            this.LatestVersionText.Text = latestVersion;

            if (latestVersion != "-")
            {
                VersionsComboBox.ItemsSource = lastFourVersions;

                //this.ButtonInstall.IsEnabled = true;
                //this.VersionsComboBox.IsEnabled = true;
            }
            else
            {
                OnlineVService.TypeVLatest = new Version("0.0.0.0");
            }

            await OnlineVService.VersionCompare(VService.TypeVInstalled, OnlineVService.TypeVLatest);
            var (boolInstall, boolAtt) = await OnlineVService.CompareResult(VService.TypeVInstalled, OnlineVService.TypeVLatest);

            this.ButtonInstall.IsEnabled = boolInstall;
            this.VersionsComboBox.IsEnabled = boolInstall;
            this.ButtonAtt.IsEnabled = boolAtt;

        }

        private async void ButtonAtt_Click(object sender, RoutedEventArgs e)
        {
            await OnlineVService.InstalarAddinAsync("latest");

            await Task.Delay(2000);

            if (VService.VerificarInstalacao() == true)
            {
                var InstalledVersion = VService.VerificarVersao();
                this.InstalledVersionText.Text = InstalledVersion;

            }
            else
            {
                this.InstalledVersionText.Text = "-";
                VService.TypeVInstalled = new Version("0.0.0.0");
            }

            var (latestVersion, lastFourVersions) = await OnlineVService.ObterVersoesAsync();
            this.LatestVersionText.Text = latestVersion;

            if (latestVersion != "-")
            {
                VersionsComboBox.ItemsSource = lastFourVersions;

                //this.ButtonInstall.IsEnabled = true;
                //this.VersionsComboBox.IsEnabled = true;
            }
            else
            {
                OnlineVService.TypeVLatest = new Version("0.0.0.0");
            }

            await OnlineVService.VersionCompare(VService.TypeVInstalled, OnlineVService.TypeVLatest);
            var (boolInstall, boolAtt) = await OnlineVService.CompareResult(VService.TypeVInstalled, OnlineVService.TypeVLatest);

            this.ButtonInstall.IsEnabled = boolInstall;
            this.VersionsComboBox.IsEnabled = boolInstall;
            this.ButtonAtt.IsEnabled = boolAtt;
        }

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

       
>>>>>>> Stashed changes
    }
}
