using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using gMKVToolNix.Controls;

namespace gMKVToolNix
{
    public class gRichTextBox : RichTextBox
    {
        public bool DarkMode { get; set; } = false;

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

        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;
            const int WM_PRINTCLIENT = 0x0318;
            const int WM_ERASEBKGND = 0x0014;

            if (DarkMode)
            {
                if (!Enabled && (m.Msg == WM_PAINT || m.Msg == WM_PRINTCLIENT))
                {
                    // Handle the paint message ourselves when disabled
                    var ps = new PAINTSTRUCT();
                    var hdc = BeginPaint(m.HWnd, ref ps);

                    using (var graphics = Graphics.FromHdc(hdc))
                    {
                        PaintDisabledRichTextBox(graphics);
                    }

                    EndPaint(m.HWnd, ref ps);
                    m.Result = IntPtr.Zero;
                    return;
                }
                else if (!Enabled && m.Msg == WM_ERASEBKGND)
                {
                    // Handle background erasing when disabled
                    using (var graphics = Graphics.FromHdc(m.WParam))
                    using (var brush = new SolidBrush(BackColor))
                    {
                        graphics.FillRectangle(brush, ClientRectangle);
                    }
                    m.Result = new IntPtr(1);
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void PaintDisabledRichTextBox(Graphics g)
        {
            // Fill background
            using (var backBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(backBrush, ClientRectangle);
            }

            // Draw text
            if (!string.IsNullOrEmpty(Text))
            {
                var textRect = new Rectangle(4, 4, Width - 8, Height - 8);
                TextRenderer.DrawText(g, Text, Font, textRect,
                    ForeColor, BackColor,
                    TextFormatFlags.Left | TextFormatFlags.Top |
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            }

            // Draw border if needed
            using (var borderPen = new Pen(Color.FromArgb(100, 100, 100)))
            {
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr BeginPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);
    }
}
