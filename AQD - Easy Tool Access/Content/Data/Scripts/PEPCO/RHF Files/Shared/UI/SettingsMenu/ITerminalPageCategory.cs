using System;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        public enum TerminalPageCategoryAccessors : int
        {
            /// <summary>
            /// string
            /// </summary>
            Name = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// out: ControlMembers
            /// </summary>
            Selection = 4,

            /// <summary>
            /// in: TerminalPageBase
            /// </summary>
            AddPage = 5,

            /// <summary>
            /// in: IReadOnlyList<TerminalPageBase>
            /// </summary>
            AddPageRange = 6,
        }

        /// <summary>
        /// Indented dropdown list of terminal pages.
        /// </summary>
        public interface ITerminalPageCategory : IEnumerable<TerminalPageBase>, IModRootMember
        {
            /// <summary>
            /// Read only collection of <see cref="TerminalPageBase"/>s assigned to this object.
            /// </summary>
            IReadOnlyList<TerminalPageBase> Pages { get; }

            /// <summary>
            /// Used to allow the addition of category elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            ITerminalPageCategory PageContainer { get; }

            /// <summary>
            /// Currently selected <see cref="TerminalPageBase"/>.
            /// </summary>
            TerminalPageBase SelectedPage { get; }

            /// <summary>
            /// Adds a terminal page to the category
            /// </summary>
            void Add(TerminalPageBase page);

            /// <summary>
            /// Adds a range of terminal pages to the category
            /// </summary>
            void AddRange(IReadOnlyList<TerminalPageBase> pages);
        }
    }
}