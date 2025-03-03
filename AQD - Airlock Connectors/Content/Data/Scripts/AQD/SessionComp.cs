using Sandbox.ModAPI;

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
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
                Connector_PropertiesChanged(connector);            
        }

        private void Connector_PropertiesChanged(IMyTerminalBlock obj)
        {
            if (!wasConnectable && connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                var otherSubtype = connector.OtherConnector.BlockDefinition.SubtypeId;
                if (connector.BlockDefinition.SubtypeId == otherSubtype || (Session.allowedTypes.Contains(connector.BlockDefinition.SubtypeId) && Session.allowedTypes.Contains(otherSubtype)))
                {
                    wasConnectable = true;
                    if (alignment && Session.controlledGrid != null && Session.controlledGrid == connector.CubeGrid)
                        Session.displayConnector = connector;
                }
            }
            else if (wasConnectable && connector.Status != Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                wasConnectable = false;
                if (Session.displayConnector == connector)
                    Session.displayConnector = null;
            }
        }

        public void Update(bool show)
        {
            alignment = show;
            if (wasConnectable && Session.controlledGrid != null && Session.controlledGrid == connector.CubeGrid)
                Session.displayConnector = connector;
            if (!alignment && Session.displayConnector == connector)
                Session.displayConnector = null;
        }

        internal void Close()
        {
            connector.IsConnectedChanged -= Connector_PropertiesChanged;
        }
    }
}
