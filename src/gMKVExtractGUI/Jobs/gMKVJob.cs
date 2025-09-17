using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using gMKVToolNix.Forms;
using gMKVToolNix.MkvExtract;
using gMKVToolNix.Segments;

namespace gMKVToolNix.Jobs
{
    public delegate void gMkvExtractMethod(object parameterList);

    [Serializable]
    [SupportedOSPlatform("windows")]
    [System.Xml.Serialization.XmlInclude(typeof(List<gMKVSegment>))]
    [System.Xml.Serialization.XmlInclude(typeof(gMKVSegment))]
    [System.Xml.Serialization.XmlInclude(typeof(gMKVTrack))]
    [System.Xml.Serialization.XmlInclude(typeof(gMKVChapter))]
    [System.Xml.Serialization.XmlInclude(typeof(gMKVAttachment))]
    public class gMKVJob
    {
        public FormMkvExtractionMode ExtractionMode { get; set; }

        public string MKVToolnixPath { get; set; }

        public gMKVExtractSegmentsParameters ParametersList { get; set; }

        public gMKVJob(FormMkvExtractionMode argExtractionMode, string argMKVToolnixPath, gMKVExtractSegmentsParameters argParameters)
        {
            ExtractionMode = argExtractionMode;
            MKVToolnixPath = argMKVToolnixPath;
            ParametersList = argParameters;
        }

        public gMkvExtractMethod ExtractMethod(gMKVExtract argGmkvExtract) 
        {
            return ExtractionMode switch
            {
                FormMkvExtractionMode.Tracks => argGmkvExtract.ExtractMKVSegmentsThreaded,
                FormMkvExtractionMode.Cue_Sheet => argGmkvExtract.ExtractMkvCuesheetThreaded,
                FormMkvExtractionMode.Tags => argGmkvExtract.ExtractMkvTagsThreaded,
                FormMkvExtractionMode.Timecodes => argGmkvExtract.ExtractMKVTimecodesThreaded,
                FormMkvExtractionMode.Tracks_And_Timecodes => argGmkvExtract.ExtractMKVSegmentsThreaded,
                FormMkvExtractionMode.Cues => argGmkvExtract.ExtractMKVCuesThreaded,
                FormMkvExtractionMode.Tracks_And_Cues => argGmkvExtract.ExtractMKVSegmentsThreaded,
                FormMkvExtractionMode.Tracks_And_Cues_And_Timecodes => argGmkvExtract.ExtractMKVSegmentsThreaded,
                _ => throw new Exception("Unsupported Extraction Mode!"),
            };
        }

        // For serialization only!!!
        internal gMKVJob() { }

        private string GetTracks(List<gMKVSegment> argSegmentList)
        {
            StringBuilder trackBuilder = new();
            foreach (gMKVSegment item in argSegmentList)
            {
                if (item is gMKVTrack track)
                {
                    trackBuilder.AppendFormat("[{0}:{1}]",
                        track.TrackType.ToString()[..3],
                        track.TrackID);
                }
                else if (item is gMKVAttachment attachment)
                {
                    trackBuilder.AppendFormat("[Att:{0}]",                        
                        attachment.ID);
                }
                else if (item is gMKVChapter)
                {
                    trackBuilder.AppendFormat("[{0}]", "Chap");
                }
            }
            return trackBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder retValue = new();
            switch (ExtractionMode)
            {
                case FormMkvExtractionMode.Tracks:
                    retValue.AppendFormat("Tracks {0} \r\n{1} \r\n{2}",
                        GetTracks(ParametersList.MKVSegmentsToExtract),
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Cue_Sheet:
                    retValue.AppendFormat("Cue Sheet \r\n{0} \r\n{1}'",
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Tags:
                    retValue.AppendFormat("Tags \r\n{0} \r\n{1}",
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Timecodes:
                    retValue.AppendFormat("Timecodes {0} \r\n{1} \r\n{2}",
                        GetTracks(ParametersList.MKVSegmentsToExtract),
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Tracks_And_Timecodes:
                    retValue.AppendFormat("Tracks/Timecodes {0} \r\n{1} \r\n{2}",
                        GetTracks(ParametersList.MKVSegmentsToExtract),
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Cues:
                    retValue.AppendFormat("Cues {0} \r\n{1} \r\n{2}",
                        GetTracks(ParametersList.MKVSegmentsToExtract),
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Tracks_And_Cues:
                    retValue.AppendFormat("Tracks/Cues {0} \r\n{1} \r\n{2}",
                        GetTracks(ParametersList.MKVSegmentsToExtract),
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                case FormMkvExtractionMode.Tracks_And_Cues_And_Timecodes:
                    retValue.AppendFormat("Tracks/Cues/Timecodes {0} \r\n{1} \r\n{2}",
                        GetTracks(ParametersList.MKVSegmentsToExtract),
                        Path.GetFileName(ParametersList.MKVFile), ParametersList.OutputDirectory);
                    break;
                default:
                    retValue.AppendFormat("Unknown job!!!");
                    break;
            }

            return retValue.ToString();
        }
    }
}
