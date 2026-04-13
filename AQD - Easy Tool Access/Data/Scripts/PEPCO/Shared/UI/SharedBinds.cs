using RichHudFramework.Internal;
using VRage.Input;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Wrapper used to provide easy access to library key binds.
    /// </summary>
    public sealed class SharedBinds : RichHudComponentBase
    {
        public static IBind LeftButton { get { return Instance.sharedMain[0]; } }
        public static IBind RightButton { get { return Instance.sharedMain[1]; } }
        public static IBind MousewheelUp { get { return Instance.sharedMain[2]; } }
        public static IBind MousewheelDown { get { return Instance.sharedMain[3]; } }

        public static IBind Enter { get { return Instance.sharedMain[4]; } }
        public static IBind Back { get { return Instance.sharedMain[5]; } }
        public static IBind Delete { get { return Instance.sharedMain[6]; } }
        public static IBind Escape { get { return Instance.sharedMain[7]; } }

        public static IBind SelectAll { get { return Instance.sharedMain[8]; } }
        public static IBind Copy { get { return Instance.sharedMain[9]; } }
        public static IBind Cut { get { return Instance.sharedMain[10]; } }
        public static IBind Paste { get { return Instance.sharedMain[11]; } }

        public static IBind UpArrow { get { return Instance.sharedMain[12]; } }
        public static IBind DownArrow { get { return Instance.sharedMain[13]; } }
        public static IBind LeftArrow { get { return Instance.sharedMain[14]; } }
        public static IBind RightArrow { get { return Instance.sharedMain[15]; } }

        public static IBind PageUp { get { return Instance.sharedMain[16]; } }
        public static IBind PageDown { get { return Instance.sharedMain[17]; } }
        public static IBind Space { get { return Instance.sharedMain[18]; } }

        public static IBind Control { get { return Instance.sharedModifiers[0]; } }
        public static IBind Shift { get { return Instance.sharedModifiers[1]; } }
        public static IBind Alt { get { return Instance.sharedModifiers[2]; } }

        private static SharedBinds Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static SharedBinds instance;
        private readonly IBindGroup sharedMain, sharedModifiers;

        private SharedBinds() : base(false, true)
        {
            sharedMain = BindManager.GetOrCreateGroup("SharedBinds");
            sharedMain.RegisterBinds(new BindGroupInitializer
            {
                { "leftbutton", MyKeys.LeftButton },
                { "rightbutton", MyKeys.RightButton },
                { "mousewheelup", RichHudControls.MousewheelUp },
                { "mousewheeldown", RichHudControls.MousewheelDown },

                { "enter", MyKeys.Enter },
                { "back", MyKeys.Back },
                { "delete", MyKeys.Delete },
                { "escape", MyKeys.Escape },

                { "selectall", MyKeys.Control, MyKeys.A },
                { "copy", MyKeys.Control, MyKeys.C },
                { "cut", MyKeys.Control, MyKeys.X },
                { "paste", MyKeys.Control, MyKeys.V },

                { "uparrow", MyKeys.Up },
                { "downarrow", MyKeys.Down },
                { "leftarrow", MyKeys.Left },
                { "rightarrow", MyKeys.Right },
                
                { "pageup", MyKeys.PageUp },
                { "pagedown", MyKeys.PageDown },
                { "space", MyKeys.Space },
            });
            sharedModifiers = BindManager.GetOrCreateGroup("SharedModifiers");
            sharedModifiers.RegisterBinds(new BindGroupInitializer
            {
                { "shift", MyKeys.Shift },
                { "control", MyKeys.Control },
                { "alt", MyKeys.Alt },
            });
        }

        private static void Init()
        {
            if (instance == null)
                instance = new SharedBinds();
        }

        public override void Close()
        {
            Instance = null;
        }
    }
}