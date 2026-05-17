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
        using Server;
        using Client;

        using ControlContainerMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember,
            MyTuple<object, Func<int>>, // Member List
            object // ID
        >;

        public enum TerminalAccessors : int
        {
            ToggleMenu = 0,
            OpenMenu = 1,
            CloseMenu = 2,
            OpenToPage = 3,
            SetPage = 4,
            GetMenuOpen = 5,

            /// <summary>
            /// out: Func<ControlContainerMembers>
            /// </summary>
            GetNewPageCategoryFunc = 6
        }

        /// <summary>
        /// Used by the API to specify to request a given type of settings menu control
        /// </summary>
        public enum MenuControls : int
        {
            Checkbox = 1,
            ColorPicker = 2,
            OnOffButton = 3,
            SliderSetting = 4,
            TerminalButton = 5,
            TextField = 6,
            DropdownControl = 7,
            ListControl = 8,
            DragBox = 9,
        }

        public enum ControlContainers : int
        {
            Tile = 1,
            Category = 2,
        }

        public enum ModPages : int
        {
            ControlPage = 1,
            RebindPage = 2,
            TextPage = 3,
        }

        public enum ModControlRootAccessors : int
        {
            /// <summary>
            /// Action
            /// </summary>
            GetOrSetCallback = 1,

            /// <summary>
            /// out: MyTuple<object, Func<int>>
            /// </summary>
            GetCategoryAccessors = 7,

            /// <summary>
            /// in: TerminalPageCategory
            /// </summary>
            AddSubcategory = 8
        }

        public interface IModRootMember
        {
            /// <summary>
            /// Name of the member as it appears in the terminal
            /// </summary>
            string Name { get; set; }

            /// <summary>
            /// Determines whether or not the element will appear in the list.
            /// Disabled by default.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier
            /// </summary>
            object ID { get; }
        }

        /// <summary>
        /// Indented dropdown list of terminal pages and page categories. Root UI element for all terminal controls
        /// associated with a given mod.
        /// </summary>
        public interface IModControlRoot : ITerminalPageCategory
        {
            /// <summary>
            /// Invoked when a new page is selected
            /// </summary>
            event EventHandler SelectionChanged;

            /// <summary>
            /// Page subcategories attached to the mod root
            /// </summary>
            IReadOnlyList<TerminalPageCategoryBase> Subcategories { get; }

            /// <summary>
            /// Adds a page subcategory to the control root
            /// </summary>
            void Add(TerminalPageCategoryBase subcategory);

            /// <summary>
            /// Adds a range of root members to the control root, either subcategories or pages.
            /// </summary>
            void AddRange(IReadOnlyList<IModRootMember> members);

            /// <summary>
            /// Retrieves data used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }
    }
}