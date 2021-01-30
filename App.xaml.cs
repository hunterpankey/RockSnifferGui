using RockSnifferGui.Services;
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
        private GameProcessService gameProcessService;


        private const string version = "0.0.4";
        public static string Version { get => "v" + version; }

        public App()
        {
            this.gameProcessService = GameProcessService.Instance;
            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            GameProcessService.Instance.Dispose();
        }
    }
}
