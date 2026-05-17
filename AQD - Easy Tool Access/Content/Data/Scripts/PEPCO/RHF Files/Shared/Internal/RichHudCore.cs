using Sandbox.ModAPI;
using System;
using System.Diagnostics;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using RichHudFramework.UI;

namespace RichHudFramework.Internal
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public sealed class RichHudCore : ModBase
    {
        public static RichHudCore Instance { get; private set; }

        /// <summary>
        /// Chat input event regestered later that the rest
        /// </summary>
        public static event MessageEnteredDel LateMessageEntered;

        private readonly Stopwatch handlerRegTimer;

        public RichHudCore() : base(false, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of RichHudCore can exist at any given time.");

            handlerRegTimer = new Stopwatch();
        }

        public override void BeforeStart()
        {
            handlerRegTimer.Start();
        }

        private void MessageHandler(string message, ref bool sendToOthers)
        {
            LateMessageEntered?.Invoke(message, ref sendToOthers);
        }

        public override void Draw()
        {
            // It seems there's some kind of bug in the game's session component system that prevents the Before/Sim/After
            // update methods from being called on more than one component with the same fully qualified name, update order
            // and priority, but for some reason, Draw and HandleInput still work.
            //
            // It would be really nice if I didn't have to work around this issue like this, but here we are.         
            BeforeUpdate();
            base.Draw();

            // Because some people are just bad neighbors
            if (handlerRegTimer.IsRunning && handlerRegTimer.ElapsedMilliseconds > 10000)
            {
                MyAPIGateway.Utilities.MessageEntered += MessageHandler;
                handlerRegTimer.Stop();
            }
        }

        public override void Close()
        {
            base.Close();

            if (ExceptionHandler.Unloading)
            {
                MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
                Instance = null;
            }
        }

        protected override void UnloadData()
        {
            LateMessageEntered = null;
        }
    }

    public abstract class RichHudComponentBase : ModBase.ModuleBase
    {
        public RichHudComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, RichHudCore.Instance)
        { }
    }

    public abstract class RichHudParallelComponentBase : ModBase.ParallelModuleBase
    {
        public RichHudParallelComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, RichHudCore.Instance)
        { }
    }
}