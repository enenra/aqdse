using RichHudFramework.IO;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace RichHudFramework.Internal
{
    /// <summary>
    /// Extends <see cref="MySessionComponentBase"/> to include built-in exception handling, logging and a component
    /// system.
    /// </summary>
    public abstract partial class ModBase : MySessionComponentBase
    {
        /// <summary>
        /// Determines whether or not the main class will be allowed to run on a dedicated server.
        /// </summary>
        public bool RunOnServer { get; }

        /// <summary>
        /// If true, then the mod will be allowed to run on a client.
        /// </summary>
        public bool RunOnClient { get; }

        /// <summary>
        /// If true, the mod is currently loaded.
        /// </summary>
        public new bool Loaded { get; private set; }

        /// <summary>
        /// If true, then the session component will be allowed to update.
        /// </summary>
        public bool CanUpdate
        {
            get { return _canUpdate && ((RunOnClient && ExceptionHandler.IsClient) || (RunOnServer && ExceptionHandler.IsDedicated)); }
            set { _canUpdate = value; }
        }

        private readonly List<ModuleBase> modules;
        private bool _canUpdate, closing;

        protected ModBase(bool runOnServer, bool runOnClient)
        {
            modules = new List<ModuleBase>();
            RunOnServer = runOnServer;
            RunOnClient = runOnClient;
        }

        public sealed override void LoadData()
        {
            if (!Loaded && !ExceptionHandler.Unloading && !closing)
            {
                CanUpdate = true;
                ExceptionHandler.RegisterClient(this);

                if (CanUpdate)
                    AfterLoadData();
            }
        }

        protected new virtual void AfterLoadData() { }

        public sealed override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (!Loaded && !ExceptionHandler.Unloading && !closing)
            {
                if (CanUpdate)
                    AfterInit();

                Loaded = true;
            }
        }

        protected virtual void AfterInit() { }

        public void ManualStart()
        {
            if (!Loaded && !ExceptionHandler.Unloading && !closing)
            {
                LoadData();
                Init(null);
            }
        }

        public override void Draw()
        {
            if (Loaded && CanUpdate)
            {
                ExceptionHandler.Run(() =>
                {
                    for (int n = 0; n < modules.Count; n++)
                    {
                        bool updateClient = modules[n].runOnClient && ExceptionHandler.IsClient,
                            updateServer = modules[n].runOnServer && ExceptionHandler.IsDedicated;

                        if (updateClient || updateServer)
                            modules[n].Draw();
                    }
                });
            }
        }

        public override void HandleInput()
        {
            if (Loaded && CanUpdate)
            {
                ExceptionHandler.Run(() =>
                {
                    for (int n = 0; n < modules.Count; n++)
                    {
                        bool updateClient = modules[n].runOnClient && ExceptionHandler.IsClient,
                            updateServer = modules[n].runOnServer && ExceptionHandler.IsDedicated;

                        if (updateClient || updateServer)
                            modules[n].HandleInput();
                    }
                });
            }
        }

        public sealed override void UpdateBeforeSimulation() =>
            BeforeUpdate();

        public sealed override void Simulate() =>
            BeforeUpdate();

        public sealed override void UpdateAfterSimulation() =>
            BeforeUpdate();

        /// <summary>
        /// The update function used (Before/Sim/After) is determined by the settings used by
        /// the MySessionComponentDescriptorAttribute applied to the child class.
        /// </summary>
        protected virtual void BeforeUpdate()
        {
            if (Loaded && CanUpdate)
            {
                ExceptionHandler.Run(() =>
                {
                    for (int n = 0; n < modules.Count; n++)
                    {
                        bool updateClient = modules[n].runOnClient && ExceptionHandler.IsClient,
                            updateServer = modules[n].runOnServer && ExceptionHandler.IsDedicated;

                        if (updateClient || updateServer)
                            modules[n].Update();
                    }

                    Update();
                });
            }
        }

        /// <summary>
        /// Sim update.
        /// </summary>
        protected virtual void Update() { }

        /// <summary>
        /// Called before close used to stop, clean up and save before other components
        /// start to unload.
        /// </summary>
        public virtual void BeforeClose() { }

        /// <summary>
        /// Called for final cleanup. Other components may have already unloaded by this point.
        /// </summary>
        public virtual void Close()
        {
            if (!closing)
            {
                Loaded = false;
                CanUpdate = false;
                closing = true;

                CloseModules();
                modules.Clear();

                closing = false;
            }
        }

        private void CloseModules()
        {
            string typeName = GetType().Name;

            for (int n = modules.Count - 1; n >= 0; n--)
            {
                var module = modules[n];
                bool success = false;

                ExceptionHandler.Run(() =>
                {
                    ExceptionHandler.WriteToLog($"[{typeName}] Closing {module.GetType().Name} module...", true);
                    module.Close();
                    success = true;
                });

                if (success)
                    ExceptionHandler.WriteToLog($"[{typeName}] Closed {module.GetType().Name} module.", true);
                else
                    ExceptionHandler.WriteToLog($"[{typeName}] Failed to close {module.GetType().Name} module.");

                module.UnregisterComponent(n);
            }
        }

        protected override void UnloadData()
        { }

        /// <summary>
        /// Base class for ModBase components.
        /// </summary>
        public abstract class ModuleBase
        {
            protected ModBase Parent { get; private set; }

            /// <summary>
            /// Determines whether or not this component will run on a dedicated server and/or client.
            /// </summary>
            public readonly bool runOnServer, runOnClient;

            protected ModuleBase(bool runOnServer, bool runOnClient, ModBase parent)
            {
                this.runOnServer = runOnServer;
                this.runOnClient = runOnClient;

                RegisterComponent(parent);
            }

            public void RegisterComponent(ModBase parent)
            {
                if (Parent == null)
                {
                    parent.modules.Add(this);

                    Parent = parent;
                    ExceptionHandler.WriteToLog($"[{Parent.GetType().Name}] Registered {GetType().Name} module.", true);
                }
            }

            /// <summary>
            /// Used to manually remove object from update queue. This should only be used for objects that
            /// need to be closed while the mod is running.
            /// </summary>
            public void UnregisterComponent()
            {
                if (Parent != null)
                {
                    Parent.modules.Remove(this);

                    ExceptionHandler.WriteToLog($"[{Parent.GetType().Name}] Unregistered {GetType().Name} module.", true);
                    Parent = null;
                }
            }

            /// <summary>
            /// Used to manually remove object from update queue. This should only be used for objects that
            /// need to be closed while the mod is running.
            /// </summary>
            public void UnregisterComponent(int index)
            {
                if (Parent != null && index < Parent.modules.Count && Parent.modules[index] == this)
                {
                    Parent.modules.RemoveAt(index);

                    ExceptionHandler.WriteToLog($"[{Parent.GetType().Name}] Unregistered {GetType().Name} module.", true);
                    Parent = null;
                }
            }

            public virtual void Draw() { }

            public virtual void HandleInput() { }

            public virtual void Update() { }

            public virtual void Close() { }
        }

        /// <summary>
        /// Extension of <see cref="ModuleBase"/> that includes a task pool.
        /// </summary>
        public abstract class ParallelModuleBase : ModuleBase
        {
            private readonly TaskPool taskPool;

            protected ParallelModuleBase(bool runOnServer, bool runOnClient, ModBase parent) : base(runOnServer, runOnClient, parent)
            {
                taskPool = new TaskPool(ErrorCallback);
            }

            /// <summary>
            /// Called in the event an exception occurs in one of the component's tasks with a list of <see cref="KnownException"/>s
            /// and a single aggregate exception of all other exceptions.
            /// </summary>
            protected virtual void ErrorCallback(List<KnownException> knownExceptions, AggregateException aggregate)
            {
                if (knownExceptions.Count > 0)
                    ExceptionHandler.ReportException(new AggregateException(knownExceptions));

                if (aggregate != null)
                    ExceptionHandler.ReportException(aggregate);
            }

            /// <summary>
            /// Enqueues an action to run in parallel. Not thread safe; must be called from the main thread.
            /// </summary>
            protected void EnqueueTask(Action action) =>
                taskPool.EnqueueTask(action);

            /// <summary>
            /// Enqueues an action to run on the main thread. Meant to be used by threads other than the main.
            /// </summary>
            protected void EnqueueAction(Action action) =>
                taskPool.EnqueueAction(action);
        }
    }
}
