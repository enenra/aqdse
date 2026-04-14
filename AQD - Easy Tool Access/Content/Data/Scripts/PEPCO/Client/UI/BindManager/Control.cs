using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            private class Control : IControl
            {
                /// <summary>
                /// Name of the control
                /// </summary>
                public string Name => _instance.GetControlMember(Index, (int)ControlAccessors.Name) as string;

                /// <summary>
                /// Name of the control as displayed in bind menu
                /// </summary>
                public string DisplayName => _instance.GetControlMember(Index, (int)ControlAccessors.DisplayName) as string;

                /// <summary>
                /// Index of the control in the bind manager
                /// </summary>
                public int Index { get; }

                /// <summary>
                /// Returns true if the control is being pressed
                /// </summary>
                public bool IsPressed => (bool)(_instance.GetControlMember(Index, (int)ControlAccessors.IsPressed) ?? false);

                /// <summary>
                /// Returns true if the control was just pressed
                /// </summary>
                public bool IsNewPressed => (bool)(_instance.GetControlMember(Index, (int)ControlAccessors.IsNewPressed) ?? false);

                /// <summary>
                /// Returns true if the control was just released
                /// </summary>
                public bool IsReleased => (bool)(_instance.GetControlMember(Index, (int)ControlAccessors.IsReleased) ?? false);

                // <summary>
                /// Returns true if the control doesn't represent a boolean value. For example, MwUp/Dn
                /// represent scroll wheel movement, but don't return an exact position/displacement.
                /// </summary>
                public bool Analog { get; }

                public Control(int index)
                {
                    this.Index = index;
                }

                public override bool Equals(object obj)
                {
                    return (obj as Control).Index == Index;
                }

                public override int GetHashCode()
                {
                    return Index.GetHashCode();
                }
            }
        }
    }
}