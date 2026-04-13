using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;

    namespace UI.Client
    {
        public abstract class TerminalPageBase : ITerminalPage
        {
            /// <summary>
            /// Name of the <see cref="ITerminalPage"/> as it appears in the dropdown of the <see cref="IModControlRoot"/>.
            /// </summary>
            public string Name
            {
                get { return GetOrSetMemberFunc(null, (int)TerminalPageAccessors.Name) as string; }
                set { GetOrSetMemberFunc(value, (int)TerminalPageAccessors.Name); }
            }

            /// <summary>
            /// Unique identifier
            /// </summary>
            public object ID => data.Item2;

            /// <summary>
            /// Determines whether or not the <see cref="ITerminalPage"/> will be visible in the mod root.
            /// </summary>
            public bool Enabled
            {
                get { return (bool)GetOrSetMemberFunc(null, (int)TerminalPageAccessors.Enabled); }
                set { GetOrSetMemberFunc(value, (int)TerminalPageAccessors.Enabled); }
            }

            protected ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
            protected readonly ControlMembers data;

            public TerminalPageBase(ModPages pageEnum)
            {
                data = RichHudTerminal.GetNewMenuPage(pageEnum);
            }

            public TerminalPageBase(ControlMembers data)
            {
                this.data = data;
            }

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            public ControlMembers GetApiData() =>
                data;
        }
    }
}