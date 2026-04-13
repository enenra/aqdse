using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using RichHudFramework.UI.Rendering;
using ApiMemberAccessor = System.Func<object, int, object>;
using FloatProp = VRage.MyTuple<System.Func<float>, System.Action<float>>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>>;
using Vec2Prop = VRage.MyTuple<System.Func<VRageMath.Vector2>, System.Action<VRageMath.Vector2>>;

namespace RichHudFramework
{
    using Internal;
    using Client;
    using CursorMembers = MyTuple<
        Func<HudSpaceDelegate, bool>, // IsCapturingSpace
        Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
        Func<ApiMemberAccessor, bool>, // IsCapturing
        Func<ApiMemberAccessor, bool>, // TryCapture
        Func<ApiMemberAccessor, bool>, // TryRelease
        ApiMemberAccessor // GetOrSetMember
    >;
    using TextBuilderMembers = MyTuple<
        MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
        Func<Vector2I, int, object>, // GetCharMember
        ApiMemberAccessor, // GetOrSetMember
        Action<IList<RichStringMembers>, Vector2I>, // Insert
        Action<IList<RichStringMembers>>, // SetText
        Action // Clear
    >;

    namespace UI
    {
        using TextBoardMembers = MyTuple<
            TextBuilderMembers,
            FloatProp, // Scale
            Func<Vector2>, // Size
            Func<Vector2>, // TextSize
            Vec2Prop, // FixedSize
            Action<BoundingBox2, BoundingBox2, MatrixD[]> // Draw 
        >;

        namespace Client
        {
            using HudClientMembers = MyTuple<
                CursorMembers, // Cursor
                Func<TextBoardMembers>, // GetNewTextBoard
                ApiMemberAccessor, // GetOrSetMembers
                Action // Unregister
            >;
            using HudUpdateAccessors = MyTuple<
                ApiMemberAccessor,
                MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
                Action, // DepthTest
                Action, // HandleInput
                Action<bool>, // BeforeLayout
                Action // BeforeDraw
            >;

            public sealed partial class HudMain : RichHudClient.ApiModule<HudClientMembers>
            {
                /// <summary>
                /// Root parent for all HUD elements.
                /// </summary>
                public static HudParentBase Root
                {
                    get
                    {
                        if (_instance == null)
                            Init();

                        return _instance.root;
                    }
                }

                /// <summary>
                /// Root node for high DPI scaling at > 1080p. Draw matrix automatically rescales to comensate
                /// for decrease in apparent size due to high DPI displays.
                /// </summary>
                public static HudParentBase HighDpiRoot
                {
                    get
                    {
                        if (_instance == null)
                            Init();

                        return _instance.highDpiRoot;
                    }
                }

                /// <summary>
                /// Cursor shared between mods.
                /// </summary>
                public static ICursor Cursor
                {
                    get
                    {
                        if (_instance == null)
                            Init();

                        return _instance.cursor;
                    }
                }

                /// <summary>
                /// Shared clipboard.
                /// </summary>
                public static RichText ClipBoard
                {
                    get
                    {
                        object value = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.ClipBoard);

                        if (value != null)
                            return new RichText(value as List<RichStringMembers>);
                        else
                            return default(RichText);
                    }
                    set { Instance.GetOrSetMemberFunc(value.apiData, (int)HudMainAccessors.ClipBoard); }
                }

                /// <summary>
                /// Resolution scale normalized to 1080p for resolutions over 1080p. Returns a scale of 1f
                /// for lower resolutions.
                /// </summary>
                public static float ResScale { get; private set; }

                /// <summary>
                /// Matrix used to convert from 2D pixel-value screen space coordinates to worldspace.
                /// </summary>
                public static MatrixD PixelToWorld => PixelToWorldRef[0];

                /// <summary>
                /// Matrix used to convert from 2D pixel-value screen space coordinates to worldspace.
                /// </summary>
                public static MatrixD[] PixelToWorldRef { get; private set; }

