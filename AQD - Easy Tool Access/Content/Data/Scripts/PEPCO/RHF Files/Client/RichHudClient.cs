using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using ClientData = VRage.MyTuple<string, System.Action<int, object>, System.Action, int>;
using ServerData = VRage.MyTuple<System.Action, System.Func<int, object>, int>;

namespace RichHudFramework.Client
{
    using ExtendedClientData = MyTuple<ClientData, Action<Action>, ApiMemberAccessor>;

    /// <summary>
    /// API Client for the Rich HUD Framework 
    /// </summary>
    public sealed class RichHudClient : RichHudComponentBase
    {
        public static readonly Vector4I versionID = new Vector4I(1, 2, 3, 1); // Major, Minor, Rev, Hotfix
        public const ClientSubtypes subtype = ClientSubtypes.Full;

        private const long modID = 1965654081, queueID = 1314086443;
        private const int vID = 10;

        public static bool Registered => Instance != null ? Instance.registered : false;
        private static RichHudClient Instance { get; set; }

        private readonly ExtendedClientData regMessage;
        private readonly Action InitAction, OnResetAction;

        private bool regFail, registered, inQueue;
        private Func<int, object> GetApiDataFunc;
        private Action UnregisterAction;

        private RichHudClient(string modName, Action InitCallback, Action ResetCallback) : base(false, true)
        {
            InitAction = InitCallback;
            OnResetAction = ResetCallback;

            ExceptionHandler.ModName = modName;

            if (LogIO.FileName == null || LogIO.FileName == "modLog.txt")
                LogIO.FileName = $"richHudLog.txt";

            var clientData = new ClientData(modName, MessageHandler, RemoteReset, vID);
            regMessage = new ExtendedClientData(clientData, ExceptionHandler.Run, GetOrSetMember);
        }

        /// <summary>
        /// Initialzes and registers the client with the API if it is not already registered.
        /// </summary>
        /// <param name="modName">Name of the mod as it appears in the settings menu and in diagnostics</param>
        /// <param name="InitCallback">Invoked upon successfully registering with the API.</param>
        /// <param name="ResetCallback">Invoked on client reset.</param>
        public static void Init(string modName, Action InitCallback, Action ResetCallback)
        {
            if (Instance == null)
            {
                Instance = new RichHudClient(modName, InitCallback, ResetCallback);
                Instance.RequestRegistration();

                if (!Registered && !Instance.regFail)
                {
                    Instance.EnterQueue();
                }
            }
        }

        /// <summary>
        /// Unregisters the client and resets all framework modules.
        /// </summary>
        public static void Reset()
        {
            if (Registered)
                ExceptionHandler.ReloadClients();
        }

        /// <summary>
        /// Handles registration response.
        /// </summary>
        private void MessageHandler(int typeValue, object message)
        {
            MsgTypes msgType = (MsgTypes)typeValue;

            if (!regFail)
            {
                if (!Registered)
                {
                    if ((msgType == MsgTypes.RegistrationSuccessful) && message is ServerData)
                    {
                        var data = (ServerData)message;
                        UnregisterAction = data.Item1;
                        GetApiDataFunc = data.Item2;

                        registered = true;

                        ExceptionHandler.Run(InitAction);
                        ExceptionHandler.WriteToLog($"[RHF] Successfully registered with Rich HUD Master.");
                    }
                    else if (msgType == MsgTypes.RegistrationFailed)
                    {
                        if (message is string)
                            ExceptionHandler.WriteToLog($"[RHF] Failed to register with Rich HUD Master. Message: {message as string}");
                        else
                            ExceptionHandler.WriteToLog($"[RHF] Failed to register with Rich HUD Master.");

                        regFail = true;
                    }
                }
            }
        }

        private object GetOrSetMember(object data, int memberEnum)
        {
            switch((ClientDataAccessors)memberEnum)
            {
                case ClientDataAccessors.GetVersionID:
                    return versionID;
                case ClientDataAccessors.GetSubtype:
                    return subtype;
            }

            return null;
        }

        /// <summary>
        /// Attempts to register the client with the API
        /// </summary>
        private void RequestRegistration() =>
            MyAPIUtilities.Static.SendModMessage(modID, regMessage);

        /// <summary>
        /// Enters queue to await client registration.
        /// </summary>
        private void EnterQueue() =>
            MyAPIUtilities.Static.RegisterMessageHandler(queueID, QueueHandler);

        /// <summary>
        /// Unregisters callback for framework client queue.
        /// </summary>
        private void ExitQueue() =>
            MyAPIUtilities.Static.UnregisterMessageHandler(queueID, QueueHandler);

        /// <summary>
        /// Resend registration request on queue invocation.
        /// </summary>
        private void QueueHandler(object message)
        {
            if (!(registered || regFail))
            {
                inQueue = true;
                RequestRegistration();
            }
        }

        public override void Update()
        {
            if (registered && inQueue)
            {
                ExitQueue();
                inQueue = false;
            }
        }

        public override void Close()
        {
            ExitQueue();
            Unregister();
            Instance = null;
        }

        private void RemoteReset()
        {
            ExceptionHandler.Run(() => 
            {
                if (registered)
                {
                    ExceptionHandler.ReloadClients();
                    OnResetAction();
                }
            });  
        }

        /// <summary>
        /// Unregisters client from API
        /// </summary>
        private void Unregister()
        {
            if (registered)
            {
                registered = false;
                UnregisterAction();
            }
        }

        /// <summary>
        /// Base class for types acting as modules for the API
        /// </summary>
        public abstract class ApiModule<T> : RichHudComponentBase
        {
            protected readonly ApiModuleTypes componentType;

            public ApiModule(ApiModuleTypes componentType, bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient)
            {
                if (!Registered)
                    throw new Exception("Types of ApiModule cannot be instantiated before RichHudClient is initialized.");

                this.componentType = componentType;
            }

            protected T GetApiData()
            {
                object data = Instance?.GetApiDataFunc((int)componentType);

                return (T)data;
            }
        }
    }
}