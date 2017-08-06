using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gMKVToolNix.Controls
{
    public static class ExceptionExtensions
    {
        public static void ShowException(this Exception ex)
        {
            Debug.WriteLine(ex);
            MessageBox.Show(String.Format("An exception has occured!\r\n\r\n{0}", ex.Message), "An exception has occured!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
