using System.Diagnostics;

namespace RockSnifferGui.Services
{
    public class GameProcessChangedEventArgs
    {
        private Process process;

        public Process Process { get => process; private set => process = value; }

        public GameProcessChangedEventArgs(Process process)
        {
            this.Process = process;
        }
    }
}