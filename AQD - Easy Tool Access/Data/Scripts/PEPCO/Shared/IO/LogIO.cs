using System;
using System.Collections.Generic;
using RichHudFramework.Internal;

namespace RichHudFramework.IO
{
    /// <summary>
    /// Handles logging to separate file in local storage
    /// </summary>
    public sealed class LogIO : RichHudParallelComponentBase
    {
        public static bool Accessible => Instance.accessible;
        public static string FileName 
        { 
            get { return _fileName; } 
            set 
            {
                if (value != _fileName)
                    Instance.logFile = new LocalFileIO(value);

                _fileName = value;
            }
        }

        private static LogIO Instance
        { 
            get 
            {
                if (_instance == null)
                    _instance = new LogIO();
                else if (_instance.Parent == null && RichHudCore.Instance != null)
                    _instance.RegisterComponent(RichHudCore.Instance);

                return _instance; 
            } 
            set { _instance = value; }
        }

        private static LogIO _instance;
        private static string _fileName;

        public bool accessible;
        private LocalFileIO logFile;

        private LogIO() : base(true, true)
        {
            accessible = true;
            _fileName = "modLog.txt";
            logFile = new LocalFileIO(_fileName);
        }

        protected override void ErrorCallback(List<KnownException> known, AggregateException unknown)
        {
            if ((known != null && known.Count > 0) || unknown != null)
            {
                WriteToLogFinish(false);

                if (known != null && known.Count > 0)
                    foreach (Exception e in known)
                        ExceptionHandler.SendChatMessage(e.Message);

                if (unknown != null)
                    throw unknown;
            }
        }

        public static bool TryWriteToLog(string message) =>
            Instance.TryWriteToLogInternal(message);

        public static void WriteToLogStart(string message) =>
            Instance.WriteToLogStartInternal(message);

        /// <summary>
        /// Attempts to synchronously update log with message and adds a time stamp.
        /// </summary>
        public bool TryWriteToLogInternal(string message)
        {
            if (accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";
                KnownException exception = logFile.TryAppend(message);

                if (exception != null)
                {
                    ExceptionHandler.SendChatMessage("Unable to update log; please check your file access permissions.");
                    accessible = false;
                    throw exception;
                }
                else
                {
                    ExceptionHandler.SendChatMessage("Log updated.");
                    accessible = true;
                    return true;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Attempts to update log in parallel with message and adds a time stamp.
        /// </summary>
        public void WriteToLogStartInternal(string message)
        {
            if (accessible)
            {
                message = $"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ms")}] {message}";

                EnqueueTask(() =>
                {
                    KnownException exception = logFile.TryAppend(message);

                    if (exception != null)
                    {
                        EnqueueAction(() => WriteToLogFinish(false));
                        throw exception;
                    }
                    else
                        EnqueueAction(() => WriteToLogFinish(true));
                });
            }
        }

        private void WriteToLogFinish(bool success)
        {
            if (!success)
            {
                if (accessible)
                    ExceptionHandler.SendChatMessage("Unable to update log; please check your file access permissions.");

                accessible = false;
            }
            else
            {
                if (accessible)
                    ExceptionHandler.SendChatMessage("Log updated.");

                accessible = true;
            }
        }
    }
}