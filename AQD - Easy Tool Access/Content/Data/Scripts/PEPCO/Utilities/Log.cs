using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ParallelTasks;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PEPCO.Utilities
{
    // FIXME: errors that occur in loading (or before chat is visible?) are not seen!
    // TODO: make use of MyDefinitionErrors ?

    /// <summary>
    /// <para>Standalone logger, does not require any setup.</para>
    /// <para>Mod name is automatically set from workshop name or folder name. Can also be manually defined using <see cref="ModName"/>.</para>
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: int.MaxValue)]
    public class Log : MySessionComponentBase
    {
        static Handler handler;
        static bool unloaded = false;
        static long dateStarted;

        public static string FILE = ModParameter.MODNAME + ".log";
        const int DEFAULT_TIME_INFO = 3000;
        const int DEFAULT_TIME_ERROR = 10000;

        int MainThreadId;

        /// <summary>
        /// Print the generic error info.
        /// (For use in <see cref="Log.Error(string, string, int)"/>'s 2nd arg)
        /// </summary>
        public const string PRINT_GENERIC_ERROR = "<err>";

        /// <summary>
        /// Prints the message instead of the generic generated error info.
        /// (For use in <see cref="Log.Error(string, string, int)"/>'s 2nd arg)
        /// </summary>
        public const string PRINT_MESSAGE = "<msg>";

        #region Handling of handler
        public override void LoadData()
        {
            MainThreadId = Environment.CurrentManagedThreadId;
            EnsureHandlerCreated();
            handler.Init(this);

            if (MainThreadId != 1)
                Info($"WARNING: Main thread is not thread 1, but thread {MainThreadId}");
        }

        protected override void UnloadData()
        {
            try
            {
                if (handler != null && handler.AutoClose)
                {
                    Unload();
                }
            }
            catch (Exception e)
            {
                MyLog.Default.Error(e.ToString());
            }
        }

        static void Unload()
        {
            if (!unloaded)
            {
                unloaded = true;
                handler?.Close();
            }

            handler = null;
        }

        static void EnsureHandlerCreated(string intendedMessage = null)
        {
            if (unloaded)
                throw new Exception($"{typeof(Log).FullName} accessed after it was unloaded! Date started: {new DateTime(dateStarted).ToString()}."
                                    + (intendedMessage != null ? $"\nThe intended message: \"{intendedMessage}\"" : ""));

            if (handler == null)
            {
                handler = new Handler();
                dateStarted = DateTime.Now.Ticks;
            }
        }
        #endregion Handling of handler

        #region Publicly accessible properties and methods
        /// <summary>
        /// Manually unload the logger. Works regardless of <see cref="AutoClose"/>, but if that property is false then this method must be called!
        /// </summary>
        public static void Close()
        {
            Unload();
        }

        /// <summary>
        /// Defines if the component self-unloads next tick or after <see cref="UNLOAD_TIMEOUT_MS"/>.
        /// <para>If set to false, you must call <see cref="Close"/> manually!</para>
        /// </summary>
        public static bool AutoClose
        {
            get
            {
                EnsureHandlerCreated();
                return handler.AutoClose;
            }
            set
            {
                EnsureHandlerCreated();
                handler.AutoClose = value;
            }
        }

        /// <summary>
        /// Sets/gets the mod name.
        /// <para>This is optional as the mod name is generated from the folder/workshop name, but those can be weird or long.</para>
        /// </summary>
        public static string ModName
        {
            get
            {
                EnsureHandlerCreated();
                return handler.ModName;
            }
            set
            {
                EnsureHandlerCreated();
                handler.ModName = value;
            }
        }

        /// <summary>
        /// Gets the workshop id of the mod.
        /// <para>Will return 0 if it's a local mod or if it's called before LoadData() executes on the logger.</para>
        /// </summary>
        public static ulong WorkshopId => handler?.WorkshopId ?? 0;

        /// <summary>
        /// <para>Increases indentation by 4 spaces.</para>
        /// Each indent adds 4 space characters before each of the future messages.
        /// </summary>
        public static void IncreaseIndent()
        {
            EnsureHandlerCreated();
            handler.IncreaseIndent();
        }

        /// <summary>
        /// <para>Decreases indentation by 4 space characters down to 0 indentation.</para>
        /// See <seealso cref="IncreaseIndent"/>
        /// </summary>
        public static void DecreaseIndent()
        {
            EnsureHandlerCreated();
            handler.DecreaseIndent();
        }

        /// <summary>
        /// <para>Resets the indentation to 0.</para>
        /// See <seealso cref="IncreaseIndent"/>
        /// </summary>
        public static void ResetIndent()
        {
            EnsureHandlerCreated();
            handler.ResetIndent();
        }

        /// <summary>
        /// Writes an exception to custom log file, game's log file and by default writes a generic error message to player's HUD.
        /// </summary>
        /// <param name="exception">The exception to write to custom log and game's log.</param>
        /// <param name="printText">HUD notification text, can be set to null to disable, to <see cref="PRINT_MESSAGE"/> to use the exception message, <see cref="PRINT_GENERIC_ERROR"/> to use the predefined error message, or any other custom string.</param>
        /// <param name="printTimeMs">How long to show the HUD notification for, in miliseconds.</param>
        public static void Error(Exception exception, string printText = PRINT_GENERIC_ERROR, int printTimeMs = DEFAULT_TIME_ERROR)
        {
            EnsureHandlerCreated(exception.ToString());
            handler.Error(exception.ToString(), printText, printTimeMs);
        }

        /// <summary>
        /// Writes a message to custom log file, game's log file and by default writes a generic error message to player's HUD.
        /// </summary>
        /// <param name="message">The message printed to custom log and game log.</param>
        /// <param name="printText">HUD notification text, can be set to null to disable, to <see cref="PRINT_MESSAGE"/> to use the message arg, <see cref="PRINT_GENERIC_ERROR"/> to use the predefined error message, or any other custom string.</param>
        /// <param name="printTimeMs">How long to show the HUD notification for, in miliseconds.</param>
        public static void Error(string message, string printText = PRINT_MESSAGE, int printTimeMs = DEFAULT_TIME_ERROR)
        {
            EnsureHandlerCreated(message);
            handler.Error(message, printText, printTimeMs);
        }

        /// <summary>
        /// Writes a message in the custom log file.
        /// <para>Optionally prints a different message (or same message) in player's HUD.</para>
        /// </summary>
        /// <param name="message">The text that's written to log.</param>
        /// <param name="printText">HUD notification text, can be set to null to disable, to <see cref="PRINT_MESSAGE"/> to use the message arg or any other custom string.</param>
        /// <param name="printTimeMs">How long to show the HUD notification for, in miliseconds.</param>
        public static void Info(string message, string printText = null, int printTimeMs = DEFAULT_TIME_INFO)
        {
            // FIXME: using printText before update starts would make player miss the text

            EnsureHandlerCreated(message);
            handler.Info(message, printText, printTimeMs);
        }

        /// <summary>
        /// Iterates task errors and reports them, returns true if any errors were found.
        /// </summary>
        /// <param name="task">The task to check for errors.</param>
        /// <param name="taskName">Used in the reports.</param>
        /// <returns>true if errors found, false otherwise.</returns>
        public static bool TaskHasErrors(Task task, string taskName)
        {
            EnsureHandlerCreated();

            if (task.Exceptions != null && task.Exceptions.Length > 0)
            {
                foreach (Exception e in task.Exceptions)
                {
                    Error($"Error in {taskName} thread!\n{e}");
                }

                return true;
            }

            return false;
        }
        #endregion Publicly accessible properties and methods

        class Handler
        {
            Log sessionComp;
            string modName = string.Empty;

            TextWriter writer;
            int indent = 0;
            string errorPrintText;
            bool sessionReady = false;

            //double chatMessageCooldown;

            IMyHudNotification notifyInfo;
            IMyHudNotification notifyError;

            StringBuilder sb = new StringBuilder(256);

            List<string> preInitMessages;
            bool preInitErrors = false;

            public bool AutoClose { get; set; } = true;

            public ulong WorkshopId { get; private set; } = 0;

            public string ModName
            {
                get { return modName; }
                set
                {
                    modName = value;
                    ComputeErrorPrintText();
                }
            }

            public Handler()
            {
            }

            public void Init(Log sessionComp)
            {
                if (writer != null)
                    return; // already initialized

                if (MyAPIGateway.Utilities == null)
                    throw new Exception("MyAPIGateway.Utilities is NULL !");

                try
                {
                    writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(FILE);
                }
                catch (Exception e)
                {
                    if (e.GetType().Name == "IOException")
                        MyLog.Default.WriteLine($"ERROR {ModName} mod: couldn't hook its log file for writing, possible causes explained at: https://spaceengineers.wiki.gg/wiki/Modding/Reference/Known_Solutions_to_crashes_or_errors#Error_during_loading_session_/_IOException,_cannot_access_the_file,_used_by_another_process");

                    throw;
                }

                MyAPIGateway.Session.OnSessionReady += OnSessionReady;

                this.sessionComp = sessionComp;

                if (string.IsNullOrWhiteSpace(ModName))
                    ModName = sessionComp.ModContext.ModName;

                WorkshopId = sessionComp.ModContext.ModItem.PublishedFileId;

                ShowPreInitMessages();
                InitMessage();
            }

            public void Close()
            {
                if (writer != null)
                {
                    try
                    {
                        Info("Unloaded.");
                        writer.Flush();
                    }
                    finally
                    {
                        writer.Close();
                        writer = null;
                    }
                }

                sessionReady = false;
                MyAPIGateway.Session.OnSessionReady -= OnSessionReady;
            }

            void OnSessionReady()
            {
                sessionReady = true;
            }

            void ShowPreInitMessages()
            {
                if (preInitMessages == null)
                    return;

                if (preInitErrors)
                    Error($"Got errors occurred during loading:", PRINT_GENERIC_ERROR, 10000);
                else
                    Info($"Got log messages during loading:", PRINT_GENERIC_ERROR, 10000);

                Info($"--- pre-init messages ---");

                foreach (string msg in preInitMessages)
                {
                    Info(msg);
                }

                Info("--- end pre-init messages ---");

                preInitMessages = null;
            }

            void InitMessage()
            {
                MyObjectBuilder_SessionSettings worldsettings = MyAPIGateway.Session.SessionSettings;

                sb.Clear();
                sb.Append("Initialized; ").Append(DateTime.Now.ToString("yyyy MMMM dd (dddd) HH:mm:ss")).Append("; GameVersion=").Append(MyAPIGateway.Session.Version.ToString());
                sb.Append("\nModWorkshopId=").Append(WorkshopId).Append("; ModName=").Append(modName).Append("; ModService=").Append(sessionComp.ModContext.ModServiceName);
                sb.Append("\nGameMode=").Append(worldsettings.GameMode.ToString()).Append("; OnlineMode=").Append(worldsettings.OnlineMode.ToString());
                sb.Append("\nServer=").Append(MyAPIGateway.Session.IsServer).Append("; DS=").Append(MyAPIGateway.Utilities.IsDedicated);
                sb.Append("\nVERSION=").Append(ModParameter.PEPCOVERSION).Append("; DEV=").Append(ModName.EndsWith("- DEV"));
                Info(sb.ToString());
                sb.Clear();
            }

            void ComputeErrorPrintText()
            {
                errorPrintText = $"report contents of: %AppData%/SpaceEngineers/Storage/{MyAPIGateway.Utilities.GamePaths.ModScopeName}/{FILE}";
            }

            public void IncreaseIndent()
            {
                Interlocked.Increment(ref indent);
            }

            public void DecreaseIndent()
            {
                if (indent > 0)
                {
                    Interlocked.Decrement(ref indent);
                }
            }

            public void ResetIndent()
            {
                indent = 0;
            }

            // slow way to do this but I need it to be reliable
            FastResourceLock Lock = new FastResourceLock();

            public void Error(string message, string printText = PRINT_GENERIC_ERROR, int printTime = DEFAULT_TIME_ERROR)
            {
                using (Lock.AcquireExclusiveUsing())
                {
                    MyLog.Default.WriteLineAndConsole($"{modName} error/exception: {message}"); // write to game's log

                    LogMessage(message, "ERROR: "); // write to custom log

                    if (printText != null) // printing to HUD is optional
                        ShowHudMessage(ref notifyError, message, printText, printTime, MyFontEnum.Red);
                }
            }

            public void Info(string message, string printText = null, int printTime = DEFAULT_TIME_INFO)
            {
                using (Lock.AcquireExclusiveUsing())
                {
                    LogMessage(message); // write to custom log

                    if (printText != null) // printing to HUD is optional
                        ShowHudMessage(ref notifyInfo, message, printText, printTime, MyFontEnum.Debug);
                }
            }

            void ShowHudMessage(ref IMyHudNotification notify, string message, string printText, int printTime, string font)
            {
                try
                {
                    if (!sessionReady || printText == null || MyAPIGateway.Utilities == null || MyAPIGateway.Utilities.IsDedicated)
                        return;

                    if (MyParticlesManager.Paused)
                        return; // HACK: fix for notifications breaking

                    if (printText == PRINT_GENERIC_ERROR)
                    {
                        printText = $"[{modName} ERROR: {errorPrintText}]";
                    }
                    else if (printText == PRINT_MESSAGE)
                    {
                        if (font == MyFontEnum.Red)
                            printText = $"[{modName} ERROR: {message}]";
                        else
                            printText = $"[{modName} WARNING: {message}]";
                    }

                    if (notify == null)
                    {
                        notify = MyAPIGateway.Utilities.CreateNotification(printText, printTime, font);
                    }
                    else
                    {
                        notify.Hide(); // required since SE v1.194
                        notify.Text = printText;
                        notify.AliveTime = printTime;
                        notify.ResetAliveTime();
                    }

                    notify.Show();
                }
                catch (Exception e)
                {
                    Info("ERROR: Could not send notification to local client: " + e);
                    MyLog.Default.WriteLineAndConsole($"{modName} :: LOGGER error/exception: Could not send notification to local client: {e}");
                }
            }

            void LogMessage(string message, string prefix = null)
            {
                try
                {
                    sb.Clear();
                    sb.Append(DateTime.Now.ToString("[HH:mm:ss/")).Append(((MyAPIGateway.Session?.GameplayFrameCounter ?? 0) % 60).ToString("00"));

                    int threadId = Environment.CurrentManagedThreadId;
                    if (sessionComp == null ? (threadId != 1) : (sessionComp.MainThreadId != threadId))
                        sb.Append("|Thr").Append(threadId);

                    if (writer == null)
                        sb.Append("|PRE-INIT");

                    sb.Append("] ");

                    for (int i = 0; i < indent; i++)
                        sb.Append(' ', 4);

                    if (prefix != null)
                        sb.Append(prefix);

                    sb.Append(message);

                    if (writer == null)
                    {
                        if (preInitMessages == null)
                            preInitMessages = new List<string>(2);

                        preInitMessages.Add(sb.ToString());

                        if (!preInitErrors && prefix != null && prefix.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) != -1)
                            preInitErrors = true;
                    }
                    else
                    {
                        writer.WriteLine(sb);
                        writer.Flush();
                    }

                    sb.Clear();
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"{modName} :: LOGGER error/exception while logging: '{message}'\nLogger error: {e}");
                }
            }
        }
    }
}