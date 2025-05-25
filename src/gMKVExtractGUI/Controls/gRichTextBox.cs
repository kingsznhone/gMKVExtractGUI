using System;
using System.Diagnostics;
using System.Windows.Forms;
using gMKVToolNix.Controls;

namespace gMKVToolNix
{
    public class gRichTextBox : RichTextBox
    {
        public gRichTextBox()
            : base()
        {
            this.DoubleBuffered = true;
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.ShortcutsEnabled = false;
            this.DetectUrls = false;            
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            try
            {
                if (e.Control && e.KeyCode == Keys.A)
                {
                    this.SelectAll();
                }
                else if (e.Control && e.KeyCode == Keys.C)
                {
                    if (!string.IsNullOrWhiteSpace(this.SelectedText))
                    {
                        Clipboard.SetText(this.SelectedText, TextDataFormat.UnicodeText);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ex.ShowException(this.FindForm());
            }
        }
    }
}
