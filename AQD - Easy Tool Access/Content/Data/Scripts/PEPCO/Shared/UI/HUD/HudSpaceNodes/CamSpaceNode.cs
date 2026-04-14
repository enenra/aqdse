using Sandbox.ModAPI;
using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        /// <summary>
        /// HUD Space Node used to generate draw matrices based on the camera's world matrix.
        /// Equivalent to HudMain.PixelToWorld in its default configuration.
        /// </summary>
        public class CamSpaceNode : HudSpaceNodeBase
        {
            /// <summary>
            /// Scaling applied on the XY plane of the node's transform.
            /// </summary>
            public float PlaneScale { get; set; }

            /// <summary>
            /// Gets/sets axis the node's transform is rotated about, starting from the matrix's
            /// origin.
            /// </summary>
            public Vector3 RotationAxis { get; set; }

            /// <summary>
            /// Rotation about the axis in radians
            /// </summary>
            public float RotationAngle { get; set; }

            /// <summary>
            /// Displacement of the matrix, in meters, from the camera
            /// </summary>
            public Vector3D TransformOffset { get; set; }

            /// <summary>
            /// If enabled, the matrix's plane will be rescaled to compensate for fov and
            /// resolution s.t. 1 unit == 1 pixel on the near plane.
            /// </summary>
            public bool IsScreenSpace { get; set; }

            /// <summary>
            /// If enabled, HudMain.ResScale will be used to rescale the matrix to compensate
            /// for high PPI displays. Requires IsScreenSpace == true.
            /// </summary>
            public bool UseResScaling { get; set; }

            public CamSpaceNode(HudParentBase parent = null) : base(parent)
            {
                PlaneScale = 1f;
                TransformOffset = new Vector3D(0d, 0d, -MyAPIGateway.Session.Camera.NearPlaneDistance);

                IsScreenSpace = true;
                UseResScaling = true;
            }

            protected override void Layout()
            {
                double scale = PlaneScale;

                if (IsScreenSpace)
                {
                    scale *= HudMain.FovScale / HudMain.ScreenHeight;

                    if (UseResScaling)
                        scale *= HudMain.ResScale;
                }

                var scaling = MatrixD.CreateScale(scale, scale, 1d);
                var rotation = MatrixD.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(RotationAxis, RotationAngle));
                var translation = MatrixD.CreateTranslation(TransformOffset);

                PlaneToWorldRef[0] = (scaling * rotation * translation) * MyAPIGateway.Session.Camera.WorldMatrix;
                base.Layout();
            }
        }
    }
}
