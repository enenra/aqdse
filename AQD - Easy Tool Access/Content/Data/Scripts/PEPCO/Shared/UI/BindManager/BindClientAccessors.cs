using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Flags used to block input for the game's controls. Useful for modifying normal
    /// input behavior. Does not affect all binds.
    /// </summary>
    [Flags]
    public enum SeBlacklistModes : int
    {
        /// <summary>
        /// Default: no blacklist.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Set flag to disable mouse button keybinds. 
        /// </summary>
        Mouse = 0x1,

        /// <summary>
        /// Set flag to blacklist every blacklist-able bind. 
        /// Keep in mind that not every SE bind can be disabled.
        /// </summary>
        AllKeys = 0x2 | Mouse,

        /// <summary>
        /// Set flag to disable camera rotation (does not disable look with alt)
        /// </summary>
        CameraRot = 0x4,

        /// <summary>
        /// Set flag to disable mouse buttons as well as camera rotation.
        /// </summary>
        MouseAndCam = Mouse | CameraRot,

        /// <summary>
        /// Set flag to disable all key binds as well as camera rotation
        /// </summary>
        Full = AllKeys | CameraRot
    }

    public enum BindClientAccessors : int
    {
        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetOrCreateGroup = 1,

        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetBindGroup = 2,

        /// <summary>
        /// in: IReadOnlyList{string}, out: int[]
        /// </summary>
        GetComboIndices = 3,

        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetControlByName = 4,

        /// <summary>
        /// void
        /// </summary>
        ClearBindGroups = 5,

        /// <summary>
        /// void
        /// </summary>
        Unload = 6,

        /// <summary>
        /// in/out: SeBlacklistModes
        /// </summary>
        RequestBlacklistMode = 7,

        /// <summary>
        /// out: bool
        /// </summary>
        IsChatOpen = 8,
    }
}