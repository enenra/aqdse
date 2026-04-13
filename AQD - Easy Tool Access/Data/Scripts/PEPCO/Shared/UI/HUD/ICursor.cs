using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>>;

namespace RichHudFramework
{
    namespace UI
    {
        public enum HudCursorAccessors : int
        {
            /// <summary>
            /// out: bool
            /// </summary>
            Visible = 0,

            /// <summary>
            /// out: bool
            /// </summary>
            IsCaptured = 1,

            /// <summary>
            /// out: Vector2
            /// </summary>
            ScreenPos = 2,

            /// <summary>
            /// out: Vector3D
            /// </summary>
            WorldPos = 3,

            /// <summary>
            /// out: LineD
            /// </summary>
            WorldLine = 4,

            /// <summary>
            /// in: Func<ToolTipMembers>
            /// </summary>
            RegisterToolTip = 5,

            /// <summary>
            /// out: bool
            /// </summary>
            IsToolTipRegistered = 6,
        }

        /// <summary>
        /// Interface for the cursor rendered by the Rich HUD Framework
        /// </summary>
        public interface ICursor
        {
            /// <summary>
            /// Indicates whether the cursor is currently visible
            /// </summary>
            bool Visible { get; }

            /// <summary>
            /// Returns true if the cursor has been captured by a UI element
            /// </summary>
            bool IsCaptured { get; }

            /// <summary>
            /// Returns true if a tooltip has been registered
            /// </summary>
            bool IsToolTipRegistered { get; }

            /// <summary>
            /// The position of the cursor in pixels in screen space
            /// </summary>
            Vector2 ScreenPos { get; }

            /// <summary>
            /// Position of the cursor in world space.
            /// </summary>
            Vector3D WorldPos { get; }

            /// <summary>
            /// Line projected from the cursor into world space on the -Z axis 
            /// correcting for apparent warping due to perspective projection.
            /// </summary>
            LineD WorldLine { get; }

            /// <summary>
            /// Returns true if the given HUD space is being captured by the cursor
            /// </summary>
            bool IsCapturingSpace(HudSpaceDelegate GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
            /// is true, then the cursor will be drawn in the given space.
            /// </summary>
            bool TryCaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
            /// is true, then the cursor will be drawn in the given space.
            /// </summary>
            void CaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor with the given object
            /// </summary>
            void Capture(ApiMemberAccessor capturedElement);

            /// <summary>
            /// Indicates whether the cursor is being captured by the given element.
            /// </summary>
            bool IsCapturing(ApiMemberAccessor capturedElement);

            /// <summary>
            /// Attempts to capture the cursor using the given object. Returns true on success.
            /// </summary>
            bool TryCapture(ApiMemberAccessor capturedElement);

            /// <summary>
            /// Attempts to release the cursor from the given element. Returns false if
            /// not capture or if not captured by the object given.
            /// </summary>
            bool TryRelease(ApiMemberAccessor capturedElement);

            /// <summary>
            /// Registers a callback delegate to set the tooltip for the next frame. Tooltips are reset
            /// every tick and must be reregistered in HandleInput() every tick. The first tooltip registered
            /// takes precedence.
            /// </summary>
            void RegisterToolTip(ToolTip toolTip);
        }
    }
}
