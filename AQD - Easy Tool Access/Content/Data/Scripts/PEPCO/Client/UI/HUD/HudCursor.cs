using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    using CursorMembers = MyTuple<
        Func<HudSpaceDelegate, bool>, // IsCapturingSpace
        Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
        Func<ApiMemberAccessor, bool>, // IsCapturing
        Func<ApiMemberAccessor, bool>, // TryCapture
        Func<ApiMemberAccessor, bool>, // TryRelease
        ApiMemberAccessor // GetOrSetMember
    >;

    namespace UI.Client
    {
        public sealed partial class HudMain
        {
            /// <summary>
            /// Wrapper for the cursor rendered by the Rich HUD Framework
            /// </summary>
            private class HudCursor : ICursor
            {
                /// <summary>
                /// Indicates whether the cursor is currently visible
                /// </summary>
                public bool Visible { get; private set; }

                /// <summary>
                /// Returns true if the cursor has been captured by a UI element
                /// </summary>
                public bool IsCaptured => (bool)GetOrSetMemberFunc(null, (int)HudCursorAccessors.IsCaptured);

                /// <summary>
                /// Returns true if a tooltip has been registered
                /// </summary>
                public bool IsToolTipRegistered { get; private set; }

                /// <summary>
                /// The position of the cursor in pixels in screen space
                /// </summary>
                public Vector2 ScreenPos { get; private set; }

                /// <summary>
                /// Position of the cursor in world space.
                /// </summary>
                public Vector3D WorldPos { get; private set; }

                /// <summary>
                /// Line projected from the cursor into world space on the -Z axis 
                /// correcting for apparent warping due to perspective projection.
                /// </summary>
                public LineD WorldLine { get; private set; }

                private readonly Func<HudSpaceDelegate, bool> IsCapturingSpaceFunc;
                private readonly Func<float, HudSpaceDelegate, bool> TryCaptureHudSpaceFunc;
                private readonly Func<ApiMemberAccessor, bool> IsCapturingFunc;
                private readonly Func<ApiMemberAccessor, bool> TryCaptureFunc;
                private readonly Func<ApiMemberAccessor, bool> TryReleaseFunc;
                private readonly ApiMemberAccessor GetOrSetMemberFunc;

                public HudCursor(CursorMembers members)
                {
                    IsCapturingSpaceFunc = members.Item1;
                    TryCaptureHudSpaceFunc = members.Item2;
                    IsCapturingFunc = members.Item3;
                    TryCaptureFunc = members.Item4;
                    TryReleaseFunc = members.Item5;
                    GetOrSetMemberFunc = members.Item6;
                }

                public void Update()
                {
                    Visible = (bool)GetOrSetMemberFunc(null, (int)HudCursorAccessors.Visible);
                    ScreenPos = (Vector2)GetOrSetMemberFunc(null, (int)HudCursorAccessors.ScreenPos);
                    WorldPos = (Vector3D)GetOrSetMemberFunc(null, (int)HudCursorAccessors.WorldPos);
                    WorldLine = (LineD)GetOrSetMemberFunc(null, (int)HudCursorAccessors.WorldLine);
                    IsToolTipRegistered = (bool)GetOrSetMemberFunc(null, (int)HudCursorAccessors.IsToolTipRegistered);
                }

                /// <summary>
                /// Returns true if the given HUD space is being captured by the cursor
                /// </summary>
                public bool IsCapturingSpace(HudSpaceDelegate GetHudSpaceFunc) =>
                    IsCapturingSpaceFunc(GetHudSpaceFunc);

                /// <summary>
                /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
                /// is true, then the cursor will be drawn in the given space.
                /// </summary>
                public bool TryCaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc) =>
                    TryCaptureHudSpaceFunc(depthSquared, GetHudSpaceFunc);

                /// <summary>
                /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
                /// is true, then the cursor will be drawn in the given space.
                /// </summary>
                public void CaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc) =>
                    TryCaptureHudSpaceFunc(depthSquared, GetHudSpaceFunc);

                /// <summary>
                /// Attempts to capture the cursor with the given object
                /// </summary>
                public void Capture(ApiMemberAccessor capturedElement) =>
                    TryCaptureFunc(capturedElement);

                /// <summary>
                /// Indicates whether the cursor is being captured by the given element.
                /// </summary>
                public bool IsCapturing(ApiMemberAccessor capturedElement) =>
                    IsCapturingFunc(capturedElement);

                /// <summary>
                /// Attempts to capture the cursor using the given object. Returns true on success.
                /// </summary>
                public bool TryCapture(ApiMemberAccessor capturedElement) =>
                    TryCaptureFunc(capturedElement);

                /// <summary>
                /// Attempts to release the cursor from the given element. Returns false if
                /// not capture or if not captured by the object given.
                /// </summary>
                public bool TryRelease(ApiMemberAccessor capturedElement) =>
                    TryReleaseFunc(capturedElement);

                /// <summary>
                /// Registers a callback delegate to set the tooltip for the next frame. Tooltips are reset
                /// every tick and must be reregistered in HandleInput() every tick. The first tooltip registered
                /// takes precedence.
                /// </summary>
                public void RegisterToolTip(ToolTip toolTip) =>
                    GetOrSetMemberFunc(toolTip.GetToolTipFunc, (int)HudCursorAccessors.RegisterToolTip);
            }
        }
    }
}
