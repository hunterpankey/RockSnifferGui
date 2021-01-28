using RockSnifferLib.Logging;
using RockSnifferLib.RSHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockSnifferGui.Services
{
    public class GameProcessService
    {
        public enum ProcessStatus { NOT_RUNNING, RUNNING, NOT_RESPONDING, EXITED }
        private const string GameProcessHash = "GxT+/TXLpUFys+Cysek8zg==";

        public delegate void OnGameProcessChanged(object sender, GameProcessChangedEventArgs args);
        public event OnGameProcessChanged GameProcessChanged;

        private Process gameProcess;
        private ProcessStatus gameProcessStatus;
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
        public ProcessStatus GameProcessStatus { get => gameProcessStatus; private set => gameProcessStatus = value; }

        public GameProcessService()
        {
            this.FindGameProcess();

            // start the process watcher thread to track the game exe process
            this.processWatcher = new Thread(new ThreadStart(this.WatchProcess));
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
                this.GameProcessStatus = ProcessStatus.NOT_RUNNING;
            }
            else
            {
                //Select the first rocksmith process and open a handle
                this.GameProcess = processes[0];

                Logger.Log("Rocksmith found! Sniffing...");
                if (this.CheckProcessHash())
                {
                    this.GameProcess.Exited += GameProcess_Exited;
                    InvokeNewGameProcess(this.GameProcess);
                }
            }
        }

        private void InvokeNewGameProcess(Process gameProcess)
        {
            this.GameProcessChanged?.BeginInvoke(this, new GameProcessChangedEventArgs(this.GameProcess), this.EndInvokeNewGameProcess, null);
            //this.GameProcessChanged?.Invoke(this, new GameProcessChangedEventArgs(this.GameProcess));
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

        private void GameProcess_Exited(object sender, EventArgs e)
        {
            this.GameProcessStatus = ProcessStatus.EXITED;
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
                    if (this.GameProcess != null)
                    {
                        if (this.GameProcess.Responding)
                        {
                            this.GameProcessStatus = ProcessStatus.RUNNING;
                        }
                        else if (!this.GameProcess.Responding && !this.GameProcess.HasExited)
                        {
                            Logger.Log("GameProcessWatcher: Game process is not responding.");
                            this.GameProcessStatus = ProcessStatus.NOT_RESPONDING;
                        }
                        else if (this.GameProcess.HasExited)
                        {
                            Logger.Log("GameProcessWatcher: Game process has exited.");
                            this.GameProcessStatus = ProcessStatus.EXITED;
                            this.GameProcess = null;

                            this.FindGameProcess();
                        }
                    }
                    else
                    {
                        this.GameProcessStatus = ProcessStatus.NOT_RUNNING;
                        this.FindGameProcess();
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Log("GameProcessWatcher: Shutting down game process watcher worker thread.");
            }
        }

        public void Shutdown()
        {
            if(this.processWatcher != null)
            {
                this.processWatcher.Abort();
            }
        }
    }
}
