using RockSnifferGui.Services;
using RockSnifferLib.Cache;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static GameProcessService gameProcessService;
        private static ICache cache;

        private const string version = "0.3.0";
        public static string Version { get => "v" + version; }
        public static ICache Cache { get => App.cache; set => App.cache = value; }

        public App()
        {
            App.gameProcessService = GameProcessService.Instance;
            App.cache = new SQLiteCache();

            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            GameProcessService.Instance.Dispose();
        }
    }
}
