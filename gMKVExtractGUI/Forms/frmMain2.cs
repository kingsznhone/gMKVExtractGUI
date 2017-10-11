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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Media;

namespace gMKVToolNix.Forms
{
    public enum TrackSelectionMode
    {
        video,
        audio,
        subtitle,
        chapter,
        attachment,
        all
    }

    public delegate void UpdateProgressDelegate(Object val);
    public delegate void UpdateTrackLabelDelegate(Object filename, Object val);

    public partial class frmMain2 : gForm, IFormMain
    {
        private frmLog _LogForm = null;
        private frmJobManager _JobManagerForm = null;

        private gMKVExtract _gMkvExtract = null;

        private gSettings _Settings = null;

        private Boolean _FromConstructor = false;

        private Boolean _ExtractRunning = false;

        private Int32 _CurrentJob = 0;
        private Int32 _TotalJobs = 0;

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

                btnAbort.Enabled = false;
                btnAbortAll.Enabled = false;

                cmbChapterType.DataSource = Enum.GetNames(typeof(MkvChapterTypes));
                cmbExtractionMode.DataSource = Enum.GetNames(typeof(FormMkvExtractionMode));

                ClearStatus();

                // Load settings
                _Settings = new gSettings(this.GetCurrentDirectory());
                _Settings.Reload();

                // Set form size and position from settings
                gMKVLogger.Log("Begin setting form size and position from settings...");
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(_Settings.WindowPosX, _Settings.WindowPosY);
                this.Size = new System.Drawing.Size(_Settings.WindowSizeWidth, _Settings.WindowSizeHeight);
                this.WindowState = _Settings.WindowState;
                gMKVLogger.Log("Finished setting form size and position from settings!");

                // Set chapter type, output directory and job mode from settings
                gMKVLogger.Log("Begin setting chapter type, output directory and job mode from settings...");
                cmbChapterType.SelectedItem = Enum.GetName(typeof(MkvChapterTypes), _Settings.ChapterType);
                txtOutputDirectory.Text = _Settings.OutputDirectory;
                chkUseSourceDirectory.Checked = _Settings.LockedOutputDirectory;
                chkShowPopup.Checked = _Settings.ShowPopup;
                gMKVLogger.Log("Finished setting chapter type, output directory and job mode from settings!");

                _FromConstructor = false;

