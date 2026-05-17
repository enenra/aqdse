using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using BindDefinitionData = VRage.MyTuple<string, string[]>;

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
        /// <summary>
        /// Scrollable list of bind group controls.
        /// </summary>
        public class RebindPage : TerminalPageBase, IRebindPage
        {
            /// <summary>
            /// List of bind groups registered to the page.
            /// </summary>
            public IReadOnlyList<IBindGroup> BindGroups => bindGroups;

            public RebindPage GroupContainer => this;

            private readonly List<IBindGroup> bindGroups;

            public RebindPage() : base(ModPages.RebindPage)
            {
                bindGroups = new List<IBindGroup>();
            }

            /// <summary>
            /// Adds the given bind group to the page.
            /// </summary>
            public void Add(IBindGroup bindGroup)
            {
                GetOrSetMemberFunc(bindGroup.ID, (int)RebindPageAccessors.Add);
                bindGroups.Add(bindGroup);
            }

            /// <summary>
            /// Adds the given bind group to the page along with its associated default configuration.
            /// </summary>
            public void Add(IBindGroup bindGroup, BindDefinition[] defaultBinds)
            {
                BindDefinitionData[] data = new BindDefinitionData[defaultBinds.Length];

                for (int n = 0; n < defaultBinds.Length; n++)
                    data[n] = defaultBinds[n];

                GetOrSetMemberFunc(new MyTuple<object, BindDefinitionData[]>(bindGroup.ID, data), (int)RebindPageAccessors.Add);
                bindGroups.Add(bindGroup);
            }

            public IEnumerator<IBindGroup> GetEnumerator() =>
                bindGroups.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                bindGroups.GetEnumerator();
        }
    }
}