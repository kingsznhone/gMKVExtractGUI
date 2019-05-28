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

        public override bool Equals(object oth)
        {
            gMKVSegmentInfo other = oth as gMKVSegmentInfo;
            if (oth == null)
            {
                return false;
            }
            return
                  this.Date == other.Date
                && this.Directory == other.Directory
                && this.Duration == other.Duration
                && this.Filename == other.Filename
                && this.MuxingApplication == other.MuxingApplication
                && this.Path == other.Path
                && this.TimecodeScale == other.TimecodeScale
                && this.WritingApplication == other.WritingApplication
                ;
        }

        public override int GetHashCode()
        {
            return
                string.Concat(
                    this.Date.GetHashCode()
                    , this.Directory.GetHashCode()
                    , this.Duration.GetHashCode()
                    , this.Filename.GetHashCode()
                    , this.MuxingApplication.GetHashCode()
                    , this.Path.GetHashCode()
                    , this.TimecodeScale.GetHashCode()
                    , this.WritingApplication.GetHashCode()
                ).GetHashCode();
        }
    }
}
