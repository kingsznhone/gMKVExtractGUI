using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace gMKVToolNix.Controls
{
    public static class ControlExtensions
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing(this Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }

        public static void ResumeDrawing(this Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Invalidate(true);
        }

        public static Boolean GetDesignMode(this Control control)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            PropertyInfo prop = control.GetType().GetProperty("DesignMode", bindFlags);
            return (Boolean)prop.GetValue(control, null);
        }

        public static Boolean IsInDesignMode(this Control control)
        {
            Control parent = control.Parent;
            while (parent != null)
            {
                if (parent.GetDesignMode())
                {
                    return true;
                }
                parent = parent.Parent;
            }
            return control.GetDesignMode();
        }
    }
}
