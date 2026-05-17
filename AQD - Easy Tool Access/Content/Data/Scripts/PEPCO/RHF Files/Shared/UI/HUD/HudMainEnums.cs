using System;

namespace RichHudFramework
{
    public delegate void EventHandler(object sender, EventArgs e);

    namespace UI
    {
        public enum HudMainAccessors : int
        {
            /// <summary>
            /// out: float
            /// </summary>
            ScreenWidth = 1,

            /// <summary>
            /// out: float
            /// </summary>
            ScreenHeight = 2,

            /// <summary>
            /// out: float
            /// </summary>
            AspectRatio = 3,

            /// <summary>
            /// out: float
            /// </summary>
            ResScale = 4,

            /// <summary>
            /// out: float
            /// </summary>
            Fov = 5,

            /// <summary>
            /// out: float
            /// </summary>
            FovScale = 6,

            /// <summary>
            /// out: MatrixD
            /// </summary>
            PixelToWorldTransform = 7,

            /// <summary>
            /// in/out: RichText
            /// </summary>
            ClipBoard = 8,

            /// <summary>
            /// out: float
            /// </summary>
            UiBkOpacity = 9,

            /// <summary>
            /// in/out: bool
            /// </summary>
            EnableCursor = 10,

            /// <summary>
            /// in/out: bool
            /// </summary>
            RefreshDrawList = 11,

            /// <summary>
            /// in/out: Action<List<HudUpdateAccessors>, byte>
            /// </summary>
            GetUpdateAccessors = 12,

            /// <summary>
            /// out: byte, in: Action{byte}
            /// </summary>
            GetFocusOffset = 13,

            /// <summary>
            /// out: HudSpaceDelegate
            /// </summary>
            GetPixelSpaceFunc = 14,

            /// <summary>
            /// out: Func{Vector3D}
            /// </summary>
            GetPixelSpaceOriginFunc = 15,

            /// <summary>
            /// in: Action
            /// </summary>
            GetInputFocus = 16,

            /// <summary>
            /// out: int
            /// </summary>
            TreeRefreshRate = 17,

            /// <summary>
            /// out: HudInputMode (int)
            /// </summary>
            InputMode = 18,

            /// <summary>
            /// in: Action
            /// </summary>
            SetBeforeDrawCallback = 19,

            /// <summary>
            /// in: Action
            /// </summary>
            SetAfterDrawCallback = 20,

            /// <summary>
            /// in: Action
            /// </summary>
            SetBeforeInputCallback = 21,

            /// <summary>
            /// in: Action
            /// </summary>
            SetAfterInputCallback = 22,
        }

        public enum HudInputMode : int
        {
            NoInput = 0,

            /// <summary>
            /// Cursor is enabled and visible
            /// </summary>
            CursorOnly = 1,

            /// <summary>
            /// Cursor and text input enabled
            /// </summary>
            Full = 2
        }

        public enum ListBoxEntryAccessors : int
        {
            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            Name = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,

            /// <summary>
            /// Object
            /// </summary>
            AssocObject = 3,

            /// <summary>
            /// Object
            /// </summary>
            ID = 4,
        }

        public enum ListBoxAccessors : int
        {
            /// <summary>
            /// CollectionData
            /// </summary>
            ListMembers = 1,

            /// <summary>
            /// in: MyTuple<IList<RichStringMembers>, T>, out: ApiMemberAccessor
            /// </summary>
            Add = 2,

            /// <summary>
            /// out: ListBoxEntry
            /// </summary>
            Selection = 3,

            /// <summary>
            /// out: int
            /// </summary>
            SelectionIndex = 4,

            /// <summary>
            /// in: T (AssocObject)
            /// </summary>
            SetSelectionAtData = 5,

            /// <summary>
            /// in: MyTuple<int, IList<RichStringMembers>, T>
            /// </summary>
            Insert = 6,

            /// <summary>
            /// in: ListBoxEntry, out: bool
            /// </summary>
            Remove = 7,

            /// <summary>
            /// in: int
            /// </summary>
            RemoveAt = 8,

            /// <summary>
            /// void
            /// </summary>
            ClearEntries = 9
        }
    }
}

namespace RichHudFramework.UI.Server { }
namespace RichHudFramework.UI.Rendering.Server { }