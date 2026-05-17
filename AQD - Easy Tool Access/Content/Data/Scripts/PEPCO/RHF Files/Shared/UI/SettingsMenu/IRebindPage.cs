using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;

        public enum RebindPageAccessors : int
        {
            Add = 10,
        }

        public interface IRebindPage : ITerminalPage, IEnumerable<IBindGroup>
        {
            /// <summary>
            /// Bind groups registered to the rebind page.
            /// </summary>
            IReadOnlyList<IBindGroup> BindGroups { get; }

            /// <summary>
            /// Adds the given bind group to the page.
            /// </summary>
            void Add(IBindGroup bindGroup);

            /// <summary>
            /// Adds the given bind group to the page along with its associated default configuration.
            /// </summary>
            void Add(IBindGroup bindGroup, BindDefinition[] defaultBinds);
        }
    }
}