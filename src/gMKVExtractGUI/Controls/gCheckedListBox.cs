using System.Runtime.Versioning;
using System.Windows.Forms;


namespace gMKVToolNix
{
    [SupportedOSPlatform("windows")]
    public class gCheckedListBox: System.Windows.Forms.CheckedListBox
    {
        public gCheckedListBox()
            : base()
        {
            this.DoubleBuffered = true;
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.Opaque, true);            
        }
    }
}