                // Find MKVToolnix path
                try
                {
                    if (!gMKVHelper.IsOnLinux)
                    {
                        // When on Windows, check the registry first
                        gMKVLogger.Log("Checking registry for mkvmerge...");
                        txtMKVToolnixPath.Text = gMKVHelper.GetMKVToolnixPathViaRegistry();
                    }
                    else
                    {
                        // When on Linux, check the usr/bin first
                        if (File.Exists(Path.Combine("/usr", "bin", gMKVHelper.MKV_MERGE_GUI_FILENAME))
                            || File.Exists(Path.Combine("/usr", "bin", gMKVHelper.MKV_MERGE_NEW_GUI_FILENAME)))
                        {
                            txtMKVToolnixPath.Text = Path.Combine("/usr", "bin");
                        }
                        else
                        {
                            throw new Exception(String.Format("mkvmerge was not found in path {0}!", Path.Combine("usr", "bin")));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    gMKVLogger.Log(ex.Message);
                    // MKVToolnix could not be found in registry
                    // check in the current directory
                    if (File.Exists(Path.Combine(GetCurrentDirectory(), gMKVHelper.MKV_MERGE_GUI_FILENAME))
                        || File.Exists(Path.Combine(GetCurrentDirectory(), gMKVHelper.MKV_MERGE_NEW_GUI_FILENAME)))
                    {
                        txtMKVToolnixPath.Text = GetCurrentDirectory();
                    }
                    else
                    {
                        // check for ini file
                        if (File.Exists(Path.Combine(_Settings.MkvToolnixPath, gMKVHelper.MKV_MERGE_GUI_FILENAME))
                            || File.Exists(Path.Combine(_Settings.MkvToolnixPath, gMKVHelper.MKV_MERGE_NEW_GUI_FILENAME)))
                        {
                            _FromConstructor = true;
                            txtMKVToolnixPath.Text = _Settings.MkvToolnixPath;
                            _FromConstructor = false;
                        }
                        else
                        {
                            // Select exception message according to running OS
                            String exceptionMessage = "";
                            if (gMKVHelper.IsOnLinux)
                            {
                                exceptionMessage = "Could not find MKVToolNix in /usr/bin, or in the current directory, or in the ini file!";
                            }
                            else
                            {
                                exceptionMessage = "Could not find MKVToolNix in registry, or in the current directory, or in the ini file!";
                            }
                            gMKVLogger.Log(exceptionMessage);
                            throw new Exception(exceptionMessage + Environment.NewLine + "Please download and reinstall or provide a manual path!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _FromConstructor = false;
                ShowErrorMessage(ex.Message);
            }
        }

        private void frmMain2_Shown(object sender, EventArgs e)
        {
            try
            {
                // check if user provided with a filename when executing the application
                string[] cmdArgs = Environment.GetCommandLineArgs();
                if (cmdArgs.Length > 1)
                {
                    // Copy the results to a list
                    List<String> arguments = cmdArgs.ToList();
                    // Remove the first argument (the executable)
                    arguments.RemoveAt(0);

                    gMKVLogger.Log(String.Format("Found command line arguments: {0}", string.Join(",", arguments)));


                    tlpMain.Enabled = false;
                    Cursor = Cursors.WaitCursor;
                    txtSegmentInfo.Text = "Getting files...";

                    // Get the file list
                    List<string> fileList = GetFilesFromInputFileDrop(arguments.ToArray());

                    // Check if any valid matroska files were provided
                    if (!fileList.Any())
                    {
                        throw new Exception("No valid matroska files were provided!");
                    }

                    // Add files to the TreeView
                    AddFileNodes(txtMKVToolnixPath.Text, fileList);

                    Cursor = Cursors.Default;
                    tlpMain.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                Cursor = Cursors.Default;
                ShowErrorMessage(ex.Message);
                tlpMain.Enabled = true;
            }
        }

        private void txt_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // check if the drop data is actually a file or folder
                if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    // check for sender
                    if (((gTextBox)sender) == txtMKVToolnixPath)
                    {
                        // check if MKVToolnix Path is already set
                        if (!String.IsNullOrWhiteSpace(txtMKVToolnixPath.Text))
                        {
                            if (ShowQuestion("Do you really want to change MKVToolnix path?", "Are you sure?") != DialogResult.Yes)
                            {
                                return;
                            }
                        }
                    }
                    else if (((gTextBox)sender) == txtOutputDirectory)
                    {
                        // check if output directory is the same as the source
                        if (chkUseSourceDirectory.Checked)
                        {
                            return;
                        }
                    }
                    String[] s = (String[])e.Data.GetData(DataFormats.FileDrop, false);
                    if (s != null && s.Length > 0)
                    {
                        ((gTextBox)sender).Text = s[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void txt_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    if (((gTextBox)sender) == txtOutputDirectory)
                    {
                        // check if output directory is the same as the source
                        if (chkUseSourceDirectory.Checked)
                        {
                            e.Effect = DragDropEffects.None;
                        }
                        else
                        {
                            // check if it is a directory or not
                            String[] s = (String[])e.Data.GetData(DataFormats.FileDrop);
                            if (s != null && s.Length > 0 && Directory.Exists(s[0]))
                            {
                                e.Effect = DragDropEffects.All;
                            }
                        }
                    }
                    else
                    {
                        e.Effect = DragDropEffects.All;
                    }
                }
                else
                    e.Effect = DragDropEffects.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowErrorMessage(ex.Message);
            }
        }

        private List<string> GetFilesFromInputFileDrop(string[] argFileDrop)
        {
            List<string> fileList = new List<string>();
            // Check if directories were provided
            if (argFileDrop.Any(f => Directory.Exists(f)))
            {
                // Check if they contain subdirectories
                List<string> subDirList = new List<string>();

                using (Task ta = Task.Factory.StartNew(() =>
                {
                    argFileDrop.Where(f => Directory.Exists(f)).ToList().ForEach(t => subDirList.AddRange(Directory.GetDirectories(t, "*", SearchOption.TopDirectoryOnly).ToList()));
                }))
                {
                    while (!ta.IsCompleted) { Application.DoEvents(); }
                    if (ta.Exception != null) { throw ta.Exception; }
                }

                if (subDirList.Any())
                {
                    Cursor = Cursors.Default;
                    var result = ShowQuestion("Do you want to include files in sub directories?", "Sub directories found!");
                    Cursor = Cursors.WaitCursor;
                    if (result == DialogResult.Yes)
                    {
                        using (Task ta = Task.Factory.StartNew(() =>
                        {
                            // Add the subdirectory files
                            argFileDrop.Where(f => Directory.Exists(f)).ToList().ForEach(t => fileList.AddRange(Directory.GetFiles(t, "*", SearchOption.AllDirectories).ToList()));
                        }))
                        {
                            while (!ta.IsCompleted) { Application.DoEvents(); }
                            if (ta.Exception != null) { throw ta.Exception; }
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        using (Task ta = Task.Factory.StartNew(() =>
                        {
                            // Add the top level directory files
                            argFileDrop.Where(f => Directory.Exists(f)).ToList().ForEach(t => fileList.AddRange(Directory.GetFiles(t, "*", SearchOption.TopDirectoryOnly).ToList()));
                        }))
                        {
                            while (!ta.IsCompleted) { Application.DoEvents(); }
                            if (ta.Exception != null) { throw ta.Exception; }
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        Cursor = Cursors.Default;
                        return new List<string>();
                    }
                }
                else
                {
                    using (Task ta = Task.Factory.StartNew(() =>
                    {
                        // Since there are no subdirectories, add the files from the directory
                        argFileDrop.Where(f => Directory.Exists(f)).ToList().ForEach(t => fileList.AddRange(Directory.GetFiles(t, "*", SearchOption.TopDirectoryOnly).ToList()));
                    }))
                    {
                        while (!ta.IsCompleted) { Application.DoEvents(); }
                        if (ta.Exception != null) { throw ta.Exception; }
                    }
                }
            }
            // Add the files provided
            argFileDrop.Where(f => File.Exists(f)).ToList().ForEach(t => fileList.Add(t));

            // Remove all non valid matroska files
            fileList.RemoveAll(f =>
                Path.GetExtension(f).ToLower() != ".mkv"
                && Path.GetExtension(f).ToLower() != ".mka"
                && Path.GetExtension(f).ToLower() != ".mks"
                && Path.GetExtension(f).ToLower() != ".mk3d"
                && Path.GetExtension(f).ToLower() != ".webm"
            );

            return fileList;
        }

        private void trvInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // check if the drop data is actually a file or folder
                if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    String[] s = (String[])e.Data.GetData(DataFormats.FileDrop, false);                    
                    if (s != null && s.Length > 0)
                    {
                        tlpMain.Enabled = false;
                        Cursor = Cursors.WaitCursor;
                        txtSegmentInfo.Text = "Getting files...";

                        // Get the file list
                        List<string> fileList = GetFilesFromInputFileDrop(s);

                        // Check if any valid matroska files were provided
                        if (!fileList.Any())
                        {
                            throw new Exception("No valid matroska files were provided!");
                        }

                        // Add files to the TreeView
                        AddFileNodes(txtMKVToolnixPath.Text, fileList);

                        Cursor = Cursors.Default;
                        tlpMain.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                Cursor = Cursors.Default;
                ShowErrorMessage(ex.Message);
                tlpMain.Enabled = true;
            }
        }

        private void trvInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e != null && e.Data != null)
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        e.Effect = DragDropEffects.All;
                    else
                        e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        internal class NodeResults
        {
            public List<TreeNode> Nodes { get; set; }
            public List<string> InformationMessages { get; set; }
            public List<string> WarningMessages { get; set; }
            public List<string> ErrorMessages { get; set; }

            public NodeResults()
            {
                InformationMessages = new List<string>();
                WarningMessages = new List<string>();
                ErrorMessages = new List<string>();
            }
        }

        private void AddFileNodes(String argMKVToolNixPath, List<String> argFiles, bool argAppend = false)
        {
            try
            {
                tlpMain.Enabled = false;
                Application.DoEvents();
                // empty all the controls in any case
                ClearControls();
                // Check for append file or not
                if (!argAppend)
                {
                    trvInputFiles.Nodes.Clear();
                }
                else
                {
                    // Remove files that already exist in the TreeView
                    argFiles.RemoveAll(f => trvInputFiles.AllNodes.Any(n => n != null && n.Tag != null && n.Tag is gMKVSegmentInfo && ((gMKVSegmentInfo)n.Tag).Path.ToLower().Equals(f.ToLower())));

                    // Check if there are any new files to add
                    if (!argFiles.Any())
                    {
                        throw new Exception("No new files to add!");
                    }
                }

                gTaskbarProgress.SetState(this, gTaskbarProgress.TaskbarStates.Indeterminate);

                NodeResults results = null;
                
                Task ta = Task.Factory.StartNew(() => {
                    results = GetFileInfoNodes(argMKVToolNixPath, argFiles);
                });

                while (!ta.IsCompleted)
                {
                    Application.DoEvents();
                }

                if (ta.Exception != null)
                {
                    gTaskbarProgress.SetState(this, gTaskbarProgress.TaskbarStates.Error);
                    throw ta.Exception;
                }

                // Add the nodes to the TreeView
                trvInputFiles.Nodes.AddRange(results.Nodes.ToArray());
                // Remove the check box from the nodes that contain the gMKVSegmentInfo
                trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVSegmentInfo).ToList().ForEach(n => trvInputFiles.SetIsCheckBoxVisible(n, false));

                trvInputFiles.ExpandAll();

                // Check for error messages
                if(results.ErrorMessages != null && results.ErrorMessages.Any())
                {
                    ShowErrorMessage(String.Join(Environment.NewLine, results.ErrorMessages));
                }
            }
            finally
            {
                prgBrStatus.Value = 0;
                lblStatus.Text = "";

                grpInputFiles.Text = String.Format("Input Files (you can drag and drop files or directories) ({0} files)",
                    trvInputFiles.AllNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVSegmentInfo));

                tlpMain.Enabled = true;
                gTaskbarProgress.SetState(this, gTaskbarProgress.TaskbarStates.NoProgress);
                Application.DoEvents();
            }
        }

        private NodeResults GetFileInfoNodes(String argMKVToolNixPath, List<String> argFiles)
        {
            NodeResults results = new NodeResults();
            List<TreeNode> fileNodes = new List<TreeNode>();            

            statusStrip.Invoke((MethodInvoker)delegate {
                prgBrStatus.Maximum = argFiles.Count;
            });
            Int32 counter = 0;

            foreach (var sf in argFiles.OrderBy(f => Path.GetDirectoryName(f)).ThenBy(f => Path.GetFileName(f)))
            {
                counter++;
                txtSegmentInfo.Invoke((MethodInvoker) delegate {
                    txtSegmentInfo.Text = String.Format("Analyzing {0}...", Path.GetFileName(sf));
                });

                statusStrip.Invoke((MethodInvoker)delegate {
                    prgBrStatus.Value = counter;
                    lblStatus.Text = String.Format("{0}%",
                        Convert.ToInt32((Convert.ToDouble(prgBrStatus.Value) / Convert.ToDouble(prgBrStatus.Maximum)) * 100.0));
                });

                try
                {
                    fileNodes.Add(GetFileNode(argMKVToolNixPath, sf));
                }
                catch (Exception ex)
                {
                    results.ErrorMessages.Add(String.Format("file: {0} error: {1}", Path.GetFileName(sf), ex.Message));
                }
            }

            txtSegmentInfo.Invoke((MethodInvoker)delegate {
                txtSegmentInfo.Clear();
            });

            results.Nodes = fileNodes;
            return results;
        }

        private TreeNode GetFileNode(String argMKVToolNixPath, String argFilename)
        {
            // Check if MKVToolNix path was provided
            if (String.IsNullOrWhiteSpace(argMKVToolNixPath))
            {
                throw new Exception("The MKVToolNix path was not provided!");
            }

            // Check if filename was provided
            if (String.IsNullOrWhiteSpace(argFilename))
            {
                throw new Exception("No filename was provided!");
            }

            // Check if file exists
            if (!File.Exists(argFilename))
            {
                throw new Exception(String.Format("The file {0} does not exist!", argFilename));
            }

            // Check if the extension is a valid matroska file
            String inputExtension = Path.GetExtension(argFilename).ToLower();
            if (inputExtension != ".mkv"
                && inputExtension != ".mka"
                && inputExtension != ".mks"
                && inputExtension != ".mk3d"
                && inputExtension != ".webm")
            {
                throw new Exception("The input file " + argFilename + Environment.NewLine + Environment.NewLine + "is not a valid matroska file!");
            }

            // get the file information                    
            List<gMKVSegment> segmentList = gMKVHelper.GetMergedMkvSegmentList(argMKVToolNixPath, argFilename);

            gMKVSegmentInfo segInfo = (gMKVSegmentInfo)segmentList.FirstOrDefault(s => s is gMKVSegmentInfo);

            TreeNode infoNode = new TreeNode(Path.GetFileName(argFilename));
            infoNode.Tag = segInfo;
            foreach (gMKVSegment seg in segmentList.Where(s => !(s is gMKVSegmentInfo)).ToList())
            {
                TreeNode segNode = new TreeNode(seg.ToString());
                segNode.Tag = seg;
                infoNode.Nodes.Add(segNode);
            }

            return infoNode;
        }

        private void trvInputFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if(trvInputFiles.SelectedNode != null)
                {
                    TreeNode selNode = trvInputFiles.SelectedNode;
                    if(selNode.Tag == null)
                    {
                        throw new Exception("Selected node has null tag!");
                    }
                    if(!(selNode.Tag is gMKVSegmentInfo))
                    {
                        // Get parent node
                        selNode = selNode.Parent;
                        if(selNode == null)
                        {
                            throw new Exception("Selected node has no parent node!");
                        }
                        if (selNode.Tag == null)
                        {
                            throw new Exception("Selected node has null tag!");
                        }
                        if (!(selNode.Tag is gMKVSegmentInfo))
                        {
                            throw new Exception("Selected node has no info!");
                        }
                    }
                    gMKVSegmentInfo seg = selNode.Tag as gMKVSegmentInfo;
                    txtSegmentInfo.Text = String.Format("Writing Application: {1}{0}Muxing Application: {2}{0}Duration: {3}{0}Date: {4}",
                        Environment.NewLine,
                        seg.WritingApplication,
                        seg.MuxingApplication,
                        seg.Duration,
                        seg.Date);

                    // check if output directory is the same as the source
                    if (chkUseSourceDirectory.Checked)
                    {
                        // set output directory to the source directory
                        txtOutputDirectory.Text = seg.Directory;
                    }

                    // Set the GroupBox title
                    grpSelectedFileInfo.Text = String.Format("Selected File Information ({0})", seg.Filename);
                }
                else
                {
                    txtSegmentInfo.Clear();
                    grpSelectedFileInfo.Text = "Selected File Information";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        void g_MkvExtractTrackUpdated(string filename, string trackName)
        {
            this.Invoke(new UpdateTrackLabelDelegate(UpdateTrackLabel), new object[] { filename, trackName });
        }

        void g_MkvExtractProgressUpdated(int progress)
        {
            this.Invoke(new UpdateProgressDelegate(UpdateProgress), new object[] { progress });
        }

        public void UpdateProgress(Object val)
        {
            prgBrStatus.Value = Convert.ToInt32(val);
            prgBrTotalStatus.Value = (_CurrentJob - 1) * 100 + Convert.ToInt32(val);
            lblStatus.Text = String.Format("{0}%", Convert.ToInt32(val));
            lblTotalStatus.Text = String.Format("{0}%", prgBrTotalStatus.Value / _TotalJobs);
            gTaskbarProgress.SetValue(this, Convert.ToUInt64(val), (UInt64)100);
            Application.DoEvents();
        }

        public void UpdateTrackLabel(Object filename, Object val)
        {
            txtSegmentInfo.Text = String.Format("Extracting {0} from {1}...", val, Path.GetFileName((string)filename));
            Application.DoEvents();
        }

        private void CheckNeccessaryInputFields(Boolean checkSelectedTracks, Boolean checkSelectedChapterType)
        {
            if (String.IsNullOrWhiteSpace(txtMKVToolnixPath.Text))
            {
                throw new Exception("You must provide with MKVToolnix path!");
            }
            if (!File.Exists(Path.Combine(txtMKVToolnixPath.Text.Trim(), gMKVHelper.MKV_MERGE_GUI_FILENAME))
                && !File.Exists(Path.Combine(txtMKVToolnixPath.Text.Trim(), gMKVHelper.MKV_MERGE_NEW_GUI_FILENAME)))
            {
                throw new Exception("The MKVToolnix path provided does not contain MKVToolnix files!");
            }

            // Get the checked nodes
            List<TreeNode> checkedNodes = trvInputFiles.CheckedNodes;

            if (checkSelectedTracks)
            {
                // Check if the checked nodes contain tracks
                if(!checkedNodes.Any(t => t.Tag != null && !(t.Tag is gMKVSegmentInfo)))
                {
                    throw new Exception("You must select a track to extract!");
                }

                FormMkvExtractionMode selectedExtractionMode = (FormMkvExtractionMode)Enum.Parse(typeof(FormMkvExtractionMode), (String)cmbExtractionMode.SelectedItem);

                if (selectedExtractionMode == FormMkvExtractionMode.Timecodes ||
                    selectedExtractionMode == FormMkvExtractionMode.Tracks_And_Timecodes ||
                    selectedExtractionMode == FormMkvExtractionMode.Tracks_And_Cues_And_Timecodes)
                {
                    // Check if the ckecked nodes contain video, audio or subtitle track
                    if (!checkedNodes.Any(t => t.Tag != null && (t.Tag is gMKVTrack)))
                    {
                        throw new Exception("You must select a video, audio or subtitles track to extract timecodes!");
                    }
                }

                if (selectedExtractionMode == FormMkvExtractionMode.Cues ||
                    selectedExtractionMode == FormMkvExtractionMode.Tracks_And_Cues ||
                    selectedExtractionMode == FormMkvExtractionMode.Tracks_And_Cues_And_Timecodes)
                {
                    // Check if the ckecked nodes contain video, audio or subtitle track
                    if (!checkedNodes.Any(t => t.Tag != null && (t.Tag is gMKVTrack)))
                    {
                        throw new Exception("You must select a video, audio or subtitles track to extract cues!");
                    }
                }
            }

            if (checkSelectedChapterType)
            {
                if (!checkedNodes.Any(t => t.Tag != null && (t.Tag is gMKVChapter)))
                {
                    if (cmbChapterType.SelectedIndex == -1)
                    {
                        throw new Exception("You must select a chapter type!");
                    }
                }
            }
        }

        private void btnExtract_btnAddJobs_Click(object sender, EventArgs e)
        {
            bool exceptionOccured = false;
            try
            {
                tlpMain.Enabled = false;
                _ExtractRunning = true;
                Application.DoEvents();
                _gMkvExtract.MkvExtractProgressUpdated += g_MkvExtractProgressUpdated;
                _gMkvExtract.MkvExtractTrackUpdated += g_MkvExtractTrackUpdated;

                FormMkvExtractionMode extractionMode = (FormMkvExtractionMode)Enum.Parse(typeof(FormMkvExtractionMode), (String)cmbExtractionMode.SelectedItem);

                // Check for necessary input fields 
                switch (extractionMode)
                {
                    case FormMkvExtractionMode.Tracks:
                        CheckNeccessaryInputFields(true, true);
                        break;
                    case FormMkvExtractionMode.Cue_Sheet:
                        CheckNeccessaryInputFields(false, false);
                        break;
                    case FormMkvExtractionMode.Tags:
                        CheckNeccessaryInputFields(false, false);
                        break;
                    case FormMkvExtractionMode.Timecodes:
                        CheckNeccessaryInputFields(true, false);
                        break;
                    case FormMkvExtractionMode.Tracks_And_Timecodes:
                        CheckNeccessaryInputFields(true, true);
                        break;
                    case FormMkvExtractionMode.Cues:
                        CheckNeccessaryInputFields(true, false);
                        break;
                    case FormMkvExtractionMode.Tracks_And_Cues:
                        CheckNeccessaryInputFields(true, false);
                        break;
                    case FormMkvExtractionMode.Tracks_And_Cues_And_Timecodes:
                        CheckNeccessaryInputFields(true, false);
                        break;
                }

                // Get all checked nodes
                List<TreeNode> checkedNodes = trvInputFiles.CheckedNodes;
                // Filter out the parent nodes
                checkedNodes.RemoveAll(t => t.Tag != null && t.Tag is gMKVSegmentInfo);
                
                // Get all the distinct parent nodes that correspond to the checked nodes
                List<TreeNode> parentNodes = checkedNodes.Where(t => t.Parent != null && t.Parent.Tag != null && t.Parent.Tag is gMKVSegmentInfo).Select(t => t.Parent).Distinct().ToList<TreeNode>();

                Thread myThread = null;
                List<gMKVJob> jobs = new List<gMKVJob>();
                List<gMKVSegment> segments = null;

                // For each file, we need a separate job
                foreach (TreeNode parentNode in parentNodes)
                {
                    gMKVSegmentInfo infoSegment = parentNode.Tag as gMKVSegmentInfo;
                    segments = checkedNodes.Where(n => n.Parent == parentNode).Select(t => t.Tag as gMKVSegment).ToList();
                    string outputDirectory = txtOutputDirectory.Text;
                    // Check if the output dir is the same as the source
                    if (chkUseSourceDirectory.Checked)
                    {
                        outputDirectory = infoSegment.Directory;
                    }
                    List<Object> parameterList = new List<object>();
                    switch (extractionMode)
                    {
                        case FormMkvExtractionMode.Tracks:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(segments);
                            parameterList.Add(outputDirectory);
                            parameterList.Add((MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem));
                            parameterList.Add(TimecodesExtractionMode.NoTimecodes);
                            parameterList.Add(CuesExtractionMode.NoCues);
                            break;
                        case FormMkvExtractionMode.Cue_Sheet:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(outputDirectory);
                            break;
                        case FormMkvExtractionMode.Tags:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(outputDirectory);
                            break;
                        case FormMkvExtractionMode.Timecodes:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(segments);
                            parameterList.Add(outputDirectory);
                            parameterList.Add((MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem));
                            parameterList.Add(TimecodesExtractionMode.OnlyTimecodes);
                            parameterList.Add(CuesExtractionMode.NoCues);
                            break;
                        case FormMkvExtractionMode.Tracks_And_Timecodes:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(segments);
                            parameterList.Add(outputDirectory);
                            parameterList.Add((MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem));
                            parameterList.Add(TimecodesExtractionMode.WithTimecodes);
                            parameterList.Add(CuesExtractionMode.NoCues);
                            break;
                        case FormMkvExtractionMode.Cues:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(segments);
                            parameterList.Add(outputDirectory);
                            parameterList.Add((MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem));
                            parameterList.Add(TimecodesExtractionMode.NoTimecodes);
                            parameterList.Add(CuesExtractionMode.OnlyCues);
                            break;
                        case FormMkvExtractionMode.Tracks_And_Cues:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(segments);
                            parameterList.Add(outputDirectory);
                            parameterList.Add((MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem));
                            parameterList.Add(TimecodesExtractionMode.NoTimecodes);
                            parameterList.Add(CuesExtractionMode.WithCues);
                            break;
                        case FormMkvExtractionMode.Tracks_And_Cues_And_Timecodes:
                            parameterList.Add(infoSegment.Path);
                            parameterList.Add(segments);
                            parameterList.Add(outputDirectory);
                            parameterList.Add((MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem));
                            parameterList.Add(TimecodesExtractionMode.WithTimecodes);
                            parameterList.Add(CuesExtractionMode.WithCues);
                            break;
                    }
                    jobs.Add(new gMKVJob(extractionMode, txtMKVToolnixPath.Text, parameterList));
                }

                if (sender == btnAddJobs)
                {
                    if (_JobManagerForm == null)
                    {
                        _JobManagerForm = new frmJobManager(this);
                    }
                    _JobManagerForm.Show();
                    foreach (var job in jobs)
                    {
                        _JobManagerForm.AddJob(new gMKVJobInfo(job));
                    }                    
                }
                else
                {
                    _CurrentJob = 0;
                    _TotalJobs = jobs.Count;

                    prgBrStatus.Minimum = 0;
                    prgBrStatus.Maximum = 100;
                    prgBrTotalStatus.Maximum = _TotalJobs * 100;
                    prgBrTotalStatus.Visible = true;

                    foreach (var job in jobs)
                    {
                        // increate the current job index
                        _CurrentJob++;
                        // start the thread
                        myThread = new Thread(new ParameterizedThreadStart(job.ExtractMethod(_gMkvExtract)));
                        myThread.Start(job.ParametersList);

                        btnAbort.Enabled = true;
                        btnAbortAll.Enabled = true;
                        gTaskbarProgress.SetState(this, gTaskbarProgress.TaskbarStates.Normal);
                        gTaskbarProgress.SetOverlayIcon(this, SystemIcons.Shield, "Extracting...");
                        Application.DoEvents();
                        while (myThread.ThreadState != System.Threading.ThreadState.Stopped)
                        {
                            Application.DoEvents();
                        }
                        // check for exceptions
                        if (_gMkvExtract.ThreadedException != null)
                        {
                            throw _gMkvExtract.ThreadedException;
                        }
                        UpdateProgress(100);
                    }
                    if (chkShowPopup.Checked)
                    {
                        ShowSuccessMessage("The extraction was completed successfully!");
                    }
                    else
                    {
                        SystemSounds.Asterisk.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());

                gTaskbarProgress.SetState(this, gTaskbarProgress.TaskbarStates.Error);
                gTaskbarProgress.SetOverlayIcon(this, SystemIcons.Error, "Error!");
                exceptionOccured = true;
                ShowErrorMessage(ex.Message);
            }
            finally
            {
                if (_gMkvExtract != null)
                {
                    _gMkvExtract.MkvExtractProgressUpdated -= g_MkvExtractProgressUpdated;
                    _gMkvExtract.MkvExtractTrackUpdated -= g_MkvExtractTrackUpdated;
                }
                if (chkShowPopup.Checked || exceptionOccured)
                {
                    ClearStatus();
                }
                else
                {
                    if (sender == btnExtract)
                    {
                        lblStatus.Text = "Extraction completed!";
                    }
                }
                trvInputFiles.SelectedNode = null;
                txtSegmentInfo.Clear();
                grpSelectedFileInfo.Text = "Selected File Information";

                _ExtractRunning = false;
                tlpMain.Enabled = true;
                btnAbort.Enabled = false;
                btnAbortAll.Enabled = false;
                gTaskbarProgress.SetState(this, gTaskbarProgress.TaskbarStates.NoProgress);
                gTaskbarProgress.SetOverlayIcon(this, null, null);
                this.Refresh();
                Application.DoEvents();
            }
        }

        private void ClearControls()
        {
            // check if output directory is the same as the source
            if (!chkUseSourceDirectory.Checked)
            {
                txtOutputDirectory.Clear();
            }

            grpInputFiles.Text = "Input Files (you can drag and drop files or directories)";
            grpSelectedFileInfo.Text = "Selected File Information";

            txtSegmentInfo.Clear();
            ClearStatus();
        }

        private void ClearStatus()
        {
            lblStatus.Text = "";
            lblTotalStatus.Text = "";
            prgBrStatus.Value = 0;
            prgBrTotalStatus.Value = 0;
            prgBrTotalStatus.Visible = false;
        }

        private void txtMKVToolnixPath_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_FromConstructor)
                {
                    // check if the folder actually contains MKVToolnix
                    if (!File.Exists(Path.Combine(txtMKVToolnixPath.Text.Trim(), gMKVHelper.MKV_MERGE_GUI_FILENAME))
                        && !File.Exists(Path.Combine(txtMKVToolnixPath.Text.Trim(), gMKVHelper.MKV_MERGE_NEW_GUI_FILENAME)))
                    {
                        _FromConstructor = true;
                        txtMKVToolnixPath.Text = "";
                        _FromConstructor = false;
                        throw new Exception("The folder does not contain MKVToolnix!");
                    }

                    // Write the value to the ini file
                    _Settings.MkvToolnixPath = txtMKVToolnixPath.Text.Trim();
                    gMKVLogger.Log("Changing MkvToolnixPath");
                    _Settings.Save();
                }
                _gMkvExtract = new gMKVExtract(txtMKVToolnixPath.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void cmbChapterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_FromConstructor)
                {
                    if (cmbChapterType.SelectedIndex > -1)
                    {
                        // Write the value to the ini file
                        _Settings.ChapterType = (MkvChapterTypes)Enum.Parse(typeof(MkvChapterTypes), (String)cmbChapterType.SelectedItem);
                        gMKVLogger.Log("Changing ChapterType...");
                        _Settings.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowErrorMessage(ex.Message);
            }
        }

        private void txtOutputDirectory_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_FromConstructor)
                {
                    _Settings.OutputDirectory = txtOutputDirectory.Text;
                    gMKVLogger.Log("Changing OutputDirectory...");
                    _Settings.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void chkUseSourceDirectory_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //if (sender == chkUseSourceDirectory)
                //{
                //    if (String.IsNullOrWhiteSpace(txtOutputDirectory.Text))
                //    {
                //        chkUseSourceDirectory.Checked = true;
                //    }
                //}
                txtOutputDirectory.ReadOnly = chkUseSourceDirectory.Checked;
                btnBrowseOutputDirectory.Enabled = !chkUseSourceDirectory.Checked;
                if (!_FromConstructor)
                {
                    _Settings.LockedOutputDirectory = chkUseSourceDirectory.Checked;
                    gMKVLogger.Log("Changing LockedOutputDirectory");
                    _Settings.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void btnBrowseOutputDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                // check if output directory is the same as the source
                if (chkUseSourceDirectory.Checked)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.RestoreDirectory = true;
                    sfd.CheckFileExists = false;
                    sfd.CheckPathExists = false;
                    sfd.OverwritePrompt = false;
                    sfd.FileName = "Select directory";
                    sfd.Title = "Select output directory...";
                    if ((sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
                    {
                        txtOutputDirectory.Text = Path.GetDirectoryName(sfd.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void btnBrowseMKVToolnixPath_Click(object sender, EventArgs e)
        {
            try
            {
                // check if MKVToolnix Path is already set
                if (!String.IsNullOrWhiteSpace(txtMKVToolnixPath.Text))
                {
                    if (ShowQuestion("Do you really want to change MKVToolnix path?", "Are you sure?", false) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.RestoreDirectory = true;
                ofd.CheckFileExists = false;
                ofd.CheckPathExists = false;
                ofd.FileName = "Select directory";
                ofd.Title = "Select MKVToolnix directory...";
                if (!String.IsNullOrWhiteSpace(txtMKVToolnixPath.Text))
                {
                    if (Directory.Exists(txtMKVToolnixPath.Text.Trim()))
                    {
                        ofd.InitialDirectory = txtMKVToolnixPath.Text.Trim();
                    }
                }
                if ((ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
                {
                    txtMKVToolnixPath.Text = Path.GetDirectoryName(ofd.FileName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void btnShowLog_Click(object sender, EventArgs e)
        {
            try
            {
                if (_LogForm == null)
                {
                    _LogForm = new frmLog();
                }
                _LogForm.Show();
                _LogForm.Focus();
                _LogForm.Select();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void btnShowJobs_Click(object sender, EventArgs e)
        {
            try
            {
                if (_JobManagerForm == null)
                {
                    _JobManagerForm = new frmJobManager(this);
                }
                _JobManagerForm.Show();
                _JobManagerForm.Focus();
                _JobManagerForm.Select();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            try
            {
                _gMkvExtract.Abort = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowErrorMessage(ex.Message);
            }
        }

        private void btnAbortAll_Click(object sender, EventArgs e)
        {
            try
            {
                _gMkvExtract.AbortAll = true;
                _gMkvExtract.Abort = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        public void SetTableLayoutMainStatus(bool argStatus)
        {
            tlpMain.Enabled = argStatus;
            Application.DoEvents();
        }

        #region "Form Events"

        private void frmMain_Move(object sender, EventArgs e)
        {
            try
            {
                if (!_FromConstructor && !(
                    this.WindowState == FormWindowState.Minimized
                    || this.WindowState == FormWindowState.Maximized))
                {
                    _Settings.WindowPosX = this.Location.X;
                    _Settings.WindowPosY = this.Location.Y;
                    _Settings.WindowState = this.WindowState;
                    gMKVLogger.Log("Changing WindowPosX, WindowPosY, WindowState");
                    _Settings.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            try
            {
                if (!_FromConstructor)
                {
                    _Settings.WindowSizeWidth = this.Size.Width;
                    _Settings.WindowSizeHeight = this.Size.Height;
                    _Settings.WindowState = this.WindowState;
                    gMKVLogger.Log("Changing WindowSizeWidth, WindowSizeHeight, WindowState");
                    _Settings.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void frmMain_ClientSizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_FromConstructor)
                {
                    _Settings.WindowState = this.WindowState;
                    gMKVLogger.Log("Changing WindowState");
                    _Settings.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_ExtractRunning)
                {
                    e.Cancel = true;
                    ShowErrorMessage("There is an extraction process running! Please abort before closing!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                e.Cancel = true;
                ShowErrorMessage(ex.Message);
            }
        }

        #endregion

        private void chkShowPopup_CheckedChanged(object sender, EventArgs e)
        {
            if (!_FromConstructor)
            {
                _Settings.ShowPopup = chkShowPopup.Checked;
                gMKVLogger.Log("Changing ShowPopup");
                _Settings.Save();
            }
        }

        #region "Context Menu"

        private void SetContextMenuText()
        {
            List<TreeNode> allNodes = trvInputFiles.AllNodes;
            List<TreeNode> checkedNodes = trvInputFiles.CheckedNodes;

            Int32 allTracksCount = allNodes.Count(n => n != null && n.Tag != null && (n.Tag is gMKVTrack || n.Tag is gMKVChapter || n.Tag is gMKVAttachment));
            Int32 videoTracksCount = allNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video);
            Int32 audioTracksCount = allNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio);
            Int32 subtitleTracksCount = allNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles);
            Int32 chapterTracksCount = allNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVChapter);
            Int32 attachmentTracksCount = allNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVAttachment);

            Int32 checkedAllTracksCount = checkedNodes.Count(n => n != null && n.Tag != null && (n.Tag is gMKVTrack || n.Tag is gMKVChapter || n.Tag is gMKVAttachment));
            Int32 checkedVideoTracksCount = checkedNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video);
            Int32 checkedAudioTracksCount = checkedNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio);
            Int32 checkedSubtitleTracksCount = checkedNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles);
            Int32 checkedChapterTracksCount = checkedNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVChapter);
            Int32 checkedAttachmentTracksCount = checkedNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVAttachment);

            Int32 allInputFilesCount = allNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVSegmentInfo);

            checkTracksToolStripMenuItem.Enabled = (allTracksCount - checkedAllTracksCount > 0);
            checkVideoTracksToolStripMenuItem.Enabled = (videoTracksCount - checkedVideoTracksCount > 0);
            checkAudioTracksToolStripMenuItem.Enabled = (audioTracksCount - checkedAudioTracksCount > 0);
            checkSubtitleTracksToolStripMenuItem.Enabled = (subtitleTracksCount - checkedSubtitleTracksCount > 0);
            checkChapterTracksToolStripMenuItem.Enabled = (chapterTracksCount - checkedChapterTracksCount > 0);
            checkAttachmentTracksToolStripMenuItem.Enabled = (attachmentTracksCount - checkedAttachmentTracksCount > 0);

            uncheckTracksToolStripMenuItem.Enabled = (checkedAllTracksCount > 0);
            uncheckVideoTracksToolStripMenuItem.Enabled = (checkedVideoTracksCount > 0);
            uncheckAudioTracksToolStripMenuItem.Enabled = (checkedAudioTracksCount > 0);
            uncheckSubtitleTracksToolStripMenuItem.Enabled = (checkedSubtitleTracksCount > 0);
            uncheckChapterTracksToolStripMenuItem.Enabled = (checkedChapterTracksCount > 0);
            uncheckAttachmentTracksToolStripMenuItem.Enabled = (checkedAttachmentTracksCount > 0);

            removeAllInputFilesToolStripMenuItem.Enabled = (allInputFilesCount > 0);
            removeSelectedInputFileToolStripMenuItem.Enabled = (trvInputFiles.SelectedNode != null && trvInputFiles.SelectedNode.Tag != null);
            openSelectedFileFolderToolStripMenuItem.Enabled = (trvInputFiles.SelectedNode != null && trvInputFiles.SelectedNode.Tag != null);
            openSelectedFileToolStripMenuItem.Enabled = (trvInputFiles.SelectedNode != null && trvInputFiles.SelectedNode.Tag != null);

            checkTracksToolStripMenuItem.Text = String.Format("Check All Tracks ({0}/{1})", checkedAllTracksCount, allTracksCount);

            checkVideoTracksToolStripMenuItem.Text = String.Format("Check Video Tracks... ({0}/{1})", checkedVideoTracksCount, videoTracksCount);
            checkAudioTracksToolStripMenuItem.Text = String.Format("Check Audio Tracks... ({0}/{1})", checkedAudioTracksCount, audioTracksCount);
            checkSubtitleTracksToolStripMenuItem.Text = String.Format("Check Subtitle Tracks... ({0}/{1})", checkedSubtitleTracksCount, subtitleTracksCount);
            checkChapterTracksToolStripMenuItem.Text = String.Format("Check Chapter Tracks... ({0}/{1})", checkedChapterTracksCount, chapterTracksCount);
            checkAttachmentTracksToolStripMenuItem.Text = String.Format("Check Attachment Tracks... ({0}/{1})", checkedAttachmentTracksCount, attachmentTracksCount);

            allVideoTracksToolStripMenuItem.Text = String.Format("All Video Tracks ({0}/{1})", checkedVideoTracksCount, videoTracksCount);
            allAudioTracksToolStripMenuItem.Text = String.Format("All Audio Tracks ({0}/{1})", checkedAudioTracksCount, audioTracksCount);
            allSubtitleTracksToolStripMenuItem.Text = String.Format("All Subtitle Tracks ({0}/{1})", checkedSubtitleTracksCount, subtitleTracksCount);
            allChapterTracksToolStripMenuItem.Text = String.Format("All Chapter Tracks ({0}/{1})", checkedChapterTracksCount, chapterTracksCount);
            allAttachmentTracksToolStripMenuItem.Text = String.Format("All Attachment Tracks ({0}/{1})", checkedAttachmentTracksCount, attachmentTracksCount);

            uncheckTracksToolStripMenuItem.Text = String.Format("Uncheck All Tracks ({0}/{1})", (allTracksCount - checkedAllTracksCount), allTracksCount);

            uncheckVideoTracksToolStripMenuItem.Text = String.Format("Uncheck Video Tracks... ({0}/{1})", videoTracksCount - checkedVideoTracksCount, videoTracksCount);
            uncheckAudioTracksToolStripMenuItem.Text = String.Format("Uncheck Audio Tracks... ({0}/{1})", audioTracksCount - checkedAudioTracksCount, audioTracksCount);
            uncheckSubtitleTracksToolStripMenuItem.Text = String.Format("Uncheck Subtitle Tracks... ({0}/{1})", subtitleTracksCount - checkedSubtitleTracksCount, subtitleTracksCount);
            uncheckChapterTracksToolStripMenuItem.Text = String.Format("Uncheck Chapter Tracks... ({0}/{1})", chapterTracksCount - checkedChapterTracksCount, chapterTracksCount);
            uncheckAttachmentTracksToolStripMenuItem.Text = String.Format("Uncheck Attachment Tracks... ({0}/{1})", attachmentTracksCount - checkedAttachmentTracksCount, attachmentTracksCount);

            allVideoTracksToolStripMenuItem1.Text = String.Format("All Video Tracks ({0}/{1})", videoTracksCount - checkedVideoTracksCount, videoTracksCount);
            allAudioTracksToolStripMenuItem1.Text = String.Format("All Audio Tracks ({0}/{1})", audioTracksCount - checkedAudioTracksCount, audioTracksCount);
            allSubtitleTracksToolStripMenuItem1.Text = String.Format("All Subtitle Tracks ({0}/{1})", subtitleTracksCount - checkedSubtitleTracksCount, subtitleTracksCount);
            allChapterTracksToolStripMenuItem1.Text = String.Format("All Chapter Tracks ({0}/{1})", chapterTracksCount - checkedChapterTracksCount, chapterTracksCount);
            allAttachmentTracksToolStripMenuItem1.Text = String.Format("All Attachment Tracks ({0}/{1})", attachmentTracksCount - checkedAttachmentTracksCount, attachmentTracksCount);

            removeAllInputFilesToolStripMenuItem.Text = String.Format("Remove All Input Files ({0})", allInputFilesCount);

            removeSelectedInputFileToolStripMenuItem.Text = "Remove Selected Input File";

            checkVideoTracksToolStripMenuItem.DropDownItems.Clear();
            checkVideoTracksToolStripMenuItem.DropDownItems.Add(allVideoTracksToolStripMenuItem);
            uncheckVideoTracksToolStripMenuItem.DropDownItems.Clear();
            uncheckVideoTracksToolStripMenuItem.DropDownItems.Add(allVideoTracksToolStripMenuItem1);

            checkAudioTracksToolStripMenuItem.DropDownItems.Clear();
            checkAudioTracksToolStripMenuItem.DropDownItems.Add(allAudioTracksToolStripMenuItem);
            uncheckAudioTracksToolStripMenuItem.DropDownItems.Clear();
            uncheckAudioTracksToolStripMenuItem.DropDownItems.Add(allAudioTracksToolStripMenuItem1);

            checkSubtitleTracksToolStripMenuItem.DropDownItems.Clear();
            checkSubtitleTracksToolStripMenuItem.DropDownItems.Add(allSubtitleTracksToolStripMenuItem);
            uncheckSubtitleTracksToolStripMenuItem.DropDownItems.Clear();
            uncheckSubtitleTracksToolStripMenuItem.DropDownItems.Add(allSubtitleTracksToolStripMenuItem1);

            List<ToolStripItem> checkItems = null;
            List<ToolStripItem> uncheckItems = null;

            // Get all video track languages
            List<string> videoLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video).
                Select(n => (n.Tag as gMKVTrack).Language).Distinct().ToList();
            ToolStripMenuItem tsCheckVideoTracksByLanguage = new ToolStripMenuItem(String.Format("Video Tracks by Language ({0})...", videoLanguages.Count));
            checkVideoTracksToolStripMenuItem.DropDownItems.Add(tsCheckVideoTracksByLanguage);
            ToolStripMenuItem tsUncheckVideoTracksByLanguage = new ToolStripMenuItem(String.Format("Video Tracks by Language ({0})...", videoLanguages.Count));
            uncheckVideoTracksToolStripMenuItem.DropDownItems.Add(tsUncheckVideoTracksByLanguage);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var lang in videoLanguages)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).Language == lang).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).Language == lang).Count();                
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Language: [{0}] ({1}/{2})", lang, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.video, true, argLanguageFilter: lang); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Language: [{0}] ({1}/{2})", lang, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.video, false, argLanguageFilter: lang); }
                    )
                );
            }
            tsCheckVideoTracksByLanguage.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckVideoTracksByLanguage.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all video track Codec_id 
            List<string> videoCodecs = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video).
                Select(n => (n.Tag as gMKVTrack).CodecID).Distinct().ToList();
            ToolStripMenuItem tsCheckVideoTracksByCodec = new ToolStripMenuItem(String.Format("Video Tracks by Codec ({0})...", videoCodecs.Count));
            checkVideoTracksToolStripMenuItem.DropDownItems.Add(tsCheckVideoTracksByCodec);
            ToolStripMenuItem tsUncheckVideoTracksByCodec = new ToolStripMenuItem(String.Format("Video Tracks by Codec ({0})...", videoCodecs.Count));
            uncheckVideoTracksToolStripMenuItem.DropDownItems.Add(tsUncheckVideoTracksByCodec);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var codec in videoCodecs)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).CodecID == codec).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).CodecID == codec).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Codec: [{0}] ({1}/{2})", codec, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.video, true, argCodecIdFilter: codec); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Codec: [{0}] ({1}/{2})", codec, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.video, false, argCodecIdFilter: codec); }
                    )
                );
            }
            tsCheckVideoTracksByCodec.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckVideoTracksByCodec.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all video track extra info
            List<string> videoExtra = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video).
                Select(n => (n.Tag as gMKVTrack).ExtraInfo).Distinct().ToList();
            ToolStripMenuItem tsCheckVideoTracksByResolution = new ToolStripMenuItem(String.Format("Video Tracks by Resolution ({0})...", videoExtra.Count));
            checkVideoTracksToolStripMenuItem.DropDownItems.Add(tsCheckVideoTracksByResolution);
            ToolStripMenuItem tsUncheckVideoTracksByResolution = new ToolStripMenuItem(String.Format("Video Tracks by Resolution ({0})...", videoExtra.Count));
            uncheckVideoTracksToolStripMenuItem.DropDownItems.Add(tsUncheckVideoTracksByResolution);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var extra in videoExtra)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).ExtraInfo == extra).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).ExtraInfo == extra).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Resolution: [{0}] ({1}/{2})", extra, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.video, true, argExtraInfoFilter: extra); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Resolution: [{0}] ({1}/{2})", extra, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.video, false, argExtraInfoFilter: extra); }
                    )
                );
            }
            tsCheckVideoTracksByResolution.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckVideoTracksByResolution.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all video track name
            List<string> videoNames = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video).
                Select(n => (n.Tag as gMKVTrack).TrackName).Distinct().ToList();
            // Only show menu items if the names are less than 50
            if (videoNames.Any() && videoNames.Count < 50)
            {
                ToolStripMenuItem tsCheckVideoTracksByName = new ToolStripMenuItem(String.Format("Video Tracks by Track Name ({0})...", videoNames.Count));
                checkVideoTracksToolStripMenuItem.DropDownItems.Add(tsCheckVideoTracksByName);
                ToolStripMenuItem tsUncheckVideoTracksByName = new ToolStripMenuItem(String.Format("Video Tracks by Track Name ({0})...", videoNames.Count));
                uncheckVideoTracksToolStripMenuItem.DropDownItems.Add(tsUncheckVideoTracksByName);
                checkItems = new List<ToolStripItem>();
                uncheckItems = new List<ToolStripItem>();
                foreach (var name in videoNames)
                {
                    Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).TrackName == name).Count();
                    Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video && (n.Tag as gMKVTrack).TrackName == name).Count();
                    checkItems.Add(
                            new ToolStripMenuItem(String.Format("Track Name: [{0}] ({1}/{2})", name, checkedLanguages, totalLanguages), null,
                            delegate { SetCheckedTracks(TrackSelectionMode.video, true, argNameFilter: name); }
                        )
                    );
                    uncheckItems.Add(
                            new ToolStripMenuItem(String.Format("Track Name: [{0}] ({1}/{2})", name, totalLanguages - checkedLanguages, totalLanguages), null,
                            delegate { SetCheckedTracks(TrackSelectionMode.video, false, argNameFilter: name); }
                        )
                    );
                }
                tsCheckVideoTracksByName.DropDownItems.AddRange(checkItems.ToArray());
                tsUncheckVideoTracksByName.DropDownItems.AddRange(uncheckItems.ToArray());
            }

            // Get all audio track languages
            List<string> audioLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio).
                Select(n => (n.Tag as gMKVTrack).Language).Distinct().ToList();
            ToolStripMenuItem tsCheckAudioTracksByLanguage = new ToolStripMenuItem(String.Format("Audio Tracks by Language ({0})...", audioLanguages.Count));
            checkAudioTracksToolStripMenuItem.DropDownItems.Add(tsCheckAudioTracksByLanguage);
            ToolStripMenuItem tsUncheckAudioTracksByLanguage = new ToolStripMenuItem(String.Format("Audio Tracks by Language ({0})...", audioLanguages.Count));
            uncheckAudioTracksToolStripMenuItem.DropDownItems.Add(tsUncheckAudioTracksByLanguage);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var lang in audioLanguages)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).Language == lang).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).Language == lang).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Language: [{0}] ({1}/{2})", lang, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.audio, true, argLanguageFilter: lang); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Language: [{0}] ({1}/{2})", lang, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.audio, false, argLanguageFilter: lang); }
                    )
                );
            }
            tsCheckAudioTracksByLanguage.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckAudioTracksByLanguage.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all audio track Codec_id 
            List<string> audioCodecs = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio).
                Select(n => (n.Tag as gMKVTrack).CodecID).Distinct().ToList();
            ToolStripMenuItem tsCheckAudioTracksByCodec = new ToolStripMenuItem(String.Format("Audio Tracks by Codec ({0})...", audioCodecs.Count));
            checkAudioTracksToolStripMenuItem.DropDownItems.Add(tsCheckAudioTracksByCodec);
            ToolStripMenuItem tsUncheckAudioTracksByCodec = new ToolStripMenuItem(String.Format("Audio Tracks by Codec ({0})...", audioCodecs.Count));
            uncheckAudioTracksToolStripMenuItem.DropDownItems.Add(tsUncheckAudioTracksByCodec);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var codec in audioCodecs)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).CodecID == codec).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).CodecID == codec).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Codec: [{0}] ({1}/{2})", codec, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.audio, true, argCodecIdFilter: codec); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Codec: [{0}] ({1}/{2})", codec, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.audio, false, argCodecIdFilter: codec); }
                    )
                );
            }
            tsCheckAudioTracksByCodec.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckAudioTracksByCodec.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all audio track extra info
            List<string> audioExtraInfo = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio).
                Select(n => (n.Tag as gMKVTrack).ExtraInfo).Distinct().ToList();
            ToolStripMenuItem tsCheckAudioTracksByChannels = new ToolStripMenuItem(String.Format("Audio Tracks by Channels ({0})...", audioExtraInfo.Count));
            checkAudioTracksToolStripMenuItem.DropDownItems.Add(tsCheckAudioTracksByChannels);
            ToolStripMenuItem tsUncheckAudioTracksByChannels = new ToolStripMenuItem(String.Format("Audio Tracks by Channels ({0})...", audioExtraInfo.Count));
            uncheckAudioTracksToolStripMenuItem.DropDownItems.Add(tsUncheckAudioTracksByChannels);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var extra in audioExtraInfo)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).ExtraInfo == extra).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).ExtraInfo == extra).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Channels: [{0}] ({1}/{2})", extra, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.audio, true, argExtraInfoFilter: extra); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Channels: [{0}] ({1}/{2})", extra, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.audio, false, argExtraInfoFilter: extra); }
                    )
                );
            }
            tsCheckAudioTracksByChannels.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckAudioTracksByChannels.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all audio track name
            List<string> audioNames = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio).
                Select(n => (n.Tag as gMKVTrack).TrackName).Distinct().ToList();
            // Only show menu items if the names are less than 50
            if (audioNames.Any() && audioNames.Count < 50)
            {
                ToolStripMenuItem tsCheckAudioTracksByName = new ToolStripMenuItem(String.Format("Audio Tracks by Track Name ({0})...", audioNames.Count));
                checkAudioTracksToolStripMenuItem.DropDownItems.Add(tsCheckAudioTracksByName);
                ToolStripMenuItem tsUncheckAudioTracksByName = new ToolStripMenuItem(String.Format("Audio Tracks by Track Name ({0})...", audioNames.Count));
                uncheckAudioTracksToolStripMenuItem.DropDownItems.Add(tsUncheckAudioTracksByName);
                checkItems = new List<ToolStripItem>();
                uncheckItems = new List<ToolStripItem>();
                foreach (var name in audioNames)
                {
                    Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).TrackName == name).Count();
                    Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio && (n.Tag as gMKVTrack).TrackName == name).Count();
                    checkItems.Add(
                            new ToolStripMenuItem(String.Format("Track Name: [{0}] ({1}/{2})", name, checkedLanguages, totalLanguages), null,
                            delegate { SetCheckedTracks(TrackSelectionMode.audio, true, argNameFilter: name); }
                        )
                    );
                    uncheckItems.Add(
                            new ToolStripMenuItem(String.Format("Track Name: [{0}] ({1}/{2})", name, totalLanguages - checkedLanguages, totalLanguages), null,
                            delegate { SetCheckedTracks(TrackSelectionMode.audio, false, argNameFilter: name); }
                        )
                    );
                }
                tsCheckAudioTracksByName.DropDownItems.AddRange(checkItems.ToArray());
                tsUncheckAudioTracksByName.DropDownItems.AddRange(uncheckItems.ToArray());
            }

            // Get all subtitle track languages
            List<string> subLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles).
                Select(n => (n.Tag as gMKVTrack).Language).Distinct().ToList();
            ToolStripMenuItem tsCheckSubtitleTracksByLanguage = new ToolStripMenuItem(String.Format("Subtitle Tracks by Language ({0})...", subLanguages.Count));
            checkSubtitleTracksToolStripMenuItem.DropDownItems.Add(tsCheckSubtitleTracksByLanguage);
            ToolStripMenuItem tsUncheckSubtitleTracksByLanguage = new ToolStripMenuItem(String.Format("Subtitle Tracks by Language ({0})...", subLanguages.Count));
            uncheckSubtitleTracksToolStripMenuItem.DropDownItems.Add(tsUncheckSubtitleTracksByLanguage);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var lang in subLanguages)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles && (n.Tag as gMKVTrack).Language == lang).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles && (n.Tag as gMKVTrack).Language == lang).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Language: [{0}] ({1}/{2})", lang, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.subtitle, true, argLanguageFilter: lang); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Language: [{0}] ({1}/{2})", lang, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.subtitle, false, argLanguageFilter: lang); }
                    )
                );
            }
            tsCheckSubtitleTracksByLanguage.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckSubtitleTracksByLanguage.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all subtitle track codec_id
            List<string> subCodecs = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles).
                Select(n => (n.Tag as gMKVTrack).CodecID).Distinct().ToList();
            ToolStripMenuItem tsCheckSubtitleTracksByCodec = new ToolStripMenuItem(String.Format("Subtitle Tracks by Codec ({0})...", subCodecs.Count));
            checkSubtitleTracksToolStripMenuItem.DropDownItems.Add(tsCheckSubtitleTracksByCodec);
            ToolStripMenuItem tsUncheckSubtitleTracksByCodec = new ToolStripMenuItem(String.Format("Subtitle Tracks by Codec ({0})...", subCodecs.Count));
            uncheckSubtitleTracksToolStripMenuItem.DropDownItems.Add(tsUncheckSubtitleTracksByCodec);
            checkItems = new List<ToolStripItem>();
            uncheckItems = new List<ToolStripItem>();
            foreach (var codec in subCodecs)
            {
                Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles && (n.Tag as gMKVTrack).CodecID == codec).Count();
                Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles && (n.Tag as gMKVTrack).CodecID == codec).Count();
                checkItems.Add(
                    new ToolStripMenuItem(String.Format("Codec: [{0}] ({1}/{2})", codec, checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.subtitle, true, argCodecIdFilter: codec); }
                    )
                );
                uncheckItems.Add(
                    new ToolStripMenuItem(String.Format("Codec: [{0}] ({1}/{2})", codec, totalLanguages - checkedLanguages, totalLanguages), null,
                        delegate { SetCheckedTracks(TrackSelectionMode.subtitle, false, argCodecIdFilter: codec); }
                    )
                );
            }
            tsCheckSubtitleTracksByCodec.DropDownItems.AddRange(checkItems.ToArray());
            tsUncheckSubtitleTracksByCodec.DropDownItems.AddRange(uncheckItems.ToArray());

            // Get all subtitle track names
            List<string> subNames = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles).
                Select(n => (n.Tag as gMKVTrack).TrackName).Distinct().ToList();
            // Only show menu items if the names are less than 50
            if (subNames.Any() && subNames.Count < 50)
            {
                ToolStripMenuItem tsCheckSubtitleTracksByName = new ToolStripMenuItem(String.Format("Subtitle Tracks by Track Name ({0})...", subNames.Count));
                checkSubtitleTracksToolStripMenuItem.DropDownItems.Add(tsCheckSubtitleTracksByName);
                ToolStripMenuItem tsUncheckSubtitleTracksByName = new ToolStripMenuItem(String.Format("Subtitle Tracks by Track Name ({0})...", subNames.Count));
                uncheckSubtitleTracksToolStripMenuItem.DropDownItems.Add(tsUncheckSubtitleTracksByName);
                checkItems = new List<ToolStripItem>();
                uncheckItems = new List<ToolStripItem>();
                foreach (var name in subNames)
                {
                    Int32 totalLanguages = allNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles && (n.Tag as gMKVTrack).TrackName == name).Count();
                    Int32 checkedLanguages = checkedNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles && (n.Tag as gMKVTrack).TrackName == name).Count();
                    checkItems.Add(
                            new ToolStripMenuItem(String.Format("Track Name: [{0}] ({1}/{2})", name, checkedLanguages, totalLanguages), null,
                            delegate { SetCheckedTracks(TrackSelectionMode.subtitle, true, argNameFilter: name); }
                        )
                    );
                    uncheckItems.Add(
                            new ToolStripMenuItem(String.Format("Track Name: [{0}] ({1}/{2})", name, totalLanguages - checkedLanguages, totalLanguages), null,
                            delegate { SetCheckedTracks(TrackSelectionMode.subtitle, false, argNameFilter: name); }
                        )
                    );
                }
                tsCheckSubtitleTracksByName.DropDownItems.AddRange(checkItems.ToArray());
                tsUncheckSubtitleTracksByName.DropDownItems.AddRange(uncheckItems.ToArray());
            }
        }

        private void SetCheckedTracks(TrackSelectionMode argSelectionMode, bool argCheck, 
            string argLanguageFilter = null, string argExtraInfoFilter = null, string argCodecIdFilter = null, string argNameFilter = null)
        {
            List<TreeNode> nodes = null;
            switch (argSelectionMode)
            {
                case TrackSelectionMode.video:
                    nodes = trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.video).ToList();
                    if (!String.IsNullOrWhiteSpace(argLanguageFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).Language == argLanguageFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argExtraInfoFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).ExtraInfo == argExtraInfoFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argCodecIdFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).CodecID == argCodecIdFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argNameFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).TrackName == argNameFilter).ToList();
                    }
                    break;
                case TrackSelectionMode.audio:
                    nodes = trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.audio).ToList();
                    if (!String.IsNullOrWhiteSpace(argLanguageFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).Language == argLanguageFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argExtraInfoFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).ExtraInfo == argExtraInfoFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argCodecIdFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).CodecID == argCodecIdFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argNameFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).TrackName == argNameFilter).ToList();
                    }
                    break;
                case TrackSelectionMode.subtitle:
                    nodes = trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVTrack && (n.Tag as gMKVTrack).TrackType == MkvTrackType.subtitles).ToList();
                    if (!String.IsNullOrWhiteSpace(argLanguageFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).Language == argLanguageFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argExtraInfoFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).ExtraInfo == argExtraInfoFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argCodecIdFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).CodecID == argCodecIdFilter).ToList();
                    }
                    if (!String.IsNullOrWhiteSpace(argNameFilter))
                    {
                        nodes = nodes.Where(n => (n.Tag as gMKVTrack).TrackName == argNameFilter).ToList();
                    }
                    break;
                case TrackSelectionMode.chapter:
                    nodes = trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVChapter).ToList();
                    break;
                case TrackSelectionMode.attachment:
                    nodes = trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && n.Tag is gMKVAttachment).ToList();
                    break;
                case TrackSelectionMode.all:
                    nodes = trvInputFiles.AllNodes.Where(n => n != null && n.Tag != null && !(n.Tag is gMKVSegmentInfo)).ToList();
                    break;
                default:

                    break;
            }
            nodes.ForEach(n => n.Checked = argCheck);
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            SetContextMenuText();
        }

        private void checkTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.all, true);
        }

        private void allVideoTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.video, true);
        }

        private void allAudioTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.audio, true);
        }

        private void allSubtitleTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.subtitle, true);
        }

        private void allChapterTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.chapter, true);
        }

        private void allAttachmentTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.attachment, true);
        }

        private void uncheckTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.all, false);
        }

        private void allVideoTracksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.video, false);
        }

        private void allAudioTracksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.audio, false);
        }

        private void allSubtitleTracksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.subtitle, false);
        }

        private void allChapterTracksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.chapter, false);
        }

        private void allAttachmentTracksToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SetCheckedTracks(TrackSelectionMode.attachment, false);
        }

        private void removeAllInputFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trvInputFiles.Nodes.Clear();
            ClearControls();
        }

        private void removeSelectedInputFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(trvInputFiles.SelectedNode == null || trvInputFiles.SelectedNode.Tag == null)
            {
                return;
            }
            TreeNode node = trvInputFiles.SelectedNode;
            if(!(node.Tag is gMKVSegmentInfo))
            {
                node = node.Parent;
            }
            trvInputFiles.Nodes.Remove(node);
            if(trvInputFiles.Nodes.Count > 0)
            {
                grpInputFiles.Text = String.Format("Input Files (you can drag and drop files or directories) ({0} files)",
                trvInputFiles.AllNodes.Count(n => n != null && n.Tag != null && n.Tag is gMKVSegmentInfo));
            }
            else
            {
                ClearControls();
            }
        }

        private void openSelectedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (trvInputFiles.SelectedNode == null || trvInputFiles.SelectedNode.Tag == null)
                {
                    return;
                }
                TreeNode node = trvInputFiles.SelectedNode;
                if (!(node.Tag is gMKVSegmentInfo))
                {
                    node = node.Parent;
                }
                gMKVSegmentInfo segInfo = node.Tag as gMKVSegmentInfo;
                if (File.Exists(segInfo.Path))
                {
                    Process.Start(segInfo.Path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void openSelectedFileFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (trvInputFiles.SelectedNode == null || trvInputFiles.SelectedNode.Tag == null)
                {
                    return;
                }
                TreeNode node = trvInputFiles.SelectedNode;
                if (!(node.Tag is gMKVSegmentInfo))
                {
                    node = node.Parent;
                }
                gMKVSegmentInfo segInfo = node.Tag as gMKVSegmentInfo;

                if (Directory.Exists(segInfo.Directory))
                {
                    if (File.Exists(segInfo.Path))
                    {
                        Process.Start("explorer.exe", String.Format("/select, \"{0}\"", segInfo.Path));
                    }
                    else
                    {
                        Process.Start("explorer.exe", segInfo.Directory);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void addInputFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Select an input matroska file...";
                ofd.Filter = "Matroska files (*.mkv;*.mka;*.mks;*.mk3d;*.webm)|*.mkv;*.mka;*.mks;*.mk3d;*.webm|Matroska video files (*.mkv)|*.mkv|Matroska audio files (*.mka)|*.mka|Matroska subtitle files (*.mks)|*.mks|Matroska 3D files (*.mk3d)|*.mk3d|Webm files (*.webm)|*.webm";
                ofd.Multiselect = true;
                ofd.AutoUpgradeEnabled = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    AddFileNodes(txtMKVToolnixPath.Text, new List<string>(ofd.FileNames), true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                gMKVLogger.Log(ex.ToString());
                ShowErrorMessage(ex.Message);
            }
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trvInputFiles.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trvInputFiles.CollapseAll();
        }

        #endregion

    }
}
