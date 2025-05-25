using System;
using System.Diagnostics;
using System.Windows.Forms;
using gMKVToolNix.Controls;

namespace gMKVToolNix
{
    public class gTextBox:TextBox
    {
        public gTextBox()
            : base()
        {
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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
