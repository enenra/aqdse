using Sandbox.ModAPI;
using System;
using VRage;
using VRage.ModAPI;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// HUD node used to rescale the plane to world matrix of a given parent space.
        /// </summary>
        public class ScaledSpaceNode : HudSpaceNodeBase
        {
            /// <summary>
            /// Scaling applied on the XY plane of the node's transform.
            /// </summary>
            public float PlaneScale { get; set; }

            public Func<float> UpdateScaleFunc { get; set; }

            public ScaledSpaceNode(HudParentBase parent = null) : base(parent)
            { }

            protected override void Layout()
            {
                if (UpdateScaleFunc != null)
                    PlaneScale = UpdateScaleFunc();

                IReadOnlyHudSpaceNode parentSpace = _parent.HudSpace;

                PlaneToWorldRef[0] = MatrixD.CreateScale(PlaneScale, PlaneScale, 1d) * parentSpace.PlaneToWorldRef[0];
                IsInFront = parentSpace.IsInFront;
                IsFacingCamera = parentSpace.IsFacingCamera;

                CursorPos = parentSpace.CursorPos / PlaneScale;
            }
        }
    }
}
