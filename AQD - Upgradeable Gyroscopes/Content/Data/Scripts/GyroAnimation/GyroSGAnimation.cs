using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using static GyroAnimations.EmissiveValues;

namespace GyroAnimations
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Gyro), false, new string[] { "SmallBlockGyro" })]
    class GyroSGAnimation : MyGameLogicComponent
    {
        private const string OUTER = "ring_1";

        private const string EMISSIVE_MATERIAL_NAME = "Emissive";

        private MyGyro Gyro;
        private MyEntitySubpart SubpartOuter;

        private bool Override;

        public override void Init(MyObjectBuilder_EntityBase ob)
        {
            if (MyAPIGateway.Utilities.IsDedicated) return;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            Gyro = (MyGyro)Entity;

            if (Gyro?.CubeGrid?.Physics == null) return;

            Gyro.IsWorkingChanged += SetEmissiveColor;

            SetEmissiveColor(Gyro);

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            if (!Gyro.IsFunctional || Gyro.MarkedForClose) return;

            var camPos = MyAPIGateway.Session?.Camera?.WorldMatrix.Translation; // local machine camera position
            if (camPos == null || Vector3D.DistanceSquared((Vector3D)camPos, Entity.GetPosition()) > 90000)
                return;

            if (SubpartOuter == null)
            {
                SetEmissiveColor(Gyro);
                return;
            }

            if (!Override && Gyro.GyroOverride)
            {
                Override = true;
                SetEmissiveColor(Gyro);
            }
            else if (Override && !Gyro.GyroOverride)
            {
                Override = false;
                SetEmissiveColor(Gyro);
            }

            if (!Vector3.IsZero(Gyro.CubeGrid.Physics.AngularVelocity))
                RotateSubparts();
        }

        public override void Close()
        {
            if (MyAPIGateway.Utilities.IsDedicated || Gyro?.CubeGrid?.Physics == null) return;

            Gyro.IsWorkingChanged -= SetEmissiveColor;
        }

        private void SetEmissiveColor(IMyCubeBlock block)
        {
            if (Gyro.MarkedForClose || !TryGetSubparts())
                return;
            
            var emissiveColor = !Gyro.IsFunctional ? Color.Black : !Gyro.IsWorking ? RED : Override ? Color.Cyan : GREEN;
            SubpartOuter.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, emissiveColor, 1f);
        }

        private bool TryGetSubparts()
        {
            if ((Entity as MyEntity).Subparts == null)
                return false;

            if (Entity.TryGetSubpart(OUTER, out SubpartOuter))
                return true;

            return false;
        }

        private void RotateSubparts()
        {
            var angularVelocityRight = Vector3.Dot(Gyro.CubeGrid.Physics.AngularVelocity, Gyro.PositionComp.WorldMatrixRef.Left);
            var outerRotation = Matrix.CreateRotationX(angularVelocityRight);
            var outerLocalMatrix = SubpartOuter.PositionComp.LocalMatrixRef * outerRotation;
            SubpartOuter.PositionComp.SetLocalMatrix(ref outerLocalMatrix);
        }

    }

}
