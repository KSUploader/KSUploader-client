﻿using KSLUploader.Classes;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System;
using Microsoft.Win32;

namespace KSLUploader
{
    public partial class App : Application
    {
        private SystemTray ksluTray;
        private KeyListener keyListener;        

        protected override void OnStartup(StartupEventArgs e)
        {
            //no double instance!
            if(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show("KSLUploader is already running.", "Error");
                Current.Shutdown();
                return;
            }
           
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            InizializeSettings();
            CheckRunAtStartup();
            
            keyListener = new KeyListener();
            ksluTray = new SystemTray();
        }

        private void InizializeSettings()
        {
            //generals
            SettingsManager.Inizialize("RunAtStartup", false);
            SettingsManager.Inizialize("SaveLocal", false);
            SettingsManager.Inizialize("SaveLocalPath", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            SettingsManager.Inizialize("UploadMethod", "SOCKET");

            //protocol
            SettingsManager.Inizialize("SocketAddress","localhost");
            SettingsManager.Inizialize("SocketPort", 4030);
            SettingsManager.Inizialize("SocketPassword", "pass");

            //ftp
            SettingsManager.Inizialize("UseFTPS", false);
            SettingsManager.Inizialize("AcceptCertificates", true);
            SettingsManager.Inizialize("FTPAddress", null);
            SettingsManager.Inizialize("FTPPort", 21);
            SettingsManager.Inizialize("FTPDirectory", null);
            SettingsManager.Inizialize("FTPWeburl", null);
            SettingsManager.Inizialize("FTPUser", null);
            SettingsManager.Inizialize("FTPPassword", null);

            //shortcut
            SettingsManager.Inizialize("ShortcutArea", new string[] { "LControlKey", "LMenu", "1" });
            SettingsManager.Inizialize("ShortcutDesktop", new string[] { "LControlKey", "LMenu", "2" });
            SettingsManager.Inizialize("ShortcutFile", new string[] { "LControlKey", "LMenu", "3" });
            SettingsManager.Inizialize("ShortcutClipboard", new string[] { "LControlKey", "LMenu", "4" });
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Current.Windows.OfType<T>().Any()
               : Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static void CheckRunAtStartup()
        {
            var registryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            if((bool)SettingsManager.Get("RunAtStartup"))
            {               
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey, true))
                {
                    key.SetValue("KSLU", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                }
            }
            else
            {
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey, true))
                {
                    key.DeleteValue("KSLU", false);
                }
            }
        }
    }
}
