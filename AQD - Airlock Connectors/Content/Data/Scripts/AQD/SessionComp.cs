using Sandbox.ModAPI;

namespace ConnectorCheck
{   
    public class cComp
    {
        internal bool alignment;
        internal IMyShipConnector connector;

        internal void Init()
        {
            connector.AttachFinished += Connector_AttachFinished;
            connector.IsConnectedChanged += Connector_IsConnectedChanged;
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
                Connector_AttachFinished(connector);
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

        private void Connector_AttachFinished(IMyShipConnector obj)
        {
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable)
            {
                var otherSubtype = connector.OtherConnector.BlockDefinition.SubtypeId;
                if (connector.BlockDefinition.SubtypeId == otherSubtype || Session.allowedTypes.Contains(connector.BlockDefinition.SubtypeId) && Session.allowedTypes.Contains(otherSubtype))
                {
                    if (!Session.displayables.Contains(connector))
                        Session.displayables.Add(connector);
                    if (Session.controlledGrid != null && (Session.controlledGrid == connector.CubeGrid || Session.controlledGrid == connector.OtherConnector.CubeGrid))
                        Session.displayConnector = connector;
                }
            }
        }
        internal void Close()
        {
            connector.AttachFinished -= Connector_AttachFinished;
            connector.IsConnectedChanged -= Connector_IsConnectedChanged;
        }
    }
}
