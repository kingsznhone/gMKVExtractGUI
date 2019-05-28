using System;
using System.Collections.Generic;
using System.Text;

namespace gMKVToolNix
{
    public enum MkvTrackType
    {
        video,
        audio,
        subtitles
    }

    [Serializable]
    public class gMKVTrack : gMKVSegment
    {
        private int _TrackNumber;

        public int TrackNumber
        {
            get { return _TrackNumber; }
            set { _TrackNumber = value; }
        }

        private int _TrackID;

        public int TrackID
        {
            get { return _TrackID; }
            set { _TrackID = value; }
        }

        private MkvTrackType _TrackType;

        public MkvTrackType TrackType
        {
            get { return _TrackType; }
            set { _TrackType = value; }
        }

        private String _CodecID;

        public String CodecID
        {
            get { return _CodecID; }
            set { _CodecID = value; }
        }

        private String _CodecPrivate = "";

        public String CodecPrivate
        {
            get { return _CodecPrivate; }
            set { _CodecPrivate = value; }
        }

        private String _CodecPrivateData = "";

        public String CodecPrivateData
        {
            get { return _CodecPrivateData; }
            set { _CodecPrivateData = value; }
        }

        private String _Language;

        public String Language
        {
            get { return _Language; }
            set { _Language = value; }
        }

        private String _TrackName;

        public String TrackName
        {
            get { return _TrackName; }
            set { _TrackName = value; }
        }

        private String _ExtraInfo;

        public String ExtraInfo
        {
            get { return _ExtraInfo; }
            set { _ExtraInfo = value; }
        }

        private Int32 _Delay = Int32.MinValue;

        public Int32 Delay
        {
            get { return _Delay; }
            set { _Delay = value; }
        }

        private Int32 _EffectiveDelay = Int32.MinValue;

        public Int32 EffectiveDelay
        {
            get { return _EffectiveDelay; }
            set { _EffectiveDelay = value; }
        }

        private Int64 _MinimumTimestamp = Int64.MinValue;

        public Int64 MinimumTimestamp // In nanoseconds
        {
            get { return _MinimumTimestamp; }
            set { _MinimumTimestamp = value; }
        }

        public Int32 VideoPixelWidth { get; set; }
        public Int32 VideoPixelHeight { get; set; }

        public Int32 AudioSamplingFrequency { get; set; }
        public Int32 AudioChannels { get; set; }

        public override string ToString()
        {
            String str = String.Format("Track {0} [TID {1}][{2}][{3}][{4}][{5}][{6}]", 
                _TrackNumber, _TrackID, Enum.GetName(typeof(MkvTrackType), _TrackType), _CodecID, _TrackName, _Language, _ExtraInfo);
            if (!String.IsNullOrEmpty(_CodecPrivate))
            {
                str = String.Format("{0}[{1}]", str, _CodecPrivate);
            }
            if (_TrackType != MkvTrackType.subtitles)
            {
                str = String.Format("{0}[{1} ms][{2} ms]", str, _Delay, _EffectiveDelay);
            }
            return str;
        }

        public override bool Equals(object oth)
        {
            gMKVTrack other = oth as gMKVTrack;
            if (oth == null)
            {
                return false;
            }
            return
                this.AudioChannels == other.AudioChannels
                && this.AudioSamplingFrequency == other.AudioSamplingFrequency
                && this.CodecID == other.CodecID
                && this.CodecPrivate == other.CodecPrivate
                && this.CodecPrivateData == other.CodecPrivateData
                && this.Delay == other.Delay
                && this.EffectiveDelay == other.EffectiveDelay
                && this.ExtraInfo == other.ExtraInfo
                && this.Language == other.Language
                && this.MinimumTimestamp == other.MinimumTimestamp
                && this.TrackID == other.TrackID
                && this.TrackName == other.TrackName
                && this.TrackNumber == other.TrackNumber
                && this.TrackType == other.TrackType
                && this.VideoPixelHeight == other.VideoPixelHeight
                && this.VideoPixelWidth == other.VideoPixelWidth
                ;
        }

        public override int GetHashCode()
        {
            return
                string.Concat(
                    this.AudioChannels.GetHashCode()
                    ,this.AudioSamplingFrequency.GetHashCode()
                    , this.CodecID.GetHashCode()
                    , this.CodecPrivate.GetHashCode()
                    , this.CodecPrivateData.GetHashCode()
                    , this.Delay.GetHashCode()
                    , this.EffectiveDelay.GetHashCode()
                    , this.ExtraInfo.GetHashCode()
                    , this.Language.GetHashCode()
                    , this.MinimumTimestamp.GetHashCode()
                    , this.TrackID.GetHashCode()
                    , this.TrackName.GetHashCode()
                    , this.TrackNumber.GetHashCode()
                    , this.TrackType.GetHashCode()
                    , this.VideoPixelHeight.GetHashCode()
                    , this.VideoPixelWidth.GetHashCode()
                ).GetHashCode();
        }
    }
}