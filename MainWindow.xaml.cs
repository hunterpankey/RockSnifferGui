using RockSnifferGui.Configuration;
using RockSnifferGui.Model;
using RockSnifferLib.Cache;
using RockSnifferLib.Events;
using RockSnifferLib.Logging;
using RockSnifferLib.RSHelpers;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal const string version = "0.0.1";

        internal static Process rsProcess;
        internal static ICache cache;
        internal static Config config;

        private static readonly bool Is64Bits = (IntPtr.Size == 8);

        private static readonly Random random = new Random();

        private SongDetails details = new SongDetails();
        private RSMemoryReadout memReadout = new RSMemoryReadout();
        private readonly System.Drawing.Image defaultAlbumCover = new Bitmap(256, 256);

        private List<SongPlayInstance> playedSongs = new List<SongPlayInstance>();
        private SongPlayInstance currentSong;

        public MainWindow()
        {
            InitializeComponent();


            this.Initialize();
            this.Run();
            //Keep running even when rocksmith disappears
            //while (true)
            //{
            //    try
            //    {
            //        this.Run();
            //    }
            //    catch (Exception e)
            //    {
            //        //Catch all exceptions that are not handled and log
            //        Logger.LogError("Encountered unhandled exception: {0}\r\n{1}", e.Message, e.StackTrace);
            //        throw e;
            //    }
            //}
        }

        public void Initialize()
        {
            //Set title and print version
            this.Title = string.Format("RockSniffer {0}", version);
            //Console.Title = string.Format("RockSniffer {0}", version);
            Logger.Log("RockSniffer {0} ({1}bits)", version, Is64Bits ? "64" : "32");

            //Initialize and load configuration
            config = new Config();
            try
            {
                config.Load();
            }
            catch (Exception e)
            {
                Logger.LogError("Could not load configuration: {0}\r\n{1}", e.Message, e.StackTrace);
                throw e;
            }

            //Run version check
            // not doing version checking for now
            //if (!config.debugSettings.disableVersionCheck)
            //{
            //    if (version.Contains("PR"))
            //    {
            //        Logger.Log("Pre-release version, skipping version check");
            //    }
            //    else
            //    {
            //        VersionCheck();
            //    }
            //}

            //Transfer logging options
            Logger.logStateMachine = config.debugSettings.debugStateMachine;
            Logger.logCache = config.debugSettings.debugCache;
            Logger.logFileDetailQuery = config.debugSettings.debugFileDetailQuery;
            Logger.logMemoryReadout = config.debugSettings.debugMemoryReadout;
            Logger.logSongDetails = config.debugSettings.debugSongDetails;
            Logger.logSystemHandleQuery = config.debugSettings.debugSystemHandleQuery;
            Logger.logProcessingQueue = config.debugSettings.debugProcessingQueue;

            //Initialize cache
            cache = new SQLiteCache();

            //Create directories
            //Directory.CreateDirectory("output");

            //Enable addon service if configured
            // don't know what this is for yet
            //if (config.addonSettings.enableAddons)
            //{
            //    try
            //    {
            //        addonService = new AddonService(config.addonSettings, new SQLiteStorage());
            //    }
            //    catch (SocketException e)
            //    {
            //        Logger.LogError("Please verify that the IP address is valid and the port is not already in use");
            //        Logger.LogError("Could not start addon service: {0}\r\n{1}", e.Message, e.StackTrace);
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.LogError("Could not start addon service: {0}\r\n{1}", e.Message, e.StackTrace);
            //    }
            //}
        }

        public void Run()
        {
            //Clear output / create output files
            // not working with file output right now
            //ClearOutput();

            Logger.Log("Waiting for rocksmith");

            //Loop infinitely trying to find rocksmith process
            // should do this asynchronously on another thread
            while (true)
            {
                var processes = Process.GetProcessesByName("Rocksmith2014");

                //Sleep for 1 second if no processes found
                if (processes.Length == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                //Select the first rocksmith process and open a handle
                rsProcess = processes[0];

                if (rsProcess.HasExited || !rsProcess.Responding)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                break;
            }

            Logger.Log("Rocksmith found! Sniffing...");

            //Check rocksmith executable hash to make sure its the correct version
            string hash = PSARCUtil.GetFileHash(new FileInfo(rsProcess.MainModule.FileName));

            Logger.Log($"Rocksmith executable hash: {hash}");

            if (!hash.Equals("GxT+/TXLpUFys+Cysek8zg=="))
            {
                Logger.LogError("Executable hash does not match expected hash, make sure you have the correct version");
                Logger.Log("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Initialize file handle reader and memory reader
            Sniffer sniffer = new Sniffer(rsProcess, cache, config.snifferSettings);

            //Listen for events
            sniffer.OnSongChanged += Sniffer_OnCurrentSongChanged;
            sniffer.OnSongStarted += Sniffer_OnSongStarted;
            sniffer.OnSongEnded += Sniffer_OnSongEnded;

            sniffer.OnMemoryReadout += Sniffer_OnMemoryReadout;

            //Add RPC event listeners
            // not doing anything with Discord right now
            //if (config.rpcSettings.enabled)
            //{
            //    rpcHandler = new DiscordRPCHandler(sniffer);
            //}

            //Inform AddonService
            // don't know what this is for yet
            //if (config.addonSettings.enableAddons && addonService != null)
            //{
            //    addonService.SetSniffer(sniffer);
            //}

            // not a console app anymore, so this has to change
            //while (true)
            //{
            //    if (rsProcess == null || rsProcess.HasExited)
            //    {
            //        break;
            //    }

            //    OutputDetails();

            //    //GOTTA GO FAST
            //    Thread.Sleep(1000);

            //    if (random.Next(100) == 0)
            //    {
            //        Console.WriteLine("*sniff sniff*");
            //    }
            //}

            //sniffer.Stop();

            //Clean up as best as we can
            //rsProcess.Dispose();
            //rsProcess = null;

            // no Discord RPC stuff right now
            //rpcHandler?.Dispose();
            //rpcHandler = null;

            //Logger.Log("This is rather unfortunate, the Rocksmith2014 process has vanished :/");
        }

        #region Sniffer Events
        private void Sniffer_OnSongStarted(object sender, OnSongStartedArgs e)
        {
            SongPlayInstance newSong = new SongPlayInstance(e.song);

            this.playedSongs.Add(newSong);
            this.currentSong = newSong;

            newSong.StartSong();
        }

        private void Sniffer_OnSongEnded(object sender, OnSongEndedArgs e)
        {
            this.currentSong.FinishSong();
            this.currentSong = null;
        }

        private void Sniffer_OnCurrentSongChanged(object sender, OnSongChangedArgs args)
        {
            details = args.songDetails;
            updateValues();

            //Write album art
            if (details.albumArt != null)
            {
                WriteImageToFileLocking("output/album_cover.jpeg", details.albumArt);

                using (var memory = new MemoryStream())
                {
                    details.albumArt.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    this.albumArtImage.Source = bitmapImage;
                }
            }
            else
            {
                WriteImageToFileLocking("output/album_cover.jpeg", defaultAlbumCover);
            }
        }

        private void Sniffer_OnMemoryReadout(object sender, OnMemoryReadoutArgs args)
        {
            memReadout = args.memoryReadout;
            updateValues();

            if (args.memoryReadout.noteData != null)
            {
                this.currentSong.UpdateNoteData(args.memoryReadout.noteData);
            }
        }
        #endregion

        private void WriteImageToFileLocking(string file, System.Drawing.Image image)
        {
            //If the file doesn't exist, create it by writing an empty string into it
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "");
            }

            try
            {
                //Open a file stream, write access, no sharing
                using (FileStream fstream = new FileStream(file, FileMode.Truncate, FileAccess.Write, FileShare.None))
                {
                    image.Save(fstream, ImageFormat.Jpeg);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Unable to write to file {0}: {1}\r\n{2}", file, e.Message, e.StackTrace);
            }
        }

        private void WriteTextToFileLocking(string file, string contents)
        {
            //If the file doesn't exist, create it by writing an empty string into it
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "");
            }

            //Encode with UTF-8
            byte[] data = Encoding.UTF8.GetBytes(contents);

            //Write to file
            WriteToFileLocking(file, data);
        }

        private void WriteToFileLocking(string file, byte[] contents)
        {
            try
            {
                //Open a file stream, write access, read only sharing
                using (FileStream fstream = new FileStream(file, FileMode.Truncate, FileAccess.Write, FileShare.Read))
                {
                    //Write to file

                    fstream.Write(contents, 0, contents.Length);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Unable to write to file {0}: {1}\r\n{2}", file, e.Message, e.StackTrace);
            }
        }

        private void updateValues()
        {
            this.bandNameLabel.Content = details.artistName;
            this.songNameLabel.Content = details.songName;
            this.albumNameLabel.Content = $"{details.albumName} ({details.albumYear})";

            var nd = memReadout.noteData ?? new LearnASongNoteData();
            this.songTimerimeValueLabel.Content = FormatTime(memReadout.songTimer);

            this.notesHitValueLabel.Content = nd.TotalNotesHit.ToString();
            this.notesMissedValueLabel.Content = nd.TotalNotesMissed.ToString();
            this.totalNotesValueLabel.Content = nd.TotalNotes.ToString();

            this.currentStreakValueLabel.Content = (nd.CurrentHitStreak - nd.CurrentMissStreak).ToString();
            this.highestStreakValueLabel.Content = nd.HighestHitStreak.ToString();

            this.noteHitPercentageValueLabel.Content = FormatPercentage(nd.Accuracy);
        }

        public static string FormatTime(float lengthTime)
        {
            TimeSpan t = TimeSpan.FromSeconds(Math.Ceiling(lengthTime));
            return t.ToString(config.formatSettings.timeFormat);
        }

        public static string FormatPercentage(double frac)
        {
            return string.Format(config.formatSettings.percentageFormat, frac);
        }
    }
}
