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
        public List<string> AvailableRevitVersions { get; } = new List<string> { "2024", "2022" };
        public IProgress<string> StatusProgress { get; }


        //Mensagem de status na parte de baixo
        private Object _statusMessage;
        public Object StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }


        //Nome do Addin usado nas buscas e etc. Ex: ProjetaHDR
        private string _addinName = "FirstProjetaHDR";
        public string AddinName
        {
            get { return _addinName; }
            set
            {
                if (_addinName != value)
                {
                    _addinName = value;
                    OnPropertyChanged();
                    UpdateUiAndProps();
                }
                
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
        private string _revitVersion = "2024";
        public string SelectedRevitVersion
        {
            get { return _revitVersion; }
            set
            {
                if (_revitVersion != value)
                {
                    _revitVersion = value;
                    OnPropertyChanged();
                    UpdateUiAndProps();
                    InstalledVersion = _vService.VerifyInstallAndVersion();
                    UpdateMessage();
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

                    _onlineVService.VersionTag = value;
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

        private bool _canInstall;
        public bool CanInstall
        {
            get { return _canInstall; }
            set
            {
                if (_canInstall != value)
                {
                _canInstall = value;
                OnPropertyChanged();
                }
            }
        }

        private bool _canUpdate;
        public bool CanUpdate
        {
            get { return _canUpdate; }
            set
            {
                if (_canUpdate != value)
                {
                    _canUpdate = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canYearChange;
        public bool CanYearChange
        {
            get { return _canYearChange; }
            set
            {
                if (_canYearChange != value)
                {
                    _canYearChange = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        //RelayCommands
        #region RelayCommands
        public RelayCommand InstallButton { get; set; }
        public RelayCommand UpdateButton { get; set; }
        public RelayCommand LoadVersionsButton { get; set; }


        #endregion


        //Constructor
        #region Constructor
        public AddinDownloadViewModel()
        {
            StatusProgress = new Progress<string>(msg => StatusMessage = msg);

            InstallButton = new RelayCommand(async o => await InstallAddin());

            UpdateButton = new RelayCommand(async o => await UpdateAddin());

            LoadVersionsButton = new RelayCommand(o => LoadVersions());

            _vService = new VersionService(AddinName, SelectedRevitVersion);
            _onlineVService = new OnlineVersionService(AddinName, SelectedRevitVersion);

            LoadVersions();

        }
        #endregion


        //Methods
        #region Methods

        public void UpdateUiAndProps()
        {
            _vService.SelectedRevitVersion = SelectedRevitVersion;
            _vService.AddinName = AddinName;
            _vService.UpdateProps();

            _onlineVService.SelectedRevitVersion = SelectedRevitVersion;
            _onlineVService.AddinName = AddinName;
            _onlineVService.SelectedVersionTag = SelectedAddinVersion;
            _onlineVService.UpdateProps();
        }

        public async void UpdateMessage()
        {
            await _onlineVService.VersionCompare(_vService.InstalledTypeOfVersion, _onlineVService.LatestTypeOfVersion, StatusProgress);
            (_canInstall, _canUpdate) = await _onlineVService.CompareResult(_vService.InstalledTypeOfVersion, _onlineVService.LatestTypeOfVersion);
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanUpdate));
        }

        public async void LoadVersions()
        {
            CanYearChange = false;

            InstalledVersion = _vService.VerifyInstallAndVersion();

            (LatestVersion, AllLatestVersion) = await _onlineVService.ObterVersoesAsync(StatusProgress);
            await _onlineVService.VersionCompare(_vService.InstalledTypeOfVersion, _onlineVService.LatestTypeOfVersion, StatusProgress);
            (_canInstall, _canUpdate) = await _onlineVService.CompareResult(_vService.InstalledTypeOfVersion, _onlineVService.LatestTypeOfVersion);

            UpdateUiAndProps();
            InstalledVersion = _vService.VerifyInstallAndVersion();
            UpdateMessage();

            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanUpdate));

            CanYearChange = true;
        }

        private async Task InstallAddin()
        {
            CanYearChange = false;

            CanInstall = CanUpdate = false;
            await _onlineVService.InstalarAddinAsync(SelectedAddinVersion, StatusProgress);
            UpdateUiAndProps();
            LoadVersions();

            CanYearChange = true;
        }

        private async Task UpdateAddin()
        {
            CanYearChange = false;

            CanInstall = CanUpdate = false;
            await _onlineVService.InstalarAddinAsync("latest", StatusProgress);
            UpdateUiAndProps();
            LoadVersions();

            CanYearChange = true;

        }

        #endregion
    }
}
