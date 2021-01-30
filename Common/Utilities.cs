using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RockSnifferGui.Common
{
    public class Utilities
    {
        public static void ShowExceptionMessageBox(Exception ex)
        {
            MessageBox.Show(ex.Message + ex.StackTrace, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
