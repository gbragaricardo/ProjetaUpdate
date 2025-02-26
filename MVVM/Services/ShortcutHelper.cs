using System;
using System.IO;
using IWshRuntimeLibrary;

public static class ShortcutHelper
{
    public static void CreateShortcut(string shortcutName, string targetPath, string iconPath)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string shortcutLocation = Path.Combine(desktopPath, $"{shortcutName}.lnk");

        // Se existir um atalho anterior, apaga antes de criar o novo
        if (System.IO.File.Exists(shortcutLocation))
        {
            System.IO.File.Delete(shortcutLocation);
        }

        // Criando um novo atalho
        WshShell shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

        shortcut.Description = "Addins Updater";
        shortcut.TargetPath = targetPath;
        shortcut.IconLocation = iconPath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
        shortcut.Save();
    }
}