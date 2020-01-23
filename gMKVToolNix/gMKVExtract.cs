using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using gMKVToolNix.CueSheet;

namespace gMKVToolNix
{
    public enum MkvExtractModes
    {
        tracks,
        tags,
        attachments,
        chapters,
        cuesheet,
        timecodes_v2,
        cues,
        timestamps_v2
    }

    public enum MkvExtractGlobalOptions
    {
        parse_fully,
        verbose,
        quiet,
        ui_language,
        command_line_charset,
        output_charset,
        redirect_output,
        help,
        version,
        check_for_updates,
        gui_mode
    }

    public enum TimecodesExtractionMode
    {
        NoTimecodes,
        WithTimecodes,
        OnlyTimecodes
    }

    public enum CuesExtractionMode
    {
        NoCues,
        WithCues,
        OnlyCues
    }

    public sealed class gMKVExtractFilenamePatterns
    {
        // Common placeholders
        public static readonly string FilenameNoExt = "{FilenameNoExt}";
        public static readonly string Filename = "{Filename}";

        // Common Track placeholders
        public static readonly string TrackNumber = "{TrackNumber}";
        public static readonly string TrackNumber_0 = "{TrackNumber:0}";
        public static readonly string TrackNumber_00 = "{TrackNumber:00}";
        public static readonly string TrackNumber_000 = "{TrackNumber:000}";
        public static readonly string TrackID = "{TrackID}";
        public static readonly string TrackID_0 = "{TrackID:0}";
        public static readonly string TrackID_00 = "{TrackID:00}";
        public static readonly string TrackID_000 = "{TrackID:000}";
        public static readonly string TrackName = "{TrackName}";
        public static readonly string TrackLanguage = "{Language}";
        public static readonly string TrackCodecID = "{CodecID}";
        public static readonly string TrackCodecPrivate = "{CodecPrivate}";
        public static readonly string TrackDelay = "{Delay}";
        public static readonly string TrackEffectiveDelay = "{EffectiveDelay}";

        // Video Track placeholders
        public static readonly string VideoPixelWidth = "{PixelWidth}";
        public static readonly string VideoPixelHeight = "{PixelHeight}";

        // Audio Track placeholders
        public static readonly string AudioSamplingFrequency = "{SamplingFrequency}";
        public static readonly string AudioChannels = "{Channels}";

        // Attachment placeholders
        public static readonly string AttachmentID = "{AttachmentID}";
        public static readonly string AttachmentID_0 = "{AttachmentID:0}";
        public static readonly string AttachmentID_00 = "{AttachmentID:00}";
        public static readonly string AttachmentID_000 = "{AttachmentID:000}";
        public static readonly string AttachmentFilename = "{AttachmentFilename}";
        public static readonly string AttachmentMimeType = "{MimeType}";
        public static readonly string AttachmentFileSize = "{AttachmentFileSize}";

        public string VideoTrackFilenamePattern { get; set; }
        public string AudioTrackFilenamePattern { get; set; }
        public string SubtitleTrackFilenamePattern { get; set; }
        public string ChapterFilenamePattern { get; set; }
        public string AttachmentFilenamePattern { get; set; }

        public override bool Equals(object oth)
        {
            gMKVExtractFilenamePatterns other = oth as gMKVExtractFilenamePatterns;
            if (oth == null)
            {
                return false;
            }
            return
                VideoTrackFilenamePattern == other.VideoTrackFilenamePattern
                && AudioTrackFilenamePattern == other.AudioTrackFilenamePattern
                && SubtitleTrackFilenamePattern == other.SubtitleTrackFilenamePattern
                && ChapterFilenamePattern == other.ChapterFilenamePattern
                && AttachmentFilenamePattern == other.AttachmentFilenamePattern
                ;
        }

        public override int GetHashCode()
        {
            return
                string.Concat(
                    VideoTrackFilenamePattern.GetHashCode()
                    , AudioTrackFilenamePattern.GetHashCode()
                    , SubtitleTrackFilenamePattern.GetHashCode()
                    , ChapterFilenamePattern.GetHashCode()
                    , AttachmentFilenamePattern.GetHashCode()
                ).GetHashCode();
        }
    }

    public sealed class gMKVExtractSegmentsParameters
    {
        public String MKVFile { get; set; }
        public List<gMKVSegment> MKVSegmentsToExtract { get; set; }
        public String OutputDirectory { get; set; }
        public MkvChapterTypes ChapterType { get; set; }
        public TimecodesExtractionMode TimecodesExtractionMode { get; set; }
        public CuesExtractionMode CueExtractionMode { get; set; }
        public gMKVExtractFilenamePatterns FilenamePatterns { get; set; }

        public override bool Equals(object oth)
        {
            gMKVExtractSegmentsParameters other = oth as gMKVExtractSegmentsParameters;
            if (other == null)
            {
                return false;
            }
            return
                MKVFile == other.MKVFile
                && !MKVSegmentsToExtract.Any(x => !other.MKVSegmentsToExtract.Contains(x))
                && OutputDirectory == other.OutputDirectory
                && ChapterType == other.ChapterType
                && TimecodesExtractionMode == other.TimecodesExtractionMode
                && CueExtractionMode == other.CueExtractionMode
                && FilenamePatterns.Equals(other.FilenamePatterns);
        }

        public override int GetHashCode()
        {
            return
                string.Concat(
                    MKVFile.GetHashCode()
                    , MKVSegmentsToExtract.GetHashCode()
                    , OutputDirectory.GetHashCode()
                    , ChapterType.GetHashCode()
                    , TimecodesExtractionMode.GetHashCode()
                    , CueExtractionMode.GetHashCode()
                    , FilenamePatterns.GetHashCode()
                ).GetHashCode();
        }
    }

    public delegate void MkvExtractProgressUpdatedEventHandler(Int32 progress);
    public delegate void MkvExtractTrackUpdatedEventHandler(String filename, String trackName);

    public class gMKVExtract
    {
        internal class TrackParameter
        {
            public MkvExtractModes ExtractMode = MkvExtractModes.tracks;
            public String Options = "";
            public String TrackOutput = "";
            public Boolean WriteOutputToFile = false;
            public String OutputFilename = "";

            public TrackParameter(MkvExtractModes argExtractMode,
                String argOptions,
                String argTrackOutput,
                Boolean argWriteOutputToFile,
                String argOutputFilename)
            {
                ExtractMode = argExtractMode;
                Options = argOptions;
                TrackOutput = argTrackOutput;
                WriteOutputToFile = argWriteOutputToFile;
                OutputFilename = argOutputFilename;
            }

            public TrackParameter() { }
        }

        internal class OptionValue
        {
            private MkvExtractGlobalOptions _Option;
            private String _Parameter;

            public MkvExtractGlobalOptions Option
            {
                get { return _Option; }
                set { _Option = value; }
            }

            public String Parameter
            {
                get { return _Parameter; }
                set { _Parameter = value; }
            }

            public OptionValue(MkvExtractGlobalOptions opt, String par)
            {
                _Option = opt;
                _Parameter = par;
            }
        }

        /// <summary>
        /// Gets the mkvextract executable filename
        /// </summary>
        public static String MKV_EXTRACT_FILENAME
        {
            get { return gMKVHelper.IsOnLinux ? "mkvextract" : "mkvextract.exe"; }
        }

