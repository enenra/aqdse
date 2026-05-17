using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    namespace UI
    {
        using Server;
        using Client;

        public enum TerminalControlAccessors : int
        {
            /// <summary>
            /// Action
            /// </summary>
            GetOrSetControlCallback = 1,

            /// <summary>
            /// string
            /// </summary>
            Name = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// in: Func<ToolTipMembers>
            /// </summary>
            ToolTip = 4,

            /// <summary>
            /// T
            /// </summary>
            Value = 8,

            /// <summary>
            /// Func{T}
            /// </summary>
            ValueGetter = 9,
        }

        /// <summary>
        /// Clickable control used in conjunction by the settings menu.
        /// </summary>
        public interface ITerminalControl
        {
            /// <summary>
            /// Raised whenever the control's value is changed.
            /// </summary>
            event EventHandler ControlChanged;

            /// <summary>
            /// Name of the control as it appears in the menu.
            /// </summary>
            string Name { get; set; }

            /// <summary>
            /// Determines whether or not the control will be visible in the menu.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Optional tooltip for the control
            /// </summary>
            ToolTip ToolTip { get; set; }

            /// <summary>
            /// Unique identifer.
            /// </summary>
            object ID { get; }

            EventHandler ControlChangedHandler { get; set; }

            /// <summary>
            /// Retrieves data used by the Framework API
            /// </summary>
            ControlMembers GetApiData();
        }

        /// <summary>
        /// Settings menu control associated with a value of a given type.
        /// </summary>
        public interface ITerminalValue<TValue> : ITerminalControl
        {
            /// <summary>
            /// Value associated with the control.
            /// </summary>
            TValue Value { get; set; }

            /// <summary>
            /// Delegate used to periodically refresh the control's value. Optional.
            /// </summary>
            Func<TValue> CustomValueGetter { get; set; }
        }
    }
}