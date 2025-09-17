using System.Runtime.Versioning;
using System.Windows.Forms;

namespace gMKVToolNix.Controls
{
    [SupportedOSPlatform("windows")]
    public class gListBox:ListBox
    {
        public gListBox()
            : base()
        {
            this.DoubleBuffered = true;
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.Opaque, true);            
        }
    }
}
