using System;
using System.Collections.Generic;
using System.Text;

namespace gMKVToolNix
{
    [Serializable]
    public class gMKVSegmentInfo : gMKVSegment
    {
        private String _TimecodeScale;

        public String TimecodeScale
        {
            get { return _TimecodeScale; }
            set { _TimecodeScale = value; }
        }

        private String _MuxingApplication;

        public String MuxingApplication
        {
            get { return _MuxingApplication; }
            set { _MuxingApplication = value; }
        }

        private String _WritingApplication;

        public String WritingApplication
        {
            get { return _WritingApplication; }
            set { _WritingApplication = value; }
        }

        private String _Duration;

        public String Duration
        {
            get { return _Duration; }
            set { _Duration = value; }
        }

        private String _Date;

        public String Date
        {
            get { return _Date; }
            set { _Date = value; }
        }

        private String _Filename;

        /// <summary>
        /// The segment's file filename
        /// </summary>
        public String Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }

        private String _Directory;

        /// <summary>
        /// The segment's file directory
        /// </summary>
        public String Directory
        {
            get { return _Directory; }
            set { _Directory = value; }
        }

        /// <summary>
        /// Returns the segment's full file path
        /// </summary>
        public String Path
        {
            get
            {
                return System.IO.Path.Combine(Directory ?? "", Filename ?? "");
            }
        }
    }
}
