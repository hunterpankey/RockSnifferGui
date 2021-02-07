using RockSnifferLib.Logging;
using RockSnifferLib.RSHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xaml.Schema;

namespace RockSnifferGui.Services
{
    public enum GameProcessStatus { NOT_RUNNING, RUNNING, NOT_RESPONDING, EXITED }

    public class GameProcessService : IDisposable, INotifyPropertyChanged
    {
        private const string GameProcessHash = "GxT+/TXLpUFys+Cysek8zg==";

        public delegate void OnGameProcessChanged(object sender, GameProcessChangedEventArgs args);
        public event OnGameProcessChanged GameProcessChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private Process gameProcess;
        private GameProcessStatus gameProcessStatus;
        private Thread processWatcher;

        private static GameProcessService instance;
        public static GameProcessService Instance
        {
            get
            {
                if (GameProcessService.instance == null)
                {
                    GameProcessService.instance = new GameProcessService();
                }

                return GameProcessService.instance;
            }
            private set
            {
                GameProcessService.instance = value;
            }
        }

        public Process GameProcess { get => gameProcess; private set => gameProcess = value; }
        public GameProcessStatus Status { get => gameProcessStatus; private set => gameProcessStatus = value; }
        public string StatusForDisplay
        {
            get
            {
                switch (this.Status)
                {
                    case GameProcessStatus.RUNNING:
                        return "Running";
                    case GameProcessStatus.NOT_RESPONDING:
                        return "Not Responding";
                    case GameProcessStatus.EXITED:
                        return "Exited";
                    case GameProcessStatus.NOT_RUNNING:
                        return "Not Running";
                    default:
                        return string.Empty;
                }
            }
        }

        public GameProcessService()
        {
            this.FindGameProcess();

            // start the process watcher thread to track the game exe process
            this.processWatcher = new Thread(new ThreadStart(this.WatchProcess));
            this.processWatcher.Name = "Game Process Service Worker";
            this.processWatcher.Start();
        }

        /// <summary>
        /// Checks for "Rocksmith2014" processes currently executing. If found, populates the GameProcess member with the 
        /// handle. Sets up an event handler for process exiting.
        /// </summary>
        private void FindGameProcess()
        {
            var processes = Process.GetProcessesByName("Rocksmith2014");

            if (processes.Length == 0)
            {
                this.Status = GameProcessStatus.NOT_RUNNING;
            }
            else
            {
                //Select the first rocksmith process and open a handle
                this.GameProcess = processes[0];

                Logger.Log("Rocksmith found! Sniffing...");
                if (this.CheckProcessHash())
                {
                    InvokeNewGameProcess(this.GameProcess);
                }
            }
        }

        private void InvokeNewGameProcess(Process gameProcess)
        {
            //this.GameProcessChanged?.BeginInvoke(this, new GameProcessChangedEventArgs(this.GameProcess), this.EndInvokeNewGameProcess, null);
            this.GameProcessChanged?.Invoke(this, new GameProcessChangedEventArgs(this.GameProcess));
        }

        private void EndInvokeNewGameProcess(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (OnGameProcessChanged)ar.AsyncDelegate;

            try
            {
                invokedMethod.EndInvoke(iar);
            }
            catch
            {
                // Handle any exceptions that were thrown by the invoked method
                Console.WriteLine("GameProcessService: GameProcessChanged handler threw an exception");
            }
        }

        private bool CheckProcessHash()
        {
            //Check rocksmith executable hash to make sure its the correct version
            string hash = PSARCUtil.GetFileHash(new FileInfo(this.GameProcess.MainModule.FileName));

            Logger.Log($"Rocksmith executable hash: {hash}");

            if (!hash.Equals(GameProcessHash))
            {
                Logger.LogError("Executable hash does not match expected hash, make sure you have the correct version");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void WatchProcess()
        {
            try
            {
                Logger.Log("GameProcessWatcher: Starting game process watcher worker thread.");

                while (true)
                {
                    GameProcessStatus currentStatus = this.Status;
                    Process currentProcess = this.GameProcess;

                    if (this.GameProcess != null)
                    {
                        if (this.GameProcess.Responding)
                        {
                            this.Status = GameProcessStatus.RUNNING;
                        }
                        else if (!this.GameProcess.Responding && !this.GameProcess.HasExited)
                        {
                            this.Status = GameProcessStatus.NOT_RESPONDING;
                        }
                        else if (this.GameProcess.HasExited)
                        {
                            this.Status = GameProcessStatus.EXITED;
                            this.GameProcess = null;

                            this.FindGameProcess();
                        }
                    }
                    else
                    {
                        this.Status = GameProcessStatus.NOT_RUNNING;
                        this.FindGameProcess();
                    }

                    if (currentStatus != this.Status)
                    {
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusForDisplay"));
                        Logger.Log($"GameProcessWatcher: New process status {this.StatusForDisplay}.");
                    }

                    if (currentProcess != this.GameProcess)
                    {
                        Logger.Log("GameProcessWatcher: Located new game process.");
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameProcess"));
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Log("GameProcessWatcher: Shutting down game process watcher worker thread.");
            }
        }

        public void Dispose()
        {
            if (this.processWatcher != null)
            {
                this.processWatcher.Abort();
            }

            if (this.GameProcess != null)
            {
                this.GameProcess.Dispose();
                this.GameProcess = null;
            }
        }
    }
}
