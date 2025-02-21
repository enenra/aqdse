using Sandbox.ModAPI;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using System.Collections.Generic;
using VRage.ModAPI;

namespace ConnectorCheck
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ShipConnector), false, "Connector", "YourOtherConnectorSubtypes")] //List all of your subtypes here (and remove the vanilla "connector" one)
    public class Connectors : MyGameLogicComponent
    {
        private bool client;
        private bool server;
        private IMyShipConnector connector;
        private List<string> regularConnectors = new List<string>() { "Connector", "PermittedOtherConnectorSubtypeID", "PermittedOtherConnectorSubtypeID2" }; //Set up a list per compatible group
        private List<string> someOtherConnectors = new List<string>() { "PermittedOtherConnectorSubtypeID3", "PermittedOtherConnectorSubtypeID4" }; //Set up a list per compatible group
        private List<string> validSubtypes = new List<string>();
        private int disabledCount;
        private bool cooldown;

        public override void OnAddedToScene()
        {
            connector = (IMyShipConnector)Entity;
            client = !MyAPIGateway.Utilities.IsDedicated || !MyAPIGateway.Multiplayer.MultiplayerActive;
            server = MyAPIGateway.Multiplayer.IsServer || !MyAPIGateway.Multiplayer.MultiplayerActive;
            connector.AttachFinished += Connector_AttachFinished;
            connector.EnabledChanged += Connector_EnabledChanged;
            NeedsUpdate = MyEntityUpdateEnum.NONE;
            var type = connector.SlimBlock.BlockDefinition.Id.SubtypeId.ToString();
            if (type == "Connector" || type == "YourOtherConnectorSubtypes") //This loads the compatible subtypes from pre-defined lists above
                validSubtypes = regularConnectors;
            else if (type == "SomeOtherConnector")
                validSubtypes = someOtherConnectors;
        }

        private void Connector_EnabledChanged(IMyTerminalBlock obj)
        {
            if (cooldown && connector.Enabled) //Player flipped it back on
            {
                cooldown = false;
                disabledCount = 0;
            }
        }

        private void Connector_AttachFinished(IMyShipConnector obj)
        {
            if (connector.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connectable && connector.OtherConnector != null)
            {
                if (!validSubtypes.Contains(connector.OtherConnector.SlimBlock.BlockDefinition.Id.SubtypeId.ToString()))
                {
                    if (client && connector.Enabled)
                        MyAPIGateway.Utilities.ShowNotification("Enenra sez:  My fancy connector isn't compatible with that one", 2000, "Red");
                    if (server)
                        Disable();
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (!cooldown)
                return;

            if (disabledCount < 3) // Set how many 100 tick increments to stay off here
                disabledCount++;
            else
                Enable();
        }

        private void Disable()
        {
            connector.Enabled = false;
            cooldown = true;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }
        private void Enable()
        {
            cooldown = false;
            connector.Enabled = true;
            disabledCount = 0;
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void OnRemovedFromScene()
        {
            validSubtypes.Clear();
            connector.AttachFinished -= Connector_AttachFinished;
            connector.EnabledChanged -= Connector_EnabledChanged;
        }
    }
}
