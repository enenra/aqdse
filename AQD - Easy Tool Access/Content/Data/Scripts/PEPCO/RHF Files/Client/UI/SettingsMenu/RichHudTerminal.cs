using RichHudFramework.UI.Rendering;
using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using Client;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI.Client
    {
        using SettingsMenuMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMembers
            ControlContainerMembers, // MenuRoot
            Func<int, ControlMembers>, // GetNewControl
            Func<int, ControlContainerMembers>, // GetNewContainer
            Func<int, ControlMembers> // GetNewModPage
        >;

        /// <summary>
        /// Windowed settings menu shared by mods using the framework.
        /// </summary>
        public sealed partial class RichHudTerminal : RichHudClient.ApiModule<SettingsMenuMembers>
        {
            /// <summary>
            /// Mod control root for the client.
            /// </summary>
            public static IModControlRoot Root => Instance.menuRoot;

            /// <summary>
            /// Determines whether or not the terminal is currently open.
            /// </summary>
            public static bool Open => (bool)Instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.GetMenuOpen);

            private static RichHudTerminal Instance
            {
                get { Init(); return _instance; }
                set { _instance = value; }
            }
            private static RichHudTerminal _instance;

            private readonly ModControlRoot menuRoot;
            private readonly ApiMemberAccessor GetOrSetMembersFunc;
            private readonly Func<int, ControlMembers> GetNewControlFunc;
            private readonly Func<int, ControlContainerMembers> GetNewContainerFunc;
            private readonly Func<int, ControlMembers> GetNewPageFunc;
            private readonly Func<ControlContainerMembers> GetNewPageCategoryFunc;

            private RichHudTerminal() : base(ApiModuleTypes.SettingsMenu, false, true)
            {
                var data = GetApiData();

                GetOrSetMembersFunc = data.Item1;
                GetNewControlFunc = data.Item3;
                GetNewContainerFunc = data.Item4;
                GetNewPageFunc = data.Item5;

                GetNewPageCategoryFunc = 
                    GetOrSetMembersFunc(null, (int)TerminalAccessors.GetNewPageCategoryFunc) as Func<ControlContainerMembers>;

                menuRoot = new ModControlRoot(data.Item2);
            }

            public static void Init()
            {
                if (_instance == null)
                {
                    _instance = new RichHudTerminal();
                }
            }

            /// <summary>
            /// Toggles the menu between open and closed
            /// </summary>
            public static void ToggleMenu()
            {
                if (_instance == null)
                    Init();

                _instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.ToggleMenu);
            }

            /// <summary>
            /// Open the menu if chat is visible
            /// </summary>
            public static void OpenMenu()
            {
                if (_instance == null)
                    Init();

                _instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.OpenMenu);
            }

            /// <summary>
            /// Close the menu
            /// </summary>
            public static void CloseMenu()
            {
                if (_instance == null)
                    Init();

                _instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.CloseMenu);
            }

            /// <summary>
            /// Sets the current page to the one given
            /// </summary>
            public static void OpenToPage(TerminalPageBase newPage)
            {
                _instance.GetOrSetMembersFunc(new MyTuple<object, object>(_instance.menuRoot.ID, newPage.ID), (int)TerminalAccessors.OpenToPage);
            }

            /// <summary>
            /// Sets the current page to the one given
            /// </summary>
            public static void SetPage(TerminalPageBase newPage)
            {
                _instance.GetOrSetMembersFunc(new MyTuple<object, object>(_instance.menuRoot.ID, newPage.ID), (int)TerminalAccessors.SetPage);
            }

            public override void Close()
            {
                _instance = null;
            }

            public static ControlMembers GetNewMenuControl(MenuControls controlEnum) =>
                Instance.GetNewControlFunc((int)controlEnum);

            public static ControlContainerMembers GetNewMenuTile() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Tile);

            public static ControlContainerMembers GetNewMenuCategory() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Category);

            public static ControlMembers GetNewMenuPage(ModPages pageEnum) =>
                Instance.GetNewPageFunc((int)pageEnum);

            public static ControlContainerMembers GetNewPageCategory() =>
                Instance.GetNewPageCategoryFunc();
        }
    }
}