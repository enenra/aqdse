using System;
using System.Collections;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    namespace UI.Client
    {
        public class TerminalPageCategory : TerminalPageCategoryBase
        { 
            public TerminalPageCategory() : base(RichHudTerminal.GetNewPageCategory())
            { }
        }

        public abstract class TerminalPageCategoryBase : ITerminalPageCategory
        {
            /// <summary>
            /// Name of the mod as it appears in the <see cref="RichHudTerminal"/> mod list
            /// </summary>
            public string Name
            {
                get { return GetOrSetMemberFunc(null, (int)TerminalPageCategoryAccessors.Name) as string; }
                set { GetOrSetMemberFunc(value, (int)TerminalPageCategoryAccessors.Name); }
            }

            /// <summary>
            /// Read only collection of <see cref="TerminalPageBase"/>s assigned to this object.
            /// </summary>
            public IReadOnlyList<TerminalPageBase> Pages { get; }

            public ITerminalPageCategory PageContainer => this;

            /// <summary>
            /// Unique identifer
            /// </summary>
            public object ID => data.Item3;

            /// <summary>
            /// Currently selected <see cref="TerminalPageBase"/>.
            /// </summary>
            public TerminalPageBase SelectedPage
            {
                get
                {
                    object id = GetOrSetMemberFunc(null, (int)TerminalPageCategoryAccessors.Selection);

                    if (id != null)
                    {
                        for (int n = 0; n < Pages.Count; n++)
                        {
                            if (id == Pages[n].ID)
                                return Pages[n];
                        }
                    }

                    return null;
                }
            }

            /// <summary>
            /// Determines whether or not the element will appear in the list.
            /// Disabled by default.
            /// </summary>
            public bool Enabled
            {
                get { return (bool)GetOrSetMemberFunc(null, (int)TerminalPageCategoryAccessors.Enabled); }
                set { GetOrSetMemberFunc(value, (int)TerminalPageCategoryAccessors.Enabled); }
            }

            protected ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
            protected readonly ControlContainerMembers data;

            public TerminalPageCategoryBase(ControlContainerMembers data)
            {
                this.data = data;

                var GetPageDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
                Func<int, TerminalPageBase> GetPageFunc = (x => new TerminalPage(GetPageDataFunc(x)));
                Pages = new ReadOnlyApiCollection<TerminalPageBase>(GetPageFunc, data.Item2.Item2);
            }

            /// <summary>
            /// Adds the given <see cref="TerminalPageBase"/> to the object.
            /// </summary>
            public void Add(TerminalPageBase page) =>
                GetOrSetMemberFunc(page.ID, (int)TerminalPageCategoryAccessors.AddPage);

            /// <summary>
            /// Adds the given ranges of pages to the control root.
            /// </summary>
            public void AddRange(IReadOnlyList<TerminalPageBase> pages)
            {
                foreach (TerminalPageBase page in pages)
                    GetOrSetMemberFunc(page.ID, (int)TerminalPageCategoryAccessors.AddPage);
            }

            /// <summary>
            /// Retrieves data used by the Framework API
            /// </summary>
            public ControlContainerMembers GetApiData() =>
                data;

            public IEnumerator<TerminalPageBase> GetEnumerator() =>
                Pages.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Pages.GetEnumerator();

            protected class TerminalPage : TerminalPageBase
            {
                public TerminalPage(ControlMembers data) : base(data)
                { }
            }
        }
    }
}