        private String _MKVToolnixPath = "";
        private String _MKVExtractFilename = "";
        private StringBuilder _MKVExtractOutput = new StringBuilder();
        private StreamWriter _OutputFileWriter = null;
        private StringBuilder _ErrorBuilder = new StringBuilder();
        private gMKVVersion _Version = null;

        public event MkvExtractProgressUpdatedEventHandler MkvExtractProgressUpdated;
        public event MkvExtractTrackUpdatedEventHandler MkvExtractTrackUpdated;

        private Exception _ThreadedException = null;
        public Exception ThreadedException { get { return _ThreadedException; } }

        private bool _Abort = false;
        public bool Abort
        {
            get { return _Abort; }
            set { _Abort = value; }
        }

        private bool _AbortAll = false;
        public bool AbortAll
        {
            get { return _AbortAll; }
            set { _AbortAll = value; }
        }

        public gMKVExtract(String mkvToonlixPath)
        {
            _MKVToolnixPath = mkvToonlixPath;
            _MKVExtractFilename = Path.Combine(_MKVToolnixPath, MKV_EXTRACT_FILENAME);
        }

        public void ExtractMKVSegmentsThreaded(object argParameters)
        {
            _ThreadedException = null;
            try
            {
                gMKVExtractSegmentsParameters parameters = (gMKVExtractSegmentsParameters)argParameters;
                ExtractMKVSegments(
                    parameters.MKVFile,
                    parameters.MKVSegmentsToExtract,
                    parameters.OutputDirectory,
                    parameters.ChapterType,
                    parameters.TimecodesExtractionMode,
                    parameters.CueExtractionMode,
                    parameters.FilenamePatterns
                );
            }
            catch (Exception ex)
            {
                _ThreadedException = ex;
            }
        }

        private List<TrackParameter> GetTrackParameters(
            gMKVSegment argSeg
            , String argMKVFile
            , String argOutputDirectory
            , MkvChapterTypes argChapterType
            , TimecodesExtractionMode argTimecodesExtractionMode
            , CuesExtractionMode argCueExtractionMode
            , gMKVExtractFilenamePatterns argFilenamePatterns
        )
        {
            // create the new parameter list type
            List<TrackParameter> trackParameterList = new List<TrackParameter>();
            
            // check the selected segment's type
            if (argSeg is gMKVTrack)
            {
                // if we are in a mode that requires timecodes extraction, add the parameter for the track
                if (argTimecodesExtractionMode != TimecodesExtractionMode.NoTimecodes)
                {
                    trackParameterList.Add(new TrackParameter(
                        // Since MKVToolNix v17.0 the timecode word has been replaced with timestamp
                        GetMKVExtractVersion().FileMajorPart >= 17 ? MkvExtractModes.timestamps_v2 : MkvExtractModes.timecodes_v2,
                        "",
                        String.Format("{0}:\"{1}\"",
                            ((gMKVTrack)argSeg).TrackID,
                            GetOutputFilename(argSeg, argOutputDirectory, argMKVFile, argFilenamePatterns, MkvExtractModes.timestamps_v2)
                        ),
                        false,
                        ""
                    ));
                }

                // if we are in a mode that requires cues extraction, add the parameter for the track
                if (argCueExtractionMode != CuesExtractionMode.NoCues)
                {
                    trackParameterList.Add(new TrackParameter(
                        MkvExtractModes.cues,
                        "",
                        String.Format("{0}:\"{1}\"",
                            ((gMKVTrack)argSeg).TrackID,
                            GetOutputFilename(argSeg, argOutputDirectory, argMKVFile, argFilenamePatterns, MkvExtractModes.cues)
                        ),
                        false,
                        ""
                    ));
                }

                // check if the mode requires the extraction of the segment itself
                if (
                    !(
                    (argTimecodesExtractionMode == TimecodesExtractionMode.OnlyTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.NoCues)
                    || (argTimecodesExtractionMode == TimecodesExtractionMode.NoTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.OnlyCues)
                    )
                    || (argTimecodesExtractionMode == TimecodesExtractionMode.OnlyTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.OnlyCues)                    
                    )
                {

                    // add the parameter for extracting the track
                    trackParameterList.Add(new TrackParameter(
                        MkvExtractModes.tracks,
                        "",
                        String.Format("{0}:\"{1}\"",
                            ((gMKVTrack)argSeg).TrackID,
                            GetOutputFilename(argSeg, argOutputDirectory, argMKVFile, argFilenamePatterns, MkvExtractModes.tracks)
                        ),
                        false,
                        ""
                    ));
                }
            }
            else if (argSeg is gMKVAttachment)
            {
                // check if the mode requires the extraction of the segment itself
                if (
                    !(
                    (argTimecodesExtractionMode == TimecodesExtractionMode.OnlyTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.NoCues)
                    || (argTimecodesExtractionMode == TimecodesExtractionMode.NoTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.OnlyCues)
                    )
                    || (argTimecodesExtractionMode == TimecodesExtractionMode.OnlyTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.OnlyCues)
                    )
                {
                    // add the parameter for extracting the attachment
                    trackParameterList.Add(new TrackParameter(
                        MkvExtractModes.attachments,
                        "",
                        String.Format("{0}:\"{1}\"",
                            ((gMKVAttachment)argSeg).ID,
                            GetOutputFilename(argSeg, argOutputDirectory, "", argFilenamePatterns, MkvExtractModes.attachments)
                        ),
                        false,
                        ""
                    ));
                }
            }
            else if (argSeg is gMKVChapter)
            {
                // check if the mode requires the extraction of the segment itself
                if (
                    !(
                    (argTimecodesExtractionMode == TimecodesExtractionMode.OnlyTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.NoCues)
                    || (argTimecodesExtractionMode == TimecodesExtractionMode.NoTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.OnlyCues)
                    )
                    || (argTimecodesExtractionMode == TimecodesExtractionMode.OnlyTimecodes &&
                    argCueExtractionMode == CuesExtractionMode.OnlyCues)
                    )
                {
                    String options = "";

                    // check the chapter's type to determine the output file's extension and options
                    if (argChapterType == MkvChapterTypes.OGM)
                    {
                        options = "--simple";
                    }

                    String chapterFile = GetOutputFilename(argSeg, argOutputDirectory, argMKVFile, argFilenamePatterns, MkvExtractModes.chapters, argChapterType);

                    // add the parameter for extracting the chapters
                    // Since MKVToolNix v17.0, items that were written to the standard output (chapters, tags and cue sheets) are now always written to files instead.
                    trackParameterList.Add(new TrackParameter(
                        MkvExtractModes.chapters,
                        options,
                        GetMKVExtractVersion().FileMajorPart >= 17 ? chapterFile : "",
                        (GetMKVExtractVersion().FileMajorPart < 17),
                        GetMKVExtractVersion().FileMajorPart >= 17 ? "" : chapterFile
                    ));
                }
            }

            return trackParameterList;
        }

        public void ExtractMKVSegments(
            String argMKVFile
            , List<gMKVSegment> argMKVSegmentsToExtract
            , String argOutputDirectory
            , MkvChapterTypes argChapterType
            , TimecodesExtractionMode argTimecodesExtractionMode
            , CuesExtractionMode argCueExtractionMode
            , gMKVExtractFilenamePatterns argFilenamePatterns
        )
        {
            _Abort = false;
            _AbortAll = false;
            _ErrorBuilder.Length = 0;
            _MKVExtractOutput.Length = 0;
            // Analyze the MKV segments and get the initial parameters
            List<TrackParameter> initialParameters = new List<TrackParameter>();
            foreach (gMKVSegment seg in argMKVSegmentsToExtract)
            {
                if (_AbortAll)
                {
                    _ErrorBuilder.AppendLine("User aborted all the processes!");
                    break;
                }
                try
                {
                    initialParameters.AddRange(GetTrackParameters(
                        seg, argMKVFile, argOutputDirectory, argChapterType, argTimecodesExtractionMode, argCueExtractionMode, argFilenamePatterns));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    _ErrorBuilder.AppendLine(String.Format("Segment: {0}" + Environment.NewLine + "Exception: {1}\r\n", seg, ex.Message));
                }                
            }

            // Group the initial parameters, in order to batch extract the mkv segments
            List<TrackParameter> finalParameters = new List<TrackParameter>();
            foreach (TrackParameter initPar in initialParameters)
            {
                TrackParameter currentPar = null;
                foreach (TrackParameter finalPar in finalParameters)
                {
                    if (finalPar.ExtractMode == initPar.ExtractMode)
                    {
                        currentPar = finalPar;
                        break;
                    }
                }
                if (currentPar != null)
                {
                    currentPar.TrackOutput = String.Format("{0} {1}", currentPar.TrackOutput, initPar.TrackOutput);
                }
                else
                {
                    finalParameters.Add(initPar);
                }
            }

            // Time to extract the mkv segments
            foreach (TrackParameter finalPar in finalParameters)
            {
                if (_AbortAll)
                {
                    _ErrorBuilder.AppendLine("User aborted all the processes!");
                    break;
                }
                try
                {
                    if (finalPar.WriteOutputToFile)
                    {
                        _OutputFileWriter = new StreamWriter(finalPar.OutputFilename, false, new UTF8Encoding(false, true));
                    }

                    OnMkvExtractTrackUpdated(argMKVFile, Enum.GetName(finalPar.ExtractMode.GetType(), finalPar.ExtractMode));
                    ExtractMkvSegment(argMKVFile, finalPar);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    _ErrorBuilder.AppendLine(String.Format("Track output: {0}" + Environment.NewLine + "Exception: {1}" + Environment.NewLine, finalPar.TrackOutput, ex.Message));
                }
                finally
                {
                    if (_OutputFileWriter != null)
                    {
                        _OutputFileWriter.Close();
                        _OutputFileWriter = null;
                    }

                    try
                    {
                        // If we have chapters with CUE format, then we read the XML chapters and convert it to CUE
                        if (finalPar.ExtractMode == MkvExtractModes.chapters)
                        {
                            // Since MKVToolNix v17.0, items that were written to the standard output (chapters, tags and cue sheets) are now always written to files instead.
                            String outputFile = GetMKVExtractVersion().FileMajorPart >= 17 ? finalPar.TrackOutput : finalPar.OutputFilename;

                            if (outputFile.ToLower().EndsWith("cue"))
                            {
                                Chapters c = null;
                                using (StreamReader sr = new StreamReader(outputFile))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(Chapters));
                                    c = (Chapters)serializer.Deserialize(sr);
                                }
                                Cue cue = new Cue();
                                cue.File = Path.GetFileName(argMKVFile);
                                cue.FileType = "WAVE";
                                cue.Title = Path.GetFileName(argMKVFile);
                                cue.Tracks = new List<CueTrack>();

                                if (c.EditionEntry != null
                                    && c.EditionEntry.Length > 0
                                    && c.EditionEntry[0].ChapterAtom != null
                                    && c.EditionEntry[0].ChapterAtom.Length > 0)
                                {
                                    Int32 currentChapterTrackNumber = 1;
                                    foreach (ChapterAtom atom in c.EditionEntry[0].ChapterAtom)
                                    {
                                        CueTrack tr = new CueTrack();
                                        tr.Number = currentChapterTrackNumber;
                                        if (atom.ChapterDisplay != null
                                            && atom.ChapterDisplay.Length > 0)
                                        {
                                            tr.Title = atom.ChapterDisplay[0].ChapterString;
                                        }
                                        if (!String.IsNullOrEmpty(atom.ChapterTimeStart)
                                            && atom.ChapterTimeStart.Contains(":"))
                                        {
                                            String[] timeElements = atom.ChapterTimeStart.Split(new String[] { ":" }, StringSplitOptions.None);
                                            if (timeElements.Length == 3)
                                            {
                                                // Find cue minutes from hours and minutes
                                                Int32 hours = Int32.Parse(timeElements[0]);
                                                Int32 minutes = Int32.Parse(timeElements[1]) + 60 * hours;
                                                // Convert nanoseconds to frames (each second is 75 frames)
                                                Int64 nanoSeconds = 0;
                                                Int32 frames = 0;
                                                Int32 secondsLength = timeElements[2].Length;
                                                if (timeElements[2].Contains("."))
                                                {
                                                    secondsLength = timeElements[2].IndexOf(".");
                                                    nanoSeconds = Int64.Parse(timeElements[2].Substring(timeElements[2].IndexOf(".") + 1));
                                                    // I take the integer part of the result action in order to get the first frame
                                                    frames = Convert.ToInt32(Math.Floor(Convert.ToDouble(nanoSeconds) / 1000000000.0 * 75.0));
                                                }
                                                tr.Index = String.Format("{0}:{1}:{2}",
                                                    minutes.ToString("#00")
                                                    , timeElements[2].Substring(0, secondsLength)
                                                    , frames.ToString("00")
                                                    );
                                            }
                                        }

                                        cue.Tracks.Add(tr);
                                        currentChapterTrackNumber++;
                                    }
                                }

                                StringBuilder cueBuilder = new StringBuilder();

                                cueBuilder.AppendFormat("REM GENRE \"\"\r\n");
                                cueBuilder.AppendFormat("REM DATE \"\"\r\n");
                                cueBuilder.AppendFormat("PERFORMER \"\"\r\n");
                                cueBuilder.AppendFormat("TITLE \"{0}\"\r\n", cue.Title);
                                cueBuilder.AppendFormat("FILE \"{0}\" {1}\r\n", cue.File, cue.FileType);

                                foreach (CueTrack tr in cue.Tracks)
                                {
                                    cueBuilder.AppendFormat("\tTRACK {0} AUDIO\r\n", tr.Number.ToString("00"));
                                    cueBuilder.AppendFormat("\t\tTITLE \"{0}\"\r\n", tr.Title);
                                    cueBuilder.AppendFormat("\t\tPERFORMER \"\"\r\n");
                                    cueBuilder.AppendFormat("\t\tINDEX 01 {0}\r\n", tr.Index);
                                }

                                using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
                                {
                                    sw.Write(cueBuilder.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc);
                        _ErrorBuilder.AppendLine(String.Format("Track output: {0}" + Environment.NewLine + "Exception: {1}" + Environment.NewLine, finalPar.TrackOutput, exc.Message));
                    }
                }
            }

            // check for errors
            if (_ErrorBuilder.Length > 0)
            {
                throw new Exception(_ErrorBuilder.ToString());
            }
        }

        public void ExtractMKVTimecodesThreaded(object argParameters)
        {
            _ThreadedException = null;
            try
            {
                gMKVExtractSegmentsParameters parameters = (gMKVExtractSegmentsParameters)argParameters;
                ExtractMKVSegments(
                    parameters.MKVFile,
                    parameters.MKVSegmentsToExtract,
                    parameters.OutputDirectory,
                    parameters.ChapterType,
                    TimecodesExtractionMode.OnlyTimecodes,
                    CuesExtractionMode.NoCues,
                    parameters.FilenamePatterns
                );
            }
            catch (Exception ex)
            {
                _ThreadedException = ex;
            }
        }

        public void ExtractMKVCuesThreaded(object argParameters)
        {
            _ThreadedException = null;
            try
            {
                gMKVExtractSegmentsParameters parameters = (gMKVExtractSegmentsParameters)argParameters;
                ExtractMKVSegments(
                    parameters.MKVFile,
                    parameters.MKVSegmentsToExtract,
                    parameters.OutputDirectory,
                    parameters.ChapterType,
                    TimecodesExtractionMode.NoTimecodes,
                    CuesExtractionMode.OnlyCues,
                    parameters.FilenamePatterns
                );
            }
            catch (Exception ex)
            {
                _ThreadedException = ex;
            }
        }

        public void ExtractMkvCuesheetThreaded(object argParameters)
        {
            _ThreadedException = null;
            try
            {
                gMKVExtractSegmentsParameters parameters = (gMKVExtractSegmentsParameters)argParameters;
                ExtractMkvCuesheet(
                    parameters.MKVFile,
                    parameters.OutputDirectory,
                    parameters.FilenamePatterns
                );
            }
            catch (Exception ex)
            {
                _ThreadedException = ex;
            }
        }

        public void ExtractMkvCuesheet(String argMKVFile, String argOutputDirectory, gMKVExtractFilenamePatterns argFilenamePatterns)
        {
            _Abort = false;
            _AbortAll = false;
            _ErrorBuilder.Length = 0;
            _MKVExtractOutput.Length = 0;
            String par = String.Format("cuesheet \"{0}\"", argMKVFile);
            String cueFile = GetOutputFilename(null, argOutputDirectory, argMKVFile, argFilenamePatterns, MkvExtractModes.cuesheet);
            try
            {
                OnMkvExtractTrackUpdated(argMKVFile, "Cue Sheet");
                // Since MKVToolNix v17.0, items that were written to the standard output (chapters, tags and cue sheets) are now always written to files instead.
                if (GetMKVExtractVersion().FileMajorPart < 17)
                {
                    _OutputFileWriter = new StreamWriter(cueFile, false, new UTF8Encoding(false, true));
                }
                ExtractMkvSegment(
                    argMKVFile
                     , new TrackParameter(
                        MkvExtractModes.cuesheet
                        , ""
                        , GetMKVExtractVersion().FileMajorPart >= 17 ? cueFile : ""
                        , (GetMKVExtractVersion().FileMajorPart < 17)
                        , GetMKVExtractVersion().FileMajorPart >= 17 ? "" : cueFile
                    )
               );
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex);
            }
            finally
            {
                if (_OutputFileWriter != null)
                {
                    _OutputFileWriter.Close();
                    _OutputFileWriter = null;
                }
            }
            // check for errors
            if (_ErrorBuilder.Length > 0)
            {
                throw new Exception(_ErrorBuilder.ToString());
            }
        }

