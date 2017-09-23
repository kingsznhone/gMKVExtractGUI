using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace gMKVToolNix.Forms
{
    public enum TrackSelectionMode
    {
        video,
        audio,
        subtitle,
        chapter,
        attachment,
        all,
        none
    }

    public delegate void UpdateProgressDelegate(Object val);
    public delegate void UpdateTrackLabelDelegate(Object val);

    public partial class frmMain2 : gForm
    {
        private frmLog _LogForm = null;
        private frmJobManager _JobManagerForm = null;
        private gSettings _Settings = null;

        private Boolean _FromConstructor = false;

        public frmMain2()
        {
            try
            {
                _FromConstructor = true;

                InitializeComponent();

                // Set form icon from the executing assembly
                Icon = Icon.ExtractAssociatedIcon(this.GetExecutingAssemblyLocation());

                // Set form title 
                Text = String.Format("gMKVExtractGUI v{0} -- By Gpower2", this.GetCurrentVersion());

                // Load settings
                _Settings = new gSettings(this.GetCurrentDirectory());
                _Settings.Reload();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _FromConstructor = false;
                ShowErrorMessage(ex.Message);
            }
        }
    }
}
