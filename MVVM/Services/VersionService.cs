using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetaUpdate
{
    public class VersionService
    {
        //Properties
        #region Properties
        public string AddinPath { get; set; }
        public string AddinName { get; set; }
        public string SelectedRevitVersion { get; set; }
        public Version InstalledTypeOfVersion { get; set; }
        #endregion

        //Constructor
        #region Constructor
        public VersionService(string addinName, string revitVersion)
        {
            AddinPath = addinName;
            SelectedRevitVersion = revitVersion;
            UpdateProps();
        }
        #endregion

        //Methods
        #region Methods
        public string VerifyInstallAndVersion()
        {
            UpdateProps();

            if (VerificarInstalacao() == false)
                return "-";
            else
                return VerificarVersao();
        }

        public void UpdateProps()
        {
            AddinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"Autodesk\Revit\Addins\{SelectedRevitVersion}\{AddinName}");
        }

        public bool VerificarInstalacao()
        {
            return Directory.Exists(AddinPath);
        }

        public string VerificarVersao()
        {
            string assemblyPath = Path.Combine(AddinPath, $"{AddinName}.dll");

            if (File.Exists(assemblyPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);

                if (versionInfo.FileVersion != null)
                {
                    InstalledTypeOfVersion = new Version(versionInfo.FileVersion);
                }
                else
                {
                    InstalledTypeOfVersion = new Version("0.0.0.0");
                }

                return versionInfo.FileVersion;
            }

            InstalledTypeOfVersion = new Version("0.0.0.0");
            return "-";
        }
        #endregion
    }
}
