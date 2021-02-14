using RockSnifferGui.Services;
using RockSnifferLib.Cache;
using System.Windows;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static GameProcessService gameProcessService;
        private static SnifferService snifferService;
        private static CurrentSongService currentSongService;
        private static ICache cache;

        private const string version = "0.3.0a";
        public static string Version { get => "v" + version; }
        public static ICache Cache { get => App.cache; set => App.cache = value; }

        public App()
        {
            App.gameProcessService = GameProcessService.Instance;
            App.snifferService = SnifferService.Instance;
            App.currentSongService = CurrentSongService.Instance;

            App.cache = new SQLiteCache();

            this.Exit += this.App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            GameProcessService.Instance.Dispose();
            RockSnifferGui.Properties.Settings.Default.Save();
        }
    }
}
