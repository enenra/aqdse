using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace ConnectorCheck
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

    public partial class Session : MySessionComponentBase
    {
        private bool controlInit;
        private bool client;
        private int tick = -500;
        public static List<string> allowedTypes = new List<string>() { "AQD_LG_AirlockConnector_Flat", "AQD_SG_AirlockConnector_Flat", "AQD_LG_AirlockConnector_Large" };
        public static IMyShipConnector displayConnector;
        private Dictionary<long, cComp> connectors = new Dictionary<long, cComp>();
        public static List<IMyShipConnector> displayables = new List<IMyShipConnector>();
        private readonly ConcurrentCachingList<IMyShipConnector> _startComps = new ConcurrentCachingList<IMyShipConnector>();
        private bool registeredController;
        public static MyCubeGrid controlledGrid = null;

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
            if (registeredController)
                MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= GridChange;
            displayables.Clear();
            connectors.Clear();
        }
        public override void UpdateBeforeSimulation()
        {
            if (!client)
                return;
            tick++;
            if (!registeredController)
            {
                try
                {
                    MyAPIGateway.Session.Player.Controller.ControlledEntityChanged += GridChange;
                    GridChange(null, Session.Player.Controller.ControlledEntity);
                    registeredController = true;
                }
                catch { };
            }
            if (!_startComps.IsEmpty && tick % 30 == 0)
                StartComps();
            if (displayConnector != null)
            {
                if(displayConnector.OtherConnector == null)
                {
                    displayConnector = null;
                    displayables.Remove(displayConnector);
                    return;
                }

                var show = tick % 10 == 0 && (connectors[displayConnector.EntityId].alignment || connectors[displayConnector.OtherConnector.EntityId].alignment);
                if (show)
                {
                    var dummies = new Dictionary<string, IMyModelDummy>();
                    displayConnector.Model.GetDummies(dummies);
                    var partMatrix = displayConnector.PositionComp.WorldMatrixRef;
                    var worldDir = Vector3D.Normalize(Vector3D.TransformNormal(dummies["detector_Connector_001"].Matrix.Forward, ref partMatrix)); //Forward for vanilla, up for AQD??

                    var otherDummies = new Dictionary<string, IMyModelDummy>();
                    displayConnector.OtherConnector.Model.GetDummies(otherDummies);
                    var partMatrixOth = displayConnector.OtherConnector.PositionComp.WorldMatrixRef;
                    var worldDirOth = Vector3D.Normalize(Vector3D.TransformNormal(otherDummies["detector_Connector_001"].Matrix.Forward, ref partMatrixOth));

                    var angle = MathHelper.ToDegrees(Math.Acos(Vector3D.Dot(worldDir, worldDirOth)));
                    MyAPIGateway.Utilities.ShowNotification("Airlock Alignment Angle: " + angle.ToString("0.0") + "°", 160, angle > 0.5 ? "Red" : "Green");
                }
            }
        }
    }
}
