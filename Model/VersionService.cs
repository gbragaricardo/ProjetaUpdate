using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetaUpdate
{
    public class VersionService
    {

        private readonly string _addinPath;
        private readonly string _addinName;
        public Version TypeVInstalled { get; set; }

        public VersionService(string AddinName)
        {
            _addinName = AddinName;
            _addinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"Autodesk\Revit\Addins\2024\{_addinName}");
        }

        public bool VerificarInstalacao()
        {
            return Directory.Exists(_addinPath);
        }

        public string VerificarVersao()
        {
            string assemblyPath = Path.Combine(_addinPath, $"{_addinName}.dll");

            if (File.Exists(assemblyPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);

                if (versionInfo.FileVersion != null)
                {
                    TypeVInstalled = new Version(versionInfo.FileVersion);
                }
                else
                {
                    TypeVInstalled = new Version("0.0.0.0");
                }

                return versionInfo.FileVersion;
            }

            TypeVInstalled = new Version("0.0.0.0");
            return "-";
        }


    }
}
