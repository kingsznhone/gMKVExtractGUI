using System;
using System.Collections.Generic;
using System.Text;

namespace gMKVToolNix
{
    [Serializable]
    public class gMKVAttachment : gMKVSegment
    {
        private String _Filename;

        public String Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }

        private String _MimeType;

        public String MimeType
        {
            get { return _MimeType; }
            set { _MimeType = value; }
        }

        private String _FileSize;

        public String FileSize
        {
            get { return _FileSize; }
            set { _FileSize = value; }
        }

        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public override string ToString()
        {
            return String.Format("Attachment {0} [{1}][{2}][{3} bytes]", _ID, _Filename, _MimeType, _FileSize);
        }

        public override bool Equals(object oth)
        {
            gMKVAttachment other = oth as gMKVAttachment;
            if (oth == null)
            {
                return false;
            }
            return
                this.Filename == other.Filename
                && this.FileSize == other.FileSize
                && this.ID == other.ID
                && this.MimeType == other.MimeType
                ;
        }

        public override int GetHashCode()
        {
            return
                string.Concat(
                    this.Filename.GetHashCode()
                    , this.FileSize.GetHashCode()
                    , this.ID.GetHashCode()
                    , this.MimeType.GetHashCode()
                ).GetHashCode();
        }
    }
}
