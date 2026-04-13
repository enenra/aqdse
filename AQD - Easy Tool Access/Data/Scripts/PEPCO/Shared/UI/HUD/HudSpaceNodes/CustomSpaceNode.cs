using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// HUD node used to replace the standard Pixel to World matrix with an arbitrary
        /// world matrix transform given by a user-supplied delegate.
        /// </summary>
        public class CustomSpaceNode : HudSpaceNodeBase
        {
            /// <summary>
            /// Used to update the current draw matrix. If no delegate is set, the node will default
            /// to the matrix supplied by its parent.
            /// </summary>
            public Func<MatrixD> UpdateMatrixFunc { get; set; }

            public CustomSpaceNode(HudParentBase parent = null) : base(parent)
            { }

            protected override void Layout()
            {
                if (UpdateMatrixFunc != null)
                    PlaneToWorldRef[0] = UpdateMatrixFunc();
                else if (Parent?.HudSpace != null)
                    PlaneToWorldRef[0] = Parent.HudSpace.PlaneToWorldRef[0];

                base.Layout();
            }
        }
    }
}
