using Sandbox.ModAPI;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.Game;
using System.Collections.Generic;

namespace ConnectorCheck
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ShipConnector), false, "AQD_LG_AirlockConnector_Flat", "AQD_SG_AirlockConnector_Flat", "AQD_LG_AirlockConnector_Large")]
    public class Connectors : MyGameLogicComponent
    {
        private IMyShipConnector connector;
        private int disabledCount;
        private bool cooldown;
        public static List<string> allowedTypes = new List<string>() { "AQD_LG_AirlockConnector_Flat", "AQD_SG_AirlockConnector_Flat", "GFA_LG_TIEFighter_DockingTube", "GFA_SG_TIEFighter_Hatch" };

        public override void OnAddedToScene()
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;
            connector = (IMyShipConnector)Entity;            
            connector.AttachFinished += Connector_AttachFinished;
            connector.EnabledChanged += Connector_EnabledChanged;
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
                Connector_AttachFinished(connector);
        }

        public override void UpdateBeforeSimulation100()
        {
            if (disabledCount++ >= 3) Cycle(true);
        }

        private void Connector_EnabledChanged(IMyTerminalBlock obj)
        {
            if (cooldown && connector.Enabled) Cycle(true); //Player cycled it back on
        }

        private void Connector_AttachFinished(IMyShipConnector obj)
        {
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                var otherSubtype = connector.OtherConnector.BlockDefinition.SubtypeId;
                if (!(connector.BlockDefinition.SubtypeId == otherSubtype || allowedTypes.Contains(connector.BlockDefinition.SubtypeId) && allowedTypes.Contains(otherSubtype)))
                {
                    var ownGridCtrlEnt = connector.CubeGrid.ControlSystem?.CurrentShipController?.ControllerInfo?.ControllingIdentityId;
                    var otherGridCtrlEnt = connector.OtherConnector.CubeGrid.ControlSystem?.CurrentShipController?.ControllerInfo?.ControllingIdentityId;
                    if (ownGridCtrlEnt != null)
                        MyVisualScriptLogicProvider.ShowNotification("Connector Incompatible", 2000, "Red", (long)ownGridCtrlEnt);
                    if (otherGridCtrlEnt != null)
                        MyVisualScriptLogicProvider.ShowNotification("Connector Incompatible", 2000, "Red", (long)otherGridCtrlEnt);
                    Cycle(false);
                }
            }
        }

        private void Cycle(bool pwr)
        {
            cooldown = !pwr;
            connector.Enabled = pwr;
            NeedsUpdate = pwr ? MyEntityUpdateEnum.NONE : MyEntityUpdateEnum.EACH_100TH_FRAME;
            disabledCount = 0;
        }
    }
}

