using System;
using VRage;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Interface for types used to define custom UI coordinate spaces
        /// </summary>
        public interface IReadOnlyHudSpaceNode : IReadOnlyHudParent
        {
            /// <summary>
            /// Cursor position on the XY plane defined by the HUD space. Z == dist from screen.
            /// </summary>
            Vector3 CursorPos { get; }

            /// <summary>
            /// Delegate used to retrieve current hud space. Used for cursor depth testing.
            /// </summary>
            HudSpaceDelegate GetHudSpaceFunc { get; }

            /// <summary>
            /// Returns the current draw matrix
            /// </summary>
            MatrixD PlaneToWorld { get; }

            /// <summary>
            /// Returns the current draw matrix by reference as an array of length 1
            /// </summary>
            MatrixD[] PlaneToWorldRef { get; }

            /// <summary>
            /// Returns the world space position of the node's origin.
            /// </summary>
            Func<Vector3D> GetNodeOriginFunc { get; }

            /// <summary>
            /// True if the origin of the HUD space is in front of the camera
            /// </summary>
            bool IsInFront { get; }

            /// <summary>
            /// True if the XY plane of the HUD space is in front and facing toward the camera
            /// </summary>
            bool IsFacingCamera { get; }
        }
    }
}
