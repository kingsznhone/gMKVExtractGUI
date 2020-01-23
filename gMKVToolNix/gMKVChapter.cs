using System;
using System.Collections.Generic;
using System.Text;

namespace gMKVToolNix
{
    public enum MkvChapterTypes
    {
        XML,
        OGM,
        CUE
    }

    [Serializable]
    public class gMKVChapter : gMKVSegment
    {
        private int _ChapterCount = 0;

        public int ChapterCount
        {
            get { return _ChapterCount; }
            set { _ChapterCount = value; }
        }

        public override string ToString()
        {
            return String.Format("Chapters {0} entries", _ChapterCount);
        }

        public override bool Equals(object oth)
        {
            gMKVChapter other = oth as gMKVChapter;
            if (other == null)
            {
                return false;
            }
            return this.ChapterCount == other.ChapterCount;
        }

        public override int GetHashCode()
        {
            return this.ChapterCount.GetHashCode();
        }
    }
}
