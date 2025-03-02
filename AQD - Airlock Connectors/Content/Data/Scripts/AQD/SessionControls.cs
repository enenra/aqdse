using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Text;
using VRage.Game.Components;
using VRage.Utils;

namespace ConnectorCheck
{
    public partial class Session : MySessionComponentBase
    {
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
}
