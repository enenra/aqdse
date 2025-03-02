using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace ConnectorCheck
{
    public partial class Session : MySessionComponentBase
    {
        private void GridChange(VRage.Game.ModAPI.Interfaces.IMyControllableEntity entity1, VRage.Game.ModAPI.Interfaces.IMyControllableEntity newEnt)
        {
            controlledGrid = newEnt?.Entity?.GetTopMostParent() as MyCubeGrid;
            if (controlledGrid == null)
                displayConnector = null;
            else
                foreach (var connector in displayables)
                    if (connector.CubeGrid == controlledGrid)
                    {
                        displayConnector = connector;
                        break;
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
    }
}