        public void ExtractMkvTagsThreaded(object argParameters)
        {
            _ThreadedException = null;
            try
            {
                gMKVExtractSegmentsParameters parameters = (gMKVExtractSegmentsParameters)argParameters;
                ExtractMkvTags(
                    parameters.MKVFile,
                    parameters.OutputDirectory,
                    parameters.FilenamePatterns
                );
            }
            catch (Exception ex)
            {
                _ThreadedException = ex;
            }
        }

        public void ExtractMkvTags(String argMKVFile, String argOutputDirectory, gMKVExtractFilenamePatterns argFilenamePatterns)
        {
            _Abort = false;
            _AbortAll = false;
            _ErrorBuilder.Length = 0;
            _MKVExtractOutput.Length = 0;
            String par = String.Format("tags \"{0}\"", argMKVFile);

            String tagsFile = GetOutputFilename(null, argOutputDirectory, argMKVFile, argFilenamePatterns, MkvExtractModes.tags);
            try
            {
                OnMkvExtractTrackUpdated(argMKVFile, "Tags");
                // Since MKVToolNix v17.0, items that were written to the standard output (chapters, tags and cue sheets) are now always written to files instead.
                if (GetMKVExtractVersion().FileMajorPart < 17)
                {
                    _OutputFileWriter = new StreamWriter(tagsFile, false, new UTF8Encoding(false, true));
                }
                ExtractMkvSegment(
                    argMKVFile
                    , new TrackParameter(
                        MkvExtractModes.tags
                        , ""
                        , GetMKVExtractVersion().FileMajorPart >= 17 ? tagsFile : ""
                        , (GetMKVExtractVersion().FileMajorPart < 17)
                        , GetMKVExtractVersion().FileMajorPart >= 17 ? "" : tagsFile
                    )
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                if (_OutputFileWriter != null)
                {
                    _OutputFileWriter.Close();
                    _OutputFileWriter = null;
                }
            }
            // check for errors
            if (_ErrorBuilder.Length > 0)
            {
                throw new Exception(_ErrorBuilder.ToString());
            }
        }

        protected void OnMkvExtractProgressUpdated(Int32 progress)
        {
            if (MkvExtractProgressUpdated != null)
                MkvExtractProgressUpdated(progress);
        }

        protected void OnMkvExtractTrackUpdated(String filename, String trackName)
        {
            if (MkvExtractTrackUpdated != null)
                MkvExtractTrackUpdated(filename, trackName);
        }

        private void ExtractMkvSegment(String argMKVFile, TrackParameter argParameter)
        {
            OnMkvExtractProgressUpdated(0);
            // check for existence of MKVExtract
            if (!File.Exists(_MKVExtractFilename)) { throw new Exception(String.Format("Could not find {0}!" + Environment.NewLine + "{1}", MKV_EXTRACT_FILENAME, _MKVExtractFilename)); }
            DataReceivedEventHandler handler = null;

            // Check the file version of the mkvextract
            if (_Version == null)
            {
                _Version = GetMKVExtractVersion();
            }

            // Since MKVToolNix v17.0, items that were written to the standard output (chapters, tags and cue sheets) are now always written to files instead.
            if (argParameter.WriteOutputToFile && _Version.FileMajorPart < 17)
            {
                handler = myProcess_OutputDataReceived_WriteToFile;
            }
            else
            {
                handler = myProcess_OutputDataReceived;
            }

            ExecuteMkvExtract(argMKVFile, argParameter, handler);
        }

        private void ExecuteMkvExtract(String argMKVFile, TrackParameter argParameter, DataReceivedEventHandler argHandler)
        {
            using (Process myProcess = new Process())
            {
                ProcessStartInfo myProcessInfo = new ProcessStartInfo();
                myProcessInfo.FileName = _MKVExtractFilename;

                // Check the file version of the mkvextract
                if (_Version == null)
                {
                    _Version = GetMKVExtractVersion();
                }

                String parameters = "";
                String LC_ALL = "";
                String LANG = "";
                String LC_MESSAGES = "";

                // Since MKVToolNix v9.7.0, start using the --gui-mode option
                if (_Version.FileMajorPart > 9 ||
                    (_Version.FileMajorPart == 9 && _Version.FileMinorPart >= 7))
                {
                    parameters = "--gui-mode";
                }
                else {
                    // Before MKVToolNix 9.7.0, the safest way to ensure English output on Linux is throught the EnvironmentVariables
                    if (gMKVHelper.IsOnLinux)
                    {
                        // Get the original values
                        LC_ALL = Environment.GetEnvironmentVariable("LC_ALL", EnvironmentVariableTarget.Process);
                        LANG = Environment.GetEnvironmentVariable("LANG", EnvironmentVariableTarget.Process);
                        LC_MESSAGES = Environment.GetEnvironmentVariable("LC_MESSAGES", EnvironmentVariableTarget.Process);

                        gMKVLogger.Log(String.Format("Detected Environment Variables: LC_ALL=\"{0}\",LANG=\"{1}\",LC_MESSAGES=\"{2}\"",
                            LC_ALL, LANG, LC_MESSAGES));

                        // Set the english locale
                        Environment.SetEnvironmentVariable("LC_ALL", "en_US.UTF-8", EnvironmentVariableTarget.Process);
                        Environment.SetEnvironmentVariable("LANG", "en_US.UTF-8", EnvironmentVariableTarget.Process);
                        Environment.SetEnvironmentVariable("LC_MESSAGES", "en_US.UTF-8", EnvironmentVariableTarget.Process);

                        gMKVLogger.Log("Setting Environment Variables: LC_ALL=LANG=LC_MESSAGES=\"en_US.UTF-8\"");
                    }
                }

                // if on Linux, the language output must be defined from the environment variables LC_ALL, LANG, and LC_MESSAGES
                // After talking with Mosu, the language output is defined from ui-language, with different language codes for Windows and Linux
                String options = "";
                if (gMKVHelper.IsOnLinux)
                {
                    options = String.Format("{0} --ui-language en_US {1}", parameters, argParameter.Options);
                }
                else
                {
                    options = String.Format("{0} --ui-language en {1}", parameters, argParameter.Options);
                }

                // Since MKVToolNix v17.0, the syntax has changed
                if (_Version.FileMajorPart >= 17)
                {
                    // new Syntax
                    // mkvextract {source-filename} {mode1} [options] [extraction-spec1] [mode2] [options] [extraction-spec2] […] 
                    myProcessInfo.Arguments = String.Format(" \"{0}\" {1} {2} {3} ",
                        argMKVFile,
                        Enum.GetName(argParameter.ExtractMode.GetType(), argParameter.ExtractMode),
                        options,
                        string.IsNullOrWhiteSpace(argParameter.TrackOutput) 
                        || argParameter.ExtractMode == MkvExtractModes.tracks 
                        || argParameter.ExtractMode == MkvExtractModes.timecodes_v2 
                        || argParameter.ExtractMode == MkvExtractModes.timestamps_v2
                        || argParameter.ExtractMode == MkvExtractModes.cues
                        || argParameter.ExtractMode == MkvExtractModes.attachments ? argParameter.TrackOutput : string.Format("\"{0}\"", argParameter.TrackOutput)
                    );
                }
                else
                {
                    // old Syntax
                    // mkvextract {mode} {source-filename} [options] [extraction-spec]
                    myProcessInfo.Arguments = String.Format(" {0} \"{1}\" {2} {3}",
                           Enum.GetName(argParameter.ExtractMode.GetType(), argParameter.ExtractMode),
                        argMKVFile,
                        options,
                        argParameter.TrackOutput
                    );
                }                

                myProcessInfo.UseShellExecute = false;
                myProcessInfo.RedirectStandardOutput = true;
                myProcessInfo.StandardOutputEncoding = Encoding.UTF8;
                myProcessInfo.RedirectStandardError = true;
                myProcessInfo.StandardErrorEncoding = Encoding.UTF8;
                myProcessInfo.CreateNoWindow = true;
                myProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.StartInfo = myProcessInfo;
                
                //// Register the event for reading the StandardOutput
                //myProcess.OutputDataReceived += argHandler;

                Debug.WriteLine(myProcessInfo.Arguments);
                gMKVLogger.Log(String.Format("\"{0}\" {1}", _MKVExtractFilename, myProcessInfo.Arguments));

                // Start the mkvinfo process
                myProcess.Start();

                //// Start reading the output
                //myProcess.BeginOutputReadLine();

                // Read the Standard output character by character
                gMKVHelper.ReadStreamPerCharacter(myProcess, argHandler);

                // Wait for the process to exit
                myProcess.WaitForExit();
                
                //// unregister the event
                //myProcess.OutputDataReceived -= argHandler;

                // Debug write the exit code
                Debug.WriteLine(String.Format("Exit code: {0}", myProcess.ExitCode));

                // Check the exit code
                // ExitCode 1 is for warnings only, so ignore it
                if (myProcess.ExitCode > 1)
                {
                    // something went wrong!
                    throw new Exception(String.Format("Mkvextract exited with error code {0}!" 
                        + Environment.NewLine + Environment.NewLine + "Errors reported:" + Environment.NewLine + "{1}",
                        myProcess.ExitCode, _ErrorBuilder.ToString()));
                }
                else if (myProcess.ExitCode < 0)
                {
                    // user aborted the current procedure!
                    throw new Exception("User aborted the current process!");
                }

                // Before MKVToolNix 9.7.0, the safest way to ensure English output on Linux is throught the EnvironmentVariables
                if (gMKVHelper.IsOnLinux)
                {
                    if (_Version.FileMajorPart < 9 ||
                        (_Version.FileMajorPart == 9 && _Version.FileMinorPart < 7))
                    {
                        // Reset the environment vairables to their original values
                        Environment.SetEnvironmentVariable("LC_ALL", LC_ALL, EnvironmentVariableTarget.Process);
                        Environment.SetEnvironmentVariable("LANG", LANG, EnvironmentVariableTarget.Process);
                        Environment.SetEnvironmentVariable("LC_MESSAGES", LC_MESSAGES, EnvironmentVariableTarget.Process);

                        gMKVLogger.Log(String.Format("Resetting Environment Variables: LC_ALL=\"{0}\",LANG=\"{1}\",LC_MESSAGES=\"{2}\"",
                            LC_ALL, LANG, LC_MESSAGES));
                    }
                }
            }
        }

        public gMKVVersion GetMKVExtractVersion()
        {
            if (_Version != null)
            {
                return _Version;
            }
            // check for existence of mkvextract
            if (!File.Exists(_MKVExtractFilename)) { throw new Exception(String.Format("Could not find {0}!" + Environment.NewLine + "{1}", MKV_EXTRACT_FILENAME, _MKVExtractFilename)); }

            if (gMKVHelper.IsOnLinux)
            {
                // When on Linux, we need to run mkvextract

                // Clear the mkvextract output
                _MKVExtractOutput.Length = 0;
                // Clear the error builder
                _ErrorBuilder.Length = 0;

                // Execute mkvextract
                List<OptionValue> options = new List<OptionValue>();
                options.Add(new OptionValue(MkvExtractGlobalOptions.version, ""));

                using (Process myProcess = new Process())
                {
                    // if on Linux, the language output must be defined from the environment variables LC_ALL, LANG, and LC_MESSAGES
                    // After talking with Mosu, the language output is defined from ui-language, with different language codes for Windows and Linux
                    if (gMKVHelper.IsOnLinux)
                    {
                        options.Add(new OptionValue(MkvExtractGlobalOptions.ui_language, "en_US"));
                    }
                    else
                    {
                        options.Add(new OptionValue(MkvExtractGlobalOptions.ui_language, "en"));
                    }

                    ProcessStartInfo myProcessInfo = new ProcessStartInfo();
                    myProcessInfo.FileName = _MKVExtractFilename;
                    myProcessInfo.Arguments = String.Format("{0}", ConvertOptionValueListToString(options));
                    myProcessInfo.UseShellExecute = false;
                    myProcessInfo.RedirectStandardOutput = true;
                    myProcessInfo.StandardOutputEncoding = Encoding.UTF8;
                    myProcessInfo.RedirectStandardError = true;
                    myProcessInfo.StandardErrorEncoding = Encoding.UTF8;
                    myProcessInfo.CreateNoWindow = true;
                    myProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    myProcess.StartInfo = myProcessInfo;

                    Debug.WriteLine(myProcessInfo.Arguments);
                    gMKVLogger.Log(String.Format("\"{0}\" {1}", _MKVExtractFilename, myProcessInfo.Arguments));

                    // Start the mkvextract process
                    myProcess.Start();

                    // Read the Standard output character by character
                    gMKVHelper.ReadStreamPerCharacter(myProcess, myProcess_OutputDataReceived);

                    // Wait for the process to exit
                    myProcess.WaitForExit();

                    // Debug write the exit code
                    Debug.WriteLine(String.Format("Exit code: {0}", myProcess.ExitCode));
                    gMKVLogger.Log(String.Format("Exit code: {0}", myProcess.ExitCode));

                    // Check the exit code
                    // ExitCode 1 is for warnings only, so ignore it
                    if (myProcess.ExitCode > 1)
                    {
                        // something went wrong!
                        throw new Exception(String.Format("Mkvmerge exited with error code {0}!" +
                            Environment.NewLine + Environment.NewLine + "Errors reported:" + Environment.NewLine + "{1}",
                            myProcess.ExitCode, _ErrorBuilder.ToString()));
                    }
                }

                // Parse version info
                ParseVersionOutput();

                // Clear the mkvextract output
                _MKVExtractOutput.Length = 0;
            }
            else
            {
                // When on Windows, we can use FileVersionInfo.GetVersionInfo
                var version = FileVersionInfo.GetVersionInfo(_MKVExtractFilename);
                _Version = new gMKVToolNix.gMKVVersion()
                {
                    FileMajorPart = version.FileMajorPart,
                    FileMinorPart = version.FileMinorPart,
                    FilePrivatePart = version.FilePrivatePart
                };
            }
            if (_Version != null)
            {
                gMKVLogger.Log(String.Format("Detected mkvextract version: {0}.{1}.{2}",
                    _Version.FileMajorPart,
                    _Version.FileMinorPart,
                    _Version.FilePrivatePart
                ));
            }
            return _Version;
        }

        void myProcess_OutputDataReceived_WriteToFile(object sender, DataReceivedEventArgs e)
        {
            // check for user abort
            if (_Abort) 
            {
                ((Process)sender).Kill();
                _Abort = false;
                return;
            }
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                // add the line to the output stringbuilder
                _OutputFileWriter.WriteLine(e.Data);
                // check for progress (in gui-mode)
                if (e.Data.Contains("#GUI#progress"))
                {
                    OnMkvExtractProgressUpdated(Convert.ToInt32(e.Data.Substring(e.Data.IndexOf(" ") + 1, e.Data.IndexOf("%") - e.Data.IndexOf(" ") - 1)));
                }
                // check for progress
                else if (e.Data.Contains("Progress:"))
                {
                    OnMkvExtractProgressUpdated(Convert.ToInt32(e.Data.Substring(e.Data.IndexOf(":") + 1, e.Data.IndexOf("%") - e.Data.IndexOf(":") - 1)));
                }
                else if (e.Data.Contains("#GUI#error"))
                {
                    _ErrorBuilder.AppendLine(e.Data.Substring(e.Data.IndexOf(" ") + 1).Trim());
                }
                // check for errors
                else if (e.Data.Contains("Error:"))
                {
                    _ErrorBuilder.AppendLine(e.Data.Substring(e.Data.IndexOf(":") + 1).Trim());
                }
                // debug write the output line
                Debug.WriteLine(e.Data);
                // log the output
                gMKVLogger.Log(e.Data);
            }
        }

