using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetaUpdate.MVVM.ViewModels
{
    internal class MainViewModel : ObservableObject
    {

        public AddinDownloadViewModel AddinDownloadVM { get; set; }
        public RelayCommand AddinDownloadViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public RelayCommand HomeViewCommand { get; set; }



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

        public MainViewModel()
        {
        
            HomeVM = new HomeViewModel();
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentViewModel = HomeVM;
            });


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

    }
}