                /// <summary>
                /// The current horizontal screen resolution in pixels.
                /// </summary>
                public static float ScreenWidth { get; private set; }

                /// <summary>
                /// The current vertical resolution in pixels.
                /// </summary>
                public static float ScreenHeight { get; private set; }

                /// <summary>
                /// The current aspect ratio (ScreenWidth/ScreenHeight).
                /// </summary>
                public static float AspectRatio { get; private set; }

                /// <summary>
                /// The current field of view
                /// </summary>
                public static float Fov { get; private set; }

                /// <summary>
                /// Scaling used by MatBoards to compensate for changes in apparent size and position as a result
                /// of changes to Fov.
                /// </summary>
                public static float FovScale { get; private set; }

                /// <summary>
                /// The current opacity for the in-game menus as configured.
                /// </summary>
                public static float UiBkOpacity { get; private set; }

                /// <summary>
                /// If true then the cursor will be visible while chat is open
                /// </summary>
                public static bool EnableCursor { get; set; }

                /// <summary>
                /// Current input mode. Used to indicate whether UI elements should accept cursor or text input.
                /// </summary>
                public static HudInputMode InputMode { get; private set; }

                private static HudMain Instance
                {
                    get { Init(); return _instance; }
                    set { _instance = value; }
                }
                private static HudMain _instance;

                private readonly HudClientRoot root;
                private readonly ScaledSpaceNode highDpiRoot;
                private readonly HudCursor cursor;
                private bool enableCursorLast;

                private readonly Func<TextBoardMembers> GetTextBoardDataFunc;
                private readonly ApiMemberAccessor GetOrSetMemberFunc;
                private readonly Action UnregisterAction;

                private HudMain() : base(ApiModuleTypes.HudMain, false, true)
                {
                    if (_instance != null)
                        throw new Exception("Only one instance of HudMain can exist at any give time!");

                    _instance = this;
                    var members = GetApiData();

                    cursor = new HudCursor(members.Item1);
                    GetTextBoardDataFunc = members.Item2;
                    GetOrSetMemberFunc = members.Item3;
                    UnregisterAction = members.Item4;

                    PixelToWorldRef = new MatrixD[1];
                    root = new HudClientRoot();
                    highDpiRoot = new ScaledSpaceNode(root) { UpdateScaleFunc = () => ResScale };

                    Action<List<HudUpdateAccessors>, byte> rootDelegate = root.GetUpdateAccessors,
                        safeAccessor = (List<HudUpdateAccessors> list, byte depth) =>
                        {
                            ExceptionHandler.Run(() => rootDelegate(list, depth));
                        };

                    // Register update delegate
                    GetOrSetMemberFunc(safeAccessor, (int)HudMainAccessors.GetUpdateAccessors);

                    GetOrSetMemberFunc(new Action(() => ExceptionHandler.Run(BeforeMasterDraw)), (int)HudMainAccessors.SetBeforeDrawCallback);
                    GetOrSetMemberFunc(new Action(() => ExceptionHandler.Run(AfterMasterDraw)), (int)HudMainAccessors.SetAfterDrawCallback);

                    UpdateCache();
                }

                private static void Init()
                {
                    BillBoardUtils.Init();

                    if (_instance == null)
                        new HudMain();
                }

                private void BeforeMasterDraw()
                {
                    UpdateCache();
                    cursor.Update();
                    BillBoardUtils.BeginDraw();
                }

                private void AfterMasterDraw()
                {
                    BillBoardUtils.FinishDraw();
                }
 
                public override void Close()
                {
                    UnregisterAction?.Invoke();
                    _instance = null;
                }

