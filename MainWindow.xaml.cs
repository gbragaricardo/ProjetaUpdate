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

        private void ProjetaHDR_Checked(object sender, RoutedEventArgs e)
        {
            this.ProjetaHDRContent.Visibility = Visibility.Visible;
        }

        private void ProjetaHDR_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ProjetaHDRContent.Visibility = Visibility.Hidden;
        }

    }
}
