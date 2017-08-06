using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gMKVToolNix.Controls
{
    public class gComboBox : ComboBox
    {
        protected ContextMenuStrip _ContextMenu = new ContextMenuStrip();
        protected ToolStripMenuItem _ClearMenu = new ToolStripMenuItem("Clear");
        protected ToolStripMenuItem _CopyMenu = new ToolStripMenuItem("Copy");

        public gComboBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            InitializeComponent();

            _ContextMenu.Items.Add(_CopyMenu);
            _ContextMenu.Items.Add(_ClearMenu);
            this.ContextMenuStrip = _ContextMenu;

            _CopyMenu.Click += _CopyMenu_Click;
            _ClearMenu.Click += _ClearMenu_Click;
        }

        void _ClearMenu_Click(object sender, EventArgs e)
        {
            try
            {
                this.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ex.ShowException();
            }
        }

        void _CopyMenu_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.SelectedIndex > -1)
                {
                    Clipboard.SetText(this.Text);
                }
            }
            catch (Exception ex)
            {
                ex.ShowException();
            }
        }

        protected void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // gComboBox
            // 
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.Size = new System.Drawing.Size(121, 21);
            this.DropDown += new System.EventHandler(this.gComboBox_DropDown);
            this.ResumeLayout(false);
        }

        protected void AutosizeDropDownWidth()
        {
            float longestItem = 0;
            // Find the longest text from the items list, in order to define the width
            using (Graphics g = Graphics.FromHwnd(this.Handle))
            {
                foreach (var item in Items)
                {
                    float itemWidth = g.MeasureString(GetItemText(item), Font).Width;
                    if (itemWidth > longestItem)
                    {
                        longestItem = itemWidth;
                    }
                }
            }
            // If there is a ScrollBar, then increase the width by 15 pixels
            if (Items.Count > MaxDropDownItems)
            {
                longestItem += 15;
            }

            // Change the width of the items list, byt never make it smaller than the width of the control
            DropDownWidth = Convert.ToInt32(Math.Max(longestItem, Width));
        }

        protected void gComboBox_DropDown(object sender, EventArgs e)
        {
            try
            {
                AutosizeDropDownWidth();
            }
            catch (Exception ex)
            {
                ex.ShowException();
            }
        }
    }
}
