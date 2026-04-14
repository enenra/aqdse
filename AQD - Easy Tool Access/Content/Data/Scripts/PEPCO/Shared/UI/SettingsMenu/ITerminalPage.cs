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
        using Client;
        using Server;

        public enum TerminalPageAccessors : int
        {
            /// <summary>
            /// string
            /// </summary>
            Name = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,
        }

        public interface ITerminalPage : IModRootMember
        {
            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlMembers GetApiData();
        }
    }
}