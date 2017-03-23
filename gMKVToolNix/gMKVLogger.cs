using System;
using System.Collections.Generic;
using System.Text;

namespace gMKVToolNix
{
    public delegate void LogLineAddedEventHandler(String lineAdded, DateTime actionDate);

    public static class gMKVLogger
    {
        private static StringBuilder _Log = new StringBuilder();

        public static String LogText { get { return _Log.ToString(); } }

        public static event LogLineAddedEventHandler LogLineAdded;

        public static void Clear()
        {
            _Log.Length = 0;
        }

        public static void Log(String message)
        {
            DateTime actionDate = DateTime.Now;
            String logMessage = String.Format("{0} {1}", actionDate.ToString("[yyyy-MM-dd][HH:mm:ss]"), message);
            _Log.AppendLine(logMessage);
            OnLogLineAdded(logMessage, actionDate);            
        }

        public static void OnLogLineAdded(String lineAdded, DateTime actionDate)
        {
            if (LogLineAdded != null)
                LogLineAdded(lineAdded, actionDate);
        }
    }
}
