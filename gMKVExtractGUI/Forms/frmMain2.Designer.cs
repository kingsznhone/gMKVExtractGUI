namespace gMKVToolNix.Forms
{
    partial class frmMain2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.prgBrStatus = new System.Windows.Forms.ToolStripProgressBar();
            this.tlpMain = new gMKVToolNix.gTableLayoutPanel();
            this.grpActions = new gMKVToolNix.gGroupBox();
            this.chkShowPopup = new System.Windows.Forms.CheckBox();
            this.btnExtract = new System.Windows.Forms.Button();
            this.lblExtractionMode = new System.Windows.Forms.Label();
            this.cmbExtractionMode = new System.Windows.Forms.ComboBox();
            this.btnShowLog = new System.Windows.Forms.Button();
            this.lblChapterType = new System.Windows.Forms.Label();
            this.cmbChapterType = new System.Windows.Forms.ComboBox();
            this.grpOutputDirectory = new gMKVToolNix.gGroupBox();
            this.chkLockOutputDirectory = new System.Windows.Forms.CheckBox();
            this.btnBrowseOutputDirectory = new System.Windows.Forms.Button();
            this.txtOutputDirectory = new gMKVToolNix.gTextBox();
            this.grpConfig = new gMKVToolNix.gGroupBox();
            this.txtMKVToolnixPath = new gMKVToolNix.gTextBox();
            this.btnBrowseMKVToolnixPath = new System.Windows.Forms.Button();
            this.grpInputFiles = new gMKVToolNix.gGroupBox();
            this.trvInputFiles = new gMKVToolNix.gTreeView();
            this.grpInputFileInfo = new gMKVToolNix.gGroupBox();
            this.txtSegmentInfo = new gMKVToolNix.gRichTextBox();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnAbortAll = new System.Windows.Forms.Button();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.statusStrip1.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.grpActions.SuspendLayout();
            this.grpOutputDirectory.SuspendLayout();
            this.grpConfig.SuspendLayout();
            this.grpInputFiles.SuspendLayout();
            this.grpInputFileInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.prgBrStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 525);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(624, 36);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // prgBrStatus
            // 
            this.prgBrStatus.AutoSize = false;
            this.prgBrStatus.Name = "prgBrStatus";
            this.prgBrStatus.Size = new System.Drawing.Size(200, 30);
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.grpActions, 0, 4);
            this.tlpMain.Controls.Add(this.grpOutputDirectory, 0, 3);
            this.tlpMain.Controls.Add(this.grpConfig, 0, 0);
            this.tlpMain.Controls.Add(this.grpInputFiles, 0, 1);
            this.tlpMain.Controls.Add(this.grpInputFileInfo, 0, 2);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 5;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpMain.Size = new System.Drawing.Size(624, 525);
            this.tlpMain.TabIndex = 1;
            // 
            // grpActions
            // 
            this.grpActions.Controls.Add(this.chkShowPopup);
            this.grpActions.Controls.Add(this.btnExtract);
            this.grpActions.Controls.Add(this.lblExtractionMode);
            this.grpActions.Controls.Add(this.cmbExtractionMode);
            this.grpActions.Controls.Add(this.btnShowLog);
            this.grpActions.Controls.Add(this.lblChapterType);
            this.grpActions.Controls.Add(this.cmbChapterType);
            this.grpActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpActions.Location = new System.Drawing.Point(3, 468);
            this.grpActions.Name = "grpActions";
            this.grpActions.Size = new System.Drawing.Size(618, 54);
            this.grpActions.TabIndex = 8;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "Actions";
            // 
            // chkShowPopup
            // 
            this.chkShowPopup.AutoSize = true;
            this.chkShowPopup.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkShowPopup.Location = new System.Drawing.Point(72, 25);
            this.chkShowPopup.Name = "chkShowPopup";
            this.chkShowPopup.Size = new System.Drawing.Size(61, 19);
            this.chkShowPopup.TabIndex = 12;
            this.chkShowPopup.Text = "Popup";
            this.chkShowPopup.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkShowPopup.UseVisualStyleBackColor = true;
            // 
            // btnExtract
            // 
            this.btnExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtract.Location = new System.Drawing.Point(521, 18);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(90, 30);
            this.btnExtract.TabIndex = 10;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            // 
            // lblExtractionMode
            // 
            this.lblExtractionMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExtractionMode.AutoSize = true;
            this.lblExtractionMode.Location = new System.Drawing.Point(360, 26);
            this.lblExtractionMode.Name = "lblExtractionMode";
            this.lblExtractionMode.Size = new System.Drawing.Size(45, 15);
            this.lblExtractionMode.TabIndex = 9;
            this.lblExtractionMode.Text = "Extract:";
            // 
            // cmbExtractionMode
            // 
            this.cmbExtractionMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbExtractionMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExtractionMode.FormattingEnabled = true;
            this.cmbExtractionMode.Location = new System.Drawing.Point(411, 22);
            this.cmbExtractionMode.Name = "cmbExtractionMode";
            this.cmbExtractionMode.Size = new System.Drawing.Size(102, 23);
            this.cmbExtractionMode.TabIndex = 8;
            // 
            // btnShowLog
            // 
            this.btnShowLog.Location = new System.Drawing.Point(6, 18);
            this.btnShowLog.Name = "btnShowLog";
            this.btnShowLog.Size = new System.Drawing.Size(60, 30);
            this.btnShowLog.TabIndex = 6;
            this.btnShowLog.Text = "Log...";
            this.btnShowLog.UseVisualStyleBackColor = true;
            // 
            // lblChapterType
            // 
            this.lblChapterType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChapterType.AutoSize = true;
            this.lblChapterType.Location = new System.Drawing.Point(228, 26);
            this.lblChapterType.Name = "lblChapterType";
            this.lblChapterType.Size = new System.Drawing.Size(49, 15);
            this.lblChapterType.TabIndex = 3;
            this.lblChapterType.Text = "Chapter";
            // 
            // cmbChapterType
            // 
            this.cmbChapterType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbChapterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChapterType.FormattingEnabled = true;
            this.cmbChapterType.Location = new System.Drawing.Point(281, 22);
            this.cmbChapterType.Name = "cmbChapterType";
            this.cmbChapterType.Size = new System.Drawing.Size(52, 23);
            this.cmbChapterType.TabIndex = 2;
            // 
            // grpOutputDirectory
            // 
            this.grpOutputDirectory.Controls.Add(this.chkLockOutputDirectory);
            this.grpOutputDirectory.Controls.Add(this.btnBrowseOutputDirectory);
            this.grpOutputDirectory.Controls.Add(this.txtOutputDirectory);
            this.grpOutputDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOutputDirectory.Location = new System.Drawing.Point(3, 408);
            this.grpOutputDirectory.Name = "grpOutputDirectory";
            this.grpOutputDirectory.Size = new System.Drawing.Size(618, 54);
            this.grpOutputDirectory.TabIndex = 7;
            this.grpOutputDirectory.TabStop = false;
            this.grpOutputDirectory.Text = "Output Directory for Selected File (you can drag and drop the directory)";
            // 
            // chkLockOutputDirectory
            // 
            this.chkLockOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLockOutputDirectory.AutoSize = true;
            this.chkLockOutputDirectory.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkLockOutputDirectory.Location = new System.Drawing.Point(474, 24);
            this.chkLockOutputDirectory.Name = "chkLockOutputDirectory";
            this.chkLockOutputDirectory.Size = new System.Drawing.Size(51, 19);
            this.chkLockOutputDirectory.TabIndex = 4;
            this.chkLockOutputDirectory.Text = "Lock";
            this.chkLockOutputDirectory.UseVisualStyleBackColor = true;
            // 
            // btnBrowseOutputDirectory
            // 
            this.btnBrowseOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutputDirectory.Location = new System.Drawing.Point(531, 18);
            this.btnBrowseOutputDirectory.Name = "btnBrowseOutputDirectory";
            this.btnBrowseOutputDirectory.Size = new System.Drawing.Size(80, 30);
            this.btnBrowseOutputDirectory.TabIndex = 3;
            this.btnBrowseOutputDirectory.Text = "Browse...";
            this.btnBrowseOutputDirectory.UseVisualStyleBackColor = true;
            // 
            // txtOutputDirectory
            // 
            this.txtOutputDirectory.AllowDrop = true;
            this.txtOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputDirectory.Location = new System.Drawing.Point(6, 22);
            this.txtOutputDirectory.Name = "txtOutputDirectory";
            this.txtOutputDirectory.ShortcutsEnabled = false;
            this.txtOutputDirectory.Size = new System.Drawing.Size(462, 23);
            this.txtOutputDirectory.TabIndex = 2;
            this.txtOutputDirectory.WordWrap = false;
            // 
            // grpConfig
            // 
            this.grpConfig.Controls.Add(this.txtMKVToolnixPath);
            this.grpConfig.Controls.Add(this.btnBrowseMKVToolnixPath);
            this.grpConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpConfig.Location = new System.Drawing.Point(3, 3);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(618, 54);
            this.grpConfig.TabIndex = 0;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "MKVToolnix Directory (you can drag and drop the directory)";
            // 
            // txtMKVToolnixPath
            // 
            this.txtMKVToolnixPath.AllowDrop = true;
            this.txtMKVToolnixPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMKVToolnixPath.Location = new System.Drawing.Point(9, 22);
            this.txtMKVToolnixPath.Name = "txtMKVToolnixPath";
            this.txtMKVToolnixPath.ReadOnly = true;
            this.txtMKVToolnixPath.ShortcutsEnabled = false;
            this.txtMKVToolnixPath.Size = new System.Drawing.Size(519, 23);
            this.txtMKVToolnixPath.TabIndex = 7;
            this.txtMKVToolnixPath.WordWrap = false;
            // 
            // btnBrowseMKVToolnixPath
            // 
            this.btnBrowseMKVToolnixPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseMKVToolnixPath.Location = new System.Drawing.Point(531, 17);
            this.btnBrowseMKVToolnixPath.Name = "btnBrowseMKVToolnixPath";
            this.btnBrowseMKVToolnixPath.Size = new System.Drawing.Size(80, 30);
            this.btnBrowseMKVToolnixPath.TabIndex = 6;
            this.btnBrowseMKVToolnixPath.Text = "Browse...";
            this.btnBrowseMKVToolnixPath.UseVisualStyleBackColor = true;
            // 
            // grpInputFiles
            // 
            this.grpInputFiles.Controls.Add(this.trvInputFiles);
            this.grpInputFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInputFiles.Location = new System.Drawing.Point(3, 63);
            this.grpInputFiles.Name = "grpInputFiles";
            this.grpInputFiles.Size = new System.Drawing.Size(618, 239);
            this.grpInputFiles.TabIndex = 1;
            this.grpInputFiles.TabStop = false;
            this.grpInputFiles.Text = "Input Files (you can drag and drop files or directories)";
            // 
            // trvInputFiles
            // 
            this.trvInputFiles.AllowDrop = true;
            this.trvInputFiles.CheckBoxes = true;
            this.trvInputFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvInputFiles.Location = new System.Drawing.Point(3, 19);
            this.trvInputFiles.Name = "trvInputFiles";
            this.trvInputFiles.ShowNodeToolTips = true;
            this.trvInputFiles.Size = new System.Drawing.Size(612, 217);
            this.trvInputFiles.TabIndex = 0;
            // 
            // grpInputFileInfo
            // 
            this.grpInputFileInfo.AllowDrop = true;
            this.grpInputFileInfo.Controls.Add(this.txtSegmentInfo);
            this.grpInputFileInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInputFileInfo.Location = new System.Drawing.Point(3, 308);
            this.grpInputFileInfo.Name = "grpInputFileInfo";
            this.grpInputFileInfo.Size = new System.Drawing.Size(618, 94);
            this.grpInputFileInfo.TabIndex = 6;
            this.grpInputFileInfo.TabStop = false;
            this.grpInputFileInfo.Text = "Selected File Information";
            // 
            // txtSegmentInfo
            // 
            this.txtSegmentInfo.DetectUrls = false;
            this.txtSegmentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSegmentInfo.Location = new System.Drawing.Point(3, 19);
            this.txtSegmentInfo.Name = "txtSegmentInfo";
            this.txtSegmentInfo.ReadOnly = true;
            this.txtSegmentInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtSegmentInfo.ShortcutsEnabled = false;
            this.txtSegmentInfo.Size = new System.Drawing.Size(612, 72);
            this.txtSegmentInfo.TabIndex = 1;
            this.txtSegmentInfo.Text = "";
            // 
            // btnAbort
            // 
            this.btnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbort.Location = new System.Drawing.Point(508, 529);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(92, 30);
            this.btnAbort.TabIndex = 12;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            // 
            // btnAbortAll
            // 
            this.btnAbortAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbortAll.Location = new System.Drawing.Point(410, 529);
            this.btnAbortAll.Name = "btnAbortAll";
            this.btnAbortAll.Size = new System.Drawing.Size(92, 30);
            this.btnAbortAll.TabIndex = 13;
            this.btnAbortAll.Text = "Abort All";
            this.btnAbortAll.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 26);
            // 
            // frmMain2
            // 
            this.ClientSize = new System.Drawing.Size(624, 561);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnAbortAll);
            this.Controls.Add(this.tlpMain);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.Name = "frmMain2";
            this.Text = "gMKVExtractGUI";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.grpActions.ResumeLayout(false);
            this.grpActions.PerformLayout();
            this.grpOutputDirectory.ResumeLayout(false);
            this.grpOutputDirectory.PerformLayout();
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.grpInputFiles.ResumeLayout(false);
            this.grpInputFileInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private gTableLayoutPanel tlpMain;
        private gGroupBox grpConfig;
        private System.Windows.Forms.Button btnBrowseMKVToolnixPath;
        private gTextBox txtMKVToolnixPath;
        private gGroupBox grpInputFiles;
        private gMKVToolNix.gTreeView trvInputFiles;
        private System.Windows.Forms.ToolStripProgressBar prgBrStatus;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnAbortAll;
        private gGroupBox grpInputFileInfo;
        private gRichTextBox txtSegmentInfo;
        private gGroupBox grpOutputDirectory;
        private System.Windows.Forms.CheckBox chkLockOutputDirectory;
        private System.Windows.Forms.Button btnBrowseOutputDirectory;
        private gTextBox txtOutputDirectory;
        private gGroupBox grpActions;
        private System.Windows.Forms.CheckBox chkShowPopup;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Label lblExtractionMode;
        private System.Windows.Forms.ComboBox cmbExtractionMode;
        private System.Windows.Forms.Button btnShowLog;
        private System.Windows.Forms.Label lblChapterType;
        private System.Windows.Forms.ComboBox cmbChapterType;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    }
}
