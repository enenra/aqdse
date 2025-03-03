using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace ConnectorCheck
{   
    public class cComp
    {
        internal bool alignment;
        internal bool wasConnectable;
        internal IMyShipConnector connector;

        internal void Init()
        {
            connector.PropertiesChanged += Connector_PropertiesChanged;
            connector.IsConnectedChanged += Connector_IsConnectedChanged;
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
                Connector_PropertiesChanged(connector);
        }

        private void Connector_PropertiesChanged(IMyTerminalBlock obj)
        {
            if (!wasConnectable && connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                wasConnectable = true;
                var otherSubtype = connector.OtherConnector.BlockDefinition.SubtypeId;
                if (connector.BlockDefinition.SubtypeId == otherSubtype || Session.allowedTypes.Contains(connector.BlockDefinition.SubtypeId) && Session.allowedTypes.Contains(otherSubtype))
                {
                    if (!Session.displayables.Contains(connector))
                        Session.displayables.Add(connector);
                    if (Session.controlledGrid != null && (Session.controlledGrid == connector.CubeGrid || Session.controlledGrid == connector.OtherConnector.CubeGrid))
                        Session.displayConnector = connector;
                }
            }
            else if (wasConnectable && connector.Status != Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
                wasConnectable = false;
        }

        private void Connector_IsConnectedChanged(IMyShipConnector connector)
        {
            if (connector.Status != Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                Session.displayables.Remove(connector);
                if (Session.displayConnector == connector)
                    Session.displayConnector = null;
            }
        }

        internal void Close()
        {
            connector.IsConnectedChanged -= Connector_IsConnectedChanged;
        }
    }
}
