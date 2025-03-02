using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace ConnectorCheck
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

    public partial class Session : MySessionComponentBase
    {
        private bool controlInit;
        private bool client;
        private int tick;
        public static List<string> allowedTypes = new List<string>() { "AQD_LG_AirlockConnector_Flat", "AQD_SG_AirlockConnector_Flat", "AQD_LG_AirlockConnector_Large"};
        public static IMyShipConnector displayConnector;
        private Dictionary<long, cComp> connectors = new Dictionary<long, cComp>();
        private readonly ConcurrentCachingList<IMyShipConnector> _startComps = new ConcurrentCachingList<IMyShipConnector>();

        public override void LoadData()
        {
            client = !MyAPIGateway.Utilities.IsDedicated;
            if (client)
                MyEntities.OnEntityCreate += OnEntityCreate;
        }
        protected override void UnloadData()
        {
            if (client)
                MyEntities.OnEntityCreate -= OnEntityCreate;
            connectors.Clear();
        }
        public override void UpdateBeforeSimulation()
        {
            tick++;
            if (!_startComps.IsEmpty && tick % 30 == 0)
                StartComps();
            if (client && displayConnector != null && displayConnector.OtherConnector != null && Session.Player?.Controller?.ControlledEntity is IMyCubeBlock)
            {
                var show = tick % 10 == 0 && (connectors[displayConnector.EntityId].alignment || connectors[displayConnector.OtherConnector.EntityId].alignment);
                if (show)
                {
                    var dummies = new Dictionary<string, IMyModelDummy>();
                    displayConnector.Model.GetDummies(dummies);
                    var partMatrix = displayConnector.PositionComp.WorldMatrixRef;
                    var worldDir = Vector3D.Normalize(Vector3D.TransformNormal(dummies["detector_Connector_001"].Matrix.Up, ref partMatrix)); //Forward for vanilla, up for AQD??

                    var otherDummies = new Dictionary<string, IMyModelDummy>();
                    displayConnector.OtherConnector.Model.GetDummies(otherDummies);
                    var partMatrixOth = displayConnector.OtherConnector.PositionComp.WorldMatrixRef;
                    var worldDirOth = Vector3D.Normalize(Vector3D.TransformNormal(otherDummies["detector_Connector_001"].Matrix.Up, ref partMatrixOth));

                    var angle = MathHelper.ToDegrees(Math.Acos(Vector3D.Dot(worldDir, worldDirOth)));
                    MyAPIGateway.Utilities.ShowNotification("Airlock Alignment Angle: " + angle.ToString("0.0") + "°", 160, angle > 0.5 ? "Red" : "Green");
                }
            }
        }
        internal void StartComps()
        {
            _startComps.ApplyAdditions();
            for (int i = 0; i < _startComps.Count; i++)
            {
                var connector = _startComps[i];
                var comp = new cComp() { connector = connector };
                comp.Init();
                connectors.Add(connector.EntityId, comp);
                connector.OnClose += Connector_OnClose;
            }
            _startComps.ClearImmediate();
        }
        private void OnEntityCreate(MyEntity entity)
        {
            var connector = entity as IMyShipConnector;
            if (connector != null)
            {
                if (!controlInit)
                {
                    controlInit = true;
                    CreateTerminalControls<IMyShipConnector>();
                }
                if (allowedTypes.Contains(connector.BlockDefinition.SubtypeId))
                    _startComps.Add(connector);
            }
        }

        private void Connector_OnClose(IMyEntity obj)
        {
            connectors[obj.EntityId].Close();
            connectors.Remove(obj.EntityId);
            obj.OnClose -= Connector_OnClose;
        }

        private void CreateTerminalControls<T>() where T : IMyShipConnector
        {
            var c = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, T>("ConnectorAlignmentAngle");
            c.Title = MyStringId.GetOrCompute("Show Airlock Alignment Angle");
            c.Tooltip = MyStringId.GetOrCompute("Show alignment data on screen");
            c.OnText = MyStringId.GetOrCompute("On");
            c.OffText = MyStringId.GetOrCompute("Off");
            c.Enabled = Visible;
            c.Visible = Visible;
            c.Getter = GetActivated;
            c.Setter = SetActivated;

            var action = MyAPIGateway.TerminalControls.CreateAction<T>("ConnectorAlignmentAngle");
            action.Icon = @"Textures\GUI\Icons\Actions\Toggle.dds";
            action.Name = new StringBuilder("Show Airlock Alignment Angle");
            action.Action = ActionAction;
            action.Writer = ActionWriter;
            action.Enabled = Visible;
            action.ValidForGroups = false;

            MyAPIGateway.TerminalControls.AddAction<T>(action);
            MyAPIGateway.TerminalControls.AddControl<T>(c);
        }
        public static bool Visible(IMyTerminalBlock block)
        {
            return allowedTypes.Contains(block.BlockDefinition.SubtypeId);
        }
        internal bool GetActivated(IMyTerminalBlock block)
        {
            cComp comp;
            if (connectors.TryGetValue(block.EntityId, out comp))
                return comp.alignment;
            return false;
        }
        internal void SetActivated(IMyTerminalBlock block, bool activated)
        {
            cComp comp;
            if (connectors.TryGetValue(block.EntityId, out comp))
                comp.alignment = activated;
        }
        internal void ActionWriter(IMyTerminalBlock block, StringBuilder builder)
        {
            cComp comp;
            if (connectors.TryGetValue(block.EntityId, out comp))
                builder.Append("Angle " + (comp.alignment ? "On" : "Off"));
        }
        internal void ActionAction(IMyTerminalBlock block)
        {
            cComp comp;
            if (connectors.TryGetValue(block.EntityId, out comp))
                comp.alignment = !comp.alignment;
        }
    }

    public class cComp
    {
        internal bool alignment;
        internal IMyShipConnector connector;

        internal void Init()
        {
            connector.AttachFinished += Connector_AttachFinished;
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
                Connector_AttachFinished(connector);
        }
        private void Connector_AttachFinished(IMyShipConnector obj)
        {
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                var otherSubtype = connector.OtherConnector.BlockDefinition.SubtypeId;
                if (connector.BlockDefinition.SubtypeId == otherSubtype || Session.allowedTypes.Contains(connector.BlockDefinition.SubtypeId) && Session.allowedTypes.Contains(otherSubtype))
                    Session.displayConnector = connector;
            }
        }
        internal void Close()
        {
            connector.AttachFinished -= Connector_AttachFinished;
        }
    }
}
