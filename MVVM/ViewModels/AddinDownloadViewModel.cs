using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ProjetaUpdate.MVVM.ViewModels
{
    internal class AddinDownloadViewModel : ObservableObject
    {
        //Properties
        #region Properties

        private readonly VersionService _vService;
        private readonly OnlineVersionService _onlineVService;

        //Mensagem de status na parte de baixo
        //private Object _statusMessage;
        //public Object StatusMessage
        //{
        //    get { return _statusMessage; }
        //    set
        //    {
        //        if (_statusMessage != value)
        //        {
        //            _statusMessage = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        public IProgress<string> StatusMessage { get; }


        //Nome do Addin usado nas buscas e etc. Ex: ProjetaHDR
        private string _addinName = "FirstProjetaHDR";
        public string AddinName
        {
            get { return _addinName; }
            set
            {
                _addinName = value;
                OnPropertyChanged();
            }
        }

        //Versao Instalada na maquina
        private string _installedVersion;
        public string InstalledVersion
        {
            get { return _installedVersion; }
            set
            {
                _installedVersion = value;
                OnPropertyChanged();
            }
        }

        //Ultima Release
        private string _latestVersion;
        public string LatestVersion
        {
            get { return _latestVersion; }
            set
            {
                _latestVersion = value;
                OnPropertyChanged();
            }
        }

        //Versao do revit selecionada pelo usuario
        private string _revitVersion;
        public string SelectedRevitVersion
        {
            get { return _revitVersion; }
            set
            {
                if (_revitVersion != value)
                {
                    _revitVersion = value;
                    OnPropertyChanged();

                    _vService.SelectedRevitVersion = value;
                    _vService.AddinName = AddinName;
                    
                    InstalledVersion = _vService.VerifyInstallAndVersion();
                }
            }
        }

        //Versoes disponiveis do plugin selecionadas pelo usuario
        private string _selectedAddinVersion;
        public string SelectedAddinVersion
        {
            get { return _selectedAddinVersion; }
            set
            {
                if (_selectedAddinVersion != value)
                {
                    _selectedAddinVersion = value;
                    OnPropertyChanged();
                }

            }
        }

        //Lista das versoes do plugin disponiveis
        private List<string> _allLatestVersion;
        public List<string> AllLatestVersion
        {
            get { return _allLatestVersion; }
            set
            {
                _allLatestVersion = value;
                OnPropertyChanged();
            }
        }

        private string _canInstall;
        public string CanInstall
        {
            get { return _canInstall; }
            set
            {
                _canInstall = value;
                OnPropertyChanged();
            }
        }

        private string _canUpdate;
        public string CanUpdate
        {
            get { return _canUpdate; }
            set
            {
                _canUpdate = value;
                OnPropertyChanged();
            }
        }

        #endregion

        //RelayCommands
        #region RelayCommands
        public RelayCommand InstallButton { get; set; }
        public RelayCommand UpdateButton { get; set; }


        #endregion


        //Constructor
        #region Constructor
        public AddinDownloadViewModel()
        {
            InstallButton = new RelayCommand(async o => await InstallAddin());

            UpdateButton = new RelayCommand(async o => await UpdateAddin());

            _vService = new VersionService(AddinName, SelectedRevitVersion);
            _onlineVService = new OnlineVersionService(AddinName, SelectedRevitVersion);

            LoadVersions();
        }
        #endregion


        //Methods
        #region Methods

        public async void LoadVersions()
        {
            InstalledVersion = _vService.VerifyInstallAndVersion();

            (LatestVersion, AllLatestVersion) = await _onlineVService.ObterVersoesAsync(StatusMessage);
            var (_canInstall, _canUpdate) = await _onlineVService.CompareResult(_vService.InstalledTypeOfVersion, _onlineVService.LatestTypeOfVersion);
            
        }

        private async Task InstallAddin()
        {
            await _onlineVService.InstalarAddinAsync(SelectedAddinVersion, StatusMessage);
            LoadVersions();
        }

        private async Task UpdateAddin()
        {

            await _onlineVService.InstalarAddinAsync("latest", StatusMessage);
            LoadVersions();
        }

        #endregion
    }
}