        void myProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // check for user abort
            if (_Abort)
            {
                ((Process)sender).Kill();
                _Abort = false;
                return;
            }
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                // add the line to the output stringbuilder
                _MKVExtractOutput.AppendLine(e.Data);
                // check for progress (in gui-mode)
                if (e.Data.Contains("#GUI#progress"))
                {
                    OnMkvExtractProgressUpdated(Convert.ToInt32(e.Data.Substring(e.Data.IndexOf(" ") + 1, e.Data.IndexOf("%") - e.Data.IndexOf(" ") - 1)));
                }
                // check for progress
                else if (e.Data.Contains("Progress:"))
                {
                    OnMkvExtractProgressUpdated(Convert.ToInt32(e.Data.Substring(e.Data.IndexOf(":") + 1, e.Data.IndexOf("%") - e.Data.IndexOf(":") - 1)));                    
                }
                else if (e.Data.Contains("#GUI#error"))
                {
                    _ErrorBuilder.AppendLine(e.Data.Substring(e.Data.IndexOf(" ") + 1).Trim());
                }
                // check for errors
                else if (e.Data.Contains("Error:"))
                {
                    _ErrorBuilder.AppendLine(e.Data.Substring(e.Data.IndexOf(":") + 1).Trim());
                }
                // debug write the output line
                Debug.WriteLine(e.Data);
                // log the output
                gMKVLogger.Log(e.Data);
            }
        }

        private void ParseVersionOutput()
        {
            String fileMajorVersion = "0";
            String fileMinorVersion = "0";
            String filePrivateVersion = "0";
            if (_MKVExtractOutput != null && _MKVExtractOutput.Length > 0)
            {
                String[] outputLines = _MKVExtractOutput.ToString().Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String outputLine in outputLines)
                {
                    if (outputLine.StartsWith("mkvextract v"))
                    {
                        String versionString = outputLine.Substring(11);
                        versionString = versionString.Substring(1, versionString.IndexOf(" "));
                        if (versionString.Contains("."))
                        {
                            String[] parts = versionString.Split(new String[] { "." }, StringSplitOptions.None);
                            if (parts.Length >= 2)
                            {
                                fileMajorVersion = parts[0];
                                fileMinorVersion = parts[1];
                                if (parts.Length > 2)
                                {
                                    filePrivateVersion = parts[2];
                                }
                            }
                        }
                        break;
                    }
                }
            }

            gMKVVersion version = new gMKVToolNix.gMKVVersion()
            {
                FileMajorPart = Convert.ToInt32(fileMajorVersion),
                FileMinorPart = Convert.ToInt32(fileMinorVersion),
                FilePrivatePart = Convert.ToInt32(filePrivateVersion)
            };

            _Version = version;
        }

        public string ReplaceFilenamePlaceholders(gMKVSegment argSeg, string argMKVFile, string argFilenamePattern)
        {
            string mkvFilenameNoExt = Path.GetFileNameWithoutExtension(argMKVFile);
            string mkvFilename = Path.GetFileName(argMKVFile);

            // Common placeholders
            argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.FilenameNoExt, mkvFilenameNoExt);
            argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.Filename, mkvFilename);

            // Track placeholders
            if (argSeg is gMKVTrack)
            {
                gMKVTrack track = argSeg as gMKVTrack;

                // Common Track placeholders
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackNumber, track.TrackNumber.ToString());
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackNumber_0, track.TrackNumber.ToString("0"));
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackNumber_00, track.TrackNumber.ToString("00"));
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackNumber_000, track.TrackNumber.ToString("000"));
                
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackID, track.TrackID.ToString());
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackID_0, track.TrackID.ToString("0"));
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackID_00, track.TrackID.ToString("00"));
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackID_000, track.TrackID.ToString("000"));

                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackName , track.TrackName);
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackLanguage , track.Language);
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackCodecID, track.CodecID);
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackCodecPrivate, track.CodecPrivate);
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackDelay, track.Delay.ToString());
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.TrackEffectiveDelay, track.EffectiveDelay.ToString());

                // Video Track placeholders
                if (track.TrackType == MkvTrackType.video)
                {
                    argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.VideoPixelWidth, track.VideoPixelWidth.ToString());
                    argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.VideoPixelHeight ,track.VideoPixelHeight.ToString());
                }

                // Audio Track placeholders
                if (track.TrackType == MkvTrackType.audio)
                {
                    argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AudioSamplingFrequency, track.AudioSamplingFrequency.ToString());
                    argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AudioChannels, track.AudioChannels.ToString());
                }
            }

            // Attachment placeholders
            if (argSeg is gMKVAttachment)
            {
                gMKVAttachment attachment = argSeg as gMKVAttachment;
                
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentID, attachment.ID.ToString());
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentID_0, attachment.ID.ToString("0"));
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentID_00, attachment.ID.ToString("00"));
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentID_000, attachment.ID.ToString("000"));

                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentFilename, attachment.Filename);
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentMimeType, attachment.MimeType);
                argFilenamePattern = argFilenamePattern.Replace(gMKVExtractFilenamePatterns.AttachmentFileSize, attachment.FileSize);
            }

            return argFilenamePattern;
        }

        public string GetOutputFilename(
            gMKVSegment argSeg
            , string argOutputDirectory
            , string argMKVFile
            , gMKVExtractFilenamePatterns argFilenamePatterns
            , MkvExtractModes argMkvExtractMode
            , MkvChapterTypes argMkvChapterType = MkvChapterTypes.XML
            )
        {
            string outputFilename = "";
            string outputFileExtension = "";
            string argMkvFilenameNoExt = Path.GetFileNameWithoutExtension(argMKVFile);
            string replacedFilePattern = "";

            switch (argMkvExtractMode)
            {
                case MkvExtractModes.tracks:
                    if (!(argSeg is gMKVTrack))
                    {
                        throw new Exception("Called GetOutputFilename without track!");
                    }
                    String outputDelayPart = "";

                    // check the track's type in order to get the output file's extension and the delay for audio tracks
                    switch (((gMKVTrack)argSeg).TrackType)
                    {
                        case MkvTrackType.video:
                            // get the extension of the output via the CODEC_ID of the track
                            outputFileExtension = getVideoFileExtensionFromCodecID((gMKVTrack)argSeg);
                            replacedFilePattern = ReplaceFilenamePlaceholders(argSeg, argMKVFile, argFilenamePatterns.VideoTrackFilenamePattern);
                            break;
                        case MkvTrackType.audio:
                            // add the delay to the extraOutput for the track filename
                            outputDelayPart = String.Format("_DELAY {0}ms", ((gMKVTrack)argSeg).EffectiveDelay.ToString(CultureInfo.InvariantCulture));
                            // get the extension of the output via the CODEC_ID of the track
                            outputFileExtension = getAudioFileExtensionFromCodecID((gMKVTrack)argSeg);
                            replacedFilePattern = ReplaceFilenamePlaceholders(argSeg, argMKVFile, argFilenamePatterns.AudioTrackFilenamePattern);
                            break;
                        case MkvTrackType.subtitles:
                            // get the extension of the output via the CODEC_ID of the track
                            outputFileExtension = getSubtitleFileExtensionFromCodecID((gMKVTrack)argSeg);
                            replacedFilePattern = ReplaceFilenamePlaceholders(argSeg, argMKVFile, argFilenamePatterns.SubtitleTrackFilenamePattern);
                            break;
                        default:
                            break;
                    }
                    outputFilename = Path.Combine(
                        argOutputDirectory,                        
                        String.Format("{0}.{1}",
                            replacedFilePattern
                            ,outputFileExtension)
                    );
                    break;
                case MkvExtractModes.tags:
                    outputFilename = Path.Combine(
                        argOutputDirectory,
                        String.Format("{0}_tags.xml", argMkvFilenameNoExt));
                    break;
                case MkvExtractModes.attachments:
                    if (!(argSeg is gMKVAttachment))
                    {
                        throw new Exception("Called GetOutputFilename without attachment!");
                    }
                    outputFilename = Path.Combine(
                        argOutputDirectory,
                        ReplaceFilenamePlaceholders(argSeg, argMKVFile, argFilenamePatterns.AttachmentFilenamePattern)
                    );
                    break;
                case MkvExtractModes.chapters:
                    // check the chapter's type to determine the output file's extension and options
                    switch (argMkvChapterType)
                    {
                        case MkvChapterTypes.XML:
                            outputFileExtension = "xml";
                            break;
                        case MkvChapterTypes.OGM:
                            outputFileExtension = "ogm.txt";
                            break;
                        case MkvChapterTypes.CUE:
                            outputFileExtension = "cue";
                            break;
                        default:
                            break;
                    }
                    outputFilename = Path.Combine(
                            argOutputDirectory,
                            String.Format("{0}.{1}",
                                ReplaceFilenamePlaceholders(argSeg, argMKVFile, argFilenamePatterns.ChapterFilenamePattern),
                                outputFileExtension)
                    );
                    break;
                case MkvExtractModes.cuesheet:
                    outputFilename = Path.Combine(
                        argOutputDirectory,
                        String.Format("{0}_cuesheet.cue", argMkvFilenameNoExt));
                    break;
                case MkvExtractModes.timecodes_v2:
                case MkvExtractModes.timestamps_v2:
                    if (!(argSeg is gMKVTrack))
                    {
                        throw new Exception("Called GetOutputFilename without track/timestamps!");
                    }
                    outputFilename = Path.Combine(
                        argOutputDirectory,
                        String.Format("{0}_track{1}_[{2}].tc.txt",
                            argMkvFilenameNoExt,
                            ((gMKVTrack)argSeg).TrackNumber,
                            ((gMKVTrack)argSeg).Language));
                    break;
                case MkvExtractModes.cues:
                    if (!(argSeg is gMKVTrack))
                    {
                        throw new Exception("Called GetOutputFilename without track/cues!");
                    }
                    outputFilename = Path.Combine(
                        argOutputDirectory,
                        String.Format("{0}_track{1}_[{2}].cue",
                            argMkvFilenameNoExt,
                            ((gMKVTrack)argSeg).TrackNumber,
                            ((gMKVTrack)argSeg).Language));
                    break;
                default:
                    break;
            }

            // Check if file already exists
            while (File.Exists(outputFilename))
            {
                string outputFilenameDirectory = Path.GetDirectoryName(outputFilename);
                string outputFilenameWithoutExtension = Path.GetFileNameWithoutExtension(outputFilename);
                string outputFilenameExtension = Path.GetExtension(outputFilename);
                int lastDotIndex = outputFilenameWithoutExtension.LastIndexOf('.');

                string outputFilenameCounterString = "";
                int outputFilenameCounter = 0;
                // Check if the filename contains a dot
                if (lastDotIndex > -1)
                {
                    // Get the last part of filename after the last dot
                    outputFilenameCounterString = outputFilenameWithoutExtension.Substring(lastDotIndex + 1);
                    // Check if it's an integer (counter)
                    if (int.TryParse(outputFilenameCounterString, out outputFilenameCounter))
                    {
                        // Isolate the filename without the counter part
                        outputFilenameWithoutExtension = outputFilenameWithoutExtension.Substring(0, lastDotIndex);
                    }
                }

                outputFilename = Path.Combine(
                    outputFilenameDirectory, 
                    string.Format("{0}.{1}{2}", outputFilenameWithoutExtension, (outputFilenameCounter + 1), outputFilenameExtension)
                );
            }

            return outputFilename;
        }

        private String getVideoFileExtensionFromCodecID(gMKVTrack argTrack)
        {
            String outputFileExtension = "";
            if (argTrack.CodecID.ToUpper().Contains("V_MS/VFW/FOURCC"))
            {
                outputFileExtension = "avi";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_UNCOMPRESSED"))
            {
                outputFileExtension = "raw";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_MPEG4/ISO/"))
            {
                outputFileExtension = "avc";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_MPEGH/ISO/HEVC"))
            {
                outputFileExtension = "hevc";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_MPEG4/MS/V3"))
            {
                outputFileExtension = "mp4";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_MPEG1"))
            {
                outputFileExtension = "mpg";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_MPEG2"))
            {
                outputFileExtension = "mpg";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_REAL/"))
            {
                outputFileExtension = "rm";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_QUICKTIME"))
            {
                outputFileExtension = "mov";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_THEORA"))
            {
                outputFileExtension = "ogv";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_PRORES"))
            {
                outputFileExtension = "mov";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_VP"))
            {
                outputFileExtension = "ivf";
            }
            else if (argTrack.CodecID.ToUpper().Contains("V_DIRAC"))
            {
                outputFileExtension = "drc";
            }
            else
            {
                outputFileExtension = "mkv";
            }
            return outputFileExtension;
        }

        private String getAudioFileExtensionFromCodecID(gMKVTrack argTrack)
        {
            String outputFileExtension = "";
            if (argTrack.CodecID.ToUpper().Contains("A_MPEG/L3"))
            {
                outputFileExtension = "mp3";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_MPEG/L2"))
            {
                outputFileExtension = "mp2";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_MPEG/L1"))
            {
                outputFileExtension = "mpa";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_PCM"))
            {
                outputFileExtension = "wav";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_MPC"))
            {
                outputFileExtension = "mpc";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_AC3"))
            {
                outputFileExtension = "ac3";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_EAC3"))
            {
                outputFileExtension = "eac3"; 
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_ALAC"))
            {
                outputFileExtension = "caf";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_DTS"))
            {
                outputFileExtension = "dts";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_VORBIS"))
            {
                outputFileExtension = "ogg";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_FLAC"))
            {
                outputFileExtension = "flac";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_REAL"))
            {
                outputFileExtension = "ra";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_MS/ACM"))
            {
                outputFileExtension = "wav";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_AAC"))
            {
                outputFileExtension = "aac";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_QUICKTIME"))
            {
                outputFileExtension = "mov";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_TRUEHD"))
            {
                outputFileExtension = "thd";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_TTA1"))
            {
                outputFileExtension = "tta";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_WAVPACK4"))
            {
                outputFileExtension = "wv";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_OPUS"))
            {
                outputFileExtension = "opus";
            }
            else if (argTrack.CodecID.ToUpper().Contains("A_MLP"))
            {
                outputFileExtension = "mlp";
            }
            else
            {
                outputFileExtension = "mka";
            }
            return outputFileExtension;
        }

        private String getSubtitleFileExtensionFromCodecID(gMKVTrack argTrack)
        {
            String outputFileExtension = "";
            if (argTrack.CodecID.ToUpper().Contains("S_TEXT/UTF8"))
            {
                outputFileExtension = "srt";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_TEXT/ASCII"))
            {
                outputFileExtension = "srt";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_TEXT/SSA"))
            {
                outputFileExtension = "ass";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_TEXT/ASS"))
            {
                outputFileExtension = "ass";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_TEXT/USF"))
            {
                outputFileExtension = "usf";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_TEXT/WEBVTT"))
            {
                outputFileExtension = "webvtt";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_IMAGE/BMP"))
            {
                outputFileExtension = "sub";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_VOBSUB"))
            {
                outputFileExtension = "sub";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_DVBSUB"))
            {
                outputFileExtension = "dvbsub";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_HDMV/PGS"))
            {
                outputFileExtension = "sup";
            }
            else if (argTrack.CodecID.ToUpper().Contains("S_HDMV/TEXTST"))
            {
                outputFileExtension = "textst";
            }            
            else if (argTrack.CodecID.ToUpper().Contains("S_KATE"))
            {
                outputFileExtension = "ogg";
            }
            else
            {
                outputFileExtension = "sub";
            }
            return outputFileExtension;
        }

        private String ConvertOptionValueListToString(List<OptionValue> listOptionValue)
        {
            StringBuilder optionString = new StringBuilder();
            foreach (OptionValue optVal in listOptionValue)
            {
                optionString.AppendFormat(" {0} {1}", ConvertEnumOptionToStringOption(optVal.Option), optVal.Parameter);
            }
            return optionString.ToString();
        }

        private String ConvertEnumOptionToStringOption(MkvExtractGlobalOptions enumOption)
        {
            return String.Format("--{0}", Enum.GetName(typeof(MkvExtractGlobalOptions), enumOption).Replace("_", "-"));
        }
    }
}