                private void UpdateCache()
                {
                    ScreenWidth = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenWidth);
                    ScreenHeight = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenHeight);
                    AspectRatio = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.AspectRatio);
                    ResScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ResScale);
                    Fov = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.Fov);
                    FovScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.FovScale);
                    PixelToWorldRef[0] = (MatrixD)GetOrSetMemberFunc(null, (int)HudMainAccessors.PixelToWorldTransform);
                    UiBkOpacity = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.UiBkOpacity);
                    InputMode = (HudInputMode)GetOrSetMemberFunc(null, (int)HudMainAccessors.InputMode);

                    if (EnableCursor != enableCursorLast)
                        GetOrSetMemberFunc(EnableCursor, (int)HudMainAccessors.EnableCursor);
                    else
                        EnableCursor = (bool)GetOrSetMemberFunc(null, (int)HudMainAccessors.EnableCursor);

                    enableCursorLast = EnableCursor;
                }

                /// <summary>
                /// Returns the ZOffset for focusing a window and registers a callback
                /// for when another object takes focus.
                /// </summary>
                public static byte GetFocusOffset(Action<byte> LoseFocusCallback) =>
                    (byte)Instance.GetOrSetMemberFunc(LoseFocusCallback, (int)HudMainAccessors.GetFocusOffset);

                /// <summary>
                /// Registers a callback for UI elements taking input focus. Callback
                /// invoked when another element takes focus.
                /// </summary>
                public static void GetInputFocus(Action LoseFocusCallback) =>
                    Instance.GetOrSetMemberFunc(LoseFocusCallback, (int)HudMainAccessors.GetInputFocus);

                /// <summary>
                /// Returns accessors for a new TextBoard
                /// </summary>
                public static TextBoardMembers GetTextBoardData() =>
                    Instance.GetTextBoardDataFunc();

                /// <summary>
                /// Converts from a position in absolute screen space coordinates to a position in pixels.
                /// </summary>
                public static Vector2 GetPixelVector(Vector2 scaledVec)
                {
                    if (_instance == null)
                        Init();

                    return new Vector2
                    (
                        (int)(scaledVec.X * ScreenWidth),
                        (int)(scaledVec.Y * ScreenHeight)
                    );
                }

                /// <summary>
                /// Converts from a coordinate given in pixels to a position in absolute units.
                /// </summary>
                public static Vector2 GetAbsoluteVector(Vector2 pixelVec)
                {
                    if (_instance == null)
                        Init();

                    return new Vector2
                    (
                        pixelVec.X / ScreenWidth,
                        pixelVec.Y / ScreenHeight
                    );
                }

                /// <summary>
                /// Root UI element for the client. Registered directly to master root.
                /// </summary>
                private class HudClientRoot : HudParentBase, IReadOnlyHudSpaceNode
                {
                    public override bool Visible => true;

                    public bool DrawCursorInHudSpace { get; }

                    public Vector3 CursorPos { get; private set; }

                    public HudSpaceDelegate GetHudSpaceFunc { get; }

                    public MatrixD PlaneToWorld => PlaneToWorldRef[0];

                    public MatrixD[] PlaneToWorldRef { get; }

                    public Func<MatrixD> UpdateMatrixFunc { get; }

                    public Func<Vector3D> GetNodeOriginFunc { get; }

                    public bool IsInFront { get; }

                    public bool IsFacingCamera { get; }

                    public HudClientRoot()
                    {
                        accessorDelegates.Item2 = new MyTuple<Func<ushort>, Func<Vector3D>>(() => 0, null);

                        State |= HudElementStates.CanUseCursor;
                        DrawCursorInHudSpace = true;
                        HudSpace = this;
                        IsInFront = true;
                        IsFacingCamera = true;
                        PlaneToWorldRef = new MatrixD[1];

                        GetHudSpaceFunc = _instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.GetPixelSpaceFunc) as HudSpaceDelegate;
                        GetNodeOriginFunc = _instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.GetPixelSpaceOriginFunc) as Func<Vector3D>;
                    }

                    protected override void Layout()
                    {
                        PlaneToWorldRef[0] = PixelToWorldRef[0];
                        CursorPos = new Vector3(Cursor.ScreenPos.X, Cursor.ScreenPos.Y, 0f);
                    }
                }
            }
        }
    }

    namespace UI.Server
    { }
}