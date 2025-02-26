using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjetaUpdate.MVVM.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        //Properties
        #region Properties
        public HomeViewModel HomeVM { get; set; }
        public AddinDownloadViewModel AddinDownloadVM { get; set; }

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        #endregion

        //RelayCommands
        #region RelayCommands
        public RelayCommand AddinDownloadViewCommand { get; set; }
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand CloseAppCommand { get; set; }
        public RelayCommand MinimizeAppCommand { get; set; }
        #endregion

        //Constructor
        #region Constructor
        public MainViewModel()
        {
            CloseAppCommand = new RelayCommand(o => { Application.Current.Shutdown(); });
            MinimizeAppCommand = new RelayCommand(o => { Application.Current.MainWindow.WindowState = WindowState.Minimized; });

            HomeVM = new HomeViewModel();
            HomeViewCommand = new RelayCommand(o => { CurrentViewModel = HomeVM; });
            
            AddinDownloadViewCommand = new RelayCommand(parameter =>
            {
                string addinName = parameter as string;

                if (AddinDownloadVM == null)
                {
                    AddinDownloadVM = new AddinDownloadViewModel{ AddinName = addinName };
                    CurrentViewModel = AddinDownloadVM;
                }
                else
                {
                    AddinDownloadVM.AddinName = addinName;
                    CurrentViewModel = AddinDownloadVM;
                    AddinDownloadVM.LoadVersions();
                }
            });
        }
        #endregion

        //Methods
        #region Methods

        #endregion

    }
}
