using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using static PEPCO.ScriptHelpers;

namespace PEPCO.Utilities
{
    public class ModParameter
    {
        public static readonly string MODNAME = "Easy Tool Swap";

        /// <summary>
        /// Human-readable mod version string.
        /// Often replaced/injected by a build script; keep stable formatting for tooling.
        /// </summary>
        public static readonly string PEPCOVERSION = "1775818624";

        public static bool IsDebug()
        {
            // Safe access - even if PEPCOMOD fails to initialize, this won't crash
            try
            {
                return PEPCOMOD.ISDEBUG;
            }
            catch
            {
                return false;
            }
        }

    }

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCOMOD : MySessionComponentBase
    {
        public static PEPCOMOD Instance; // changed to static
        public static bool ISDEBUG = false;

        // more stuff for debugging and mod context

        /// <summary>
        /// True when running on the authoritative server (includes dedicated server and host in SP/MP).
        /// </summary>
        public bool ISSERVER { get; private set; }

        /// <summary>
        /// True when there is a local game client (i.e., not a dedicated server process).
        /// </summary>
        public bool ISCLIENT { get; private set; }

        /// <summary>
        /// True when running as a dedicated server without any local client ("headless").
        /// </summary>
        public bool ISHEADLESS { get; private set; }

        public override void LoadData()
        {
            try
            {
                // Expose this instance globally for other mod classes.
                Instance = this; // important for accessing this object from other classes

                if (ModContext != null && !string.IsNullOrEmpty(ModContext.ModName))
                {
                    ISDEBUG = ModContext.ModName.EndsWith("- DEV");
                }
                else
                {
                    ISDEBUG = false;
                    MyLog.Default.WriteLineAndConsole($"{ModParameter.MODNAME}: Error in ModContext or ModName was null");
                }

                // Determine current runtime context.
                ISSERVER = MyAPIGateway.Multiplayer.IsServer;
            }
            catch (Exception ex)
            {
                // Prevent exceptions from crashing the mod
                ISDEBUG = false;
                // Optionally log the exception if you have a logging system
                MyLog.Default.WriteLineAndConsole($"{ModParameter.MODNAME}: Error in LoadData: {ex}");
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            ISCLIENT = !MyAPIGateway.Utilities.IsDedicated;               // true for SP and client instances, false for DS
            ISHEADLESS = MyAPIGateway.Multiplayer.IsServer
                       && MyAPIGateway.Utilities.IsDedicated;              // dedicated server without local client

            // Consider the mod to be in DEV mode if its name ends with "- DEV".
            ISDEBUG = ModContext.ModName.EndsWith("- DEV");

        }

        protected override void UnloadData()
        {
            try
            {
                Instance = null;
            }
            catch
            {
                // Prevent exceptions during cleanup
            }
        }

    }
}
