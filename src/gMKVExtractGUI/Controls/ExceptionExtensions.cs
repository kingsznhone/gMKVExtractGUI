using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace gMKVToolNix.Controls
{
    [SupportedOSPlatform("windows")]
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Displays a messagebox containing the message of the exception and aldo writes the exception stacktrace to the Debug console
        /// </summary>
        /// <param name="ex"></param>
        public static void ShowException(this Exception ex, Form form)
        {
            Debug.WriteLine(ex);
            MessageBox.Show(
                form,
                $"An exception has occured!{Environment.NewLine}{Environment.NewLine}{ex.Message}", 
                "An exception has occured!", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);
        }
    }
}
