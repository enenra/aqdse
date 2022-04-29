using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using static GyroAnimations.EmissiveValues;

namespace GyroAnimations
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Gyro), false, new string[] { "AQD_LG_LargeGyro" })]
    class LargeGyroAnimation : MyGameLogicComponent
    {
        private const string OUTER = "ring_1";
        private const string INNER = "ring_2";
        private const string CORE = "gyrosphere";

        private const string EMISSIVE_MATERIAL_NAME = "Emissive";

        private MyGyro Gyro;
        private MyEntitySubpart SubpartOuter;
        private MyEntitySubpart SubpartInner;
        private MyEntitySubpart SubpartCore;

        private bool Override;
        private bool Visible;

        public override void Init(MyObjectBuilder_EntityBase ob)
        {
            if (MyAPIGateway.Utilities.IsDedicated) return;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            Gyro = (MyGyro)Entity;
            
            if (Gyro?.CubeGrid?.Physics == null) return;

            Gyro.NeedsWorldMatrix = true;
            if (TryGetSubparts())
            {
                SubpartOuter.NeedsWorldMatrix = true;
                SubpartInner.NeedsWorldMatrix = true;
                SubpartCore.NeedsWorldMatrix = true;
            }

            Gyro.IsWorkingChanged += SetEmissiveColor;

            SetEmissiveColor(Gyro);

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            if (!Visible || !Gyro.IsFunctional || Gyro.MarkedForClose) return;

            if (SubpartOuter == null || SubpartInner == null || SubpartCore == null)
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

        public override void UpdateAfterSimulation100()
        {
            var camPos = MyAPIGateway.Session?.Camera?.Position;
            Visible = camPos != null && Vector3D.DistanceSquared((Vector3D)camPos, Gyro.PositionComp.WorldMatrixRef.Translation) < 90000;
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
            SubpartCore.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, emissiveColor, 1f);
        }

        private bool TryGetSubparts()
        {
            if ((Entity as MyEntity).Subparts == null)
                return false;

            if (Entity.TryGetSubpart(OUTER, out SubpartOuter))
                if (SubpartOuter.TryGetSubpart(INNER, out SubpartInner))
                    if (SubpartInner.TryGetSubpart(CORE, out SubpartCore))
                        return true;

            return false;
        }

        private void RotateSubparts()
        {
            var angularVelocityRight = Vector3.Dot(Gyro.CubeGrid.Physics.AngularVelocity, Gyro.PositionComp.WorldMatrixRef.Forward);
            var outerRotation = Matrix.CreateRotationZ(angularVelocityRight);
            var outerLocalMatrix = SubpartOuter.PositionComp.LocalMatrixRef * outerRotation;
            SubpartOuter.PositionComp.SetLocalMatrix(ref outerLocalMatrix);

            var angularVelocityInner = Vector3.Dot(Gyro.CubeGrid.Physics.AngularVelocity, SubpartOuter.PositionComp.WorldMatrixRef.Left);
            var innerRotation = Matrix.CreateRotationX(angularVelocityInner);
            var innerLocalMatrix = SubpartInner.PositionComp.LocalMatrixRef * innerRotation;
            SubpartInner.PositionComp.SetLocalMatrix(ref innerLocalMatrix);

            var angularVelocityCore = Vector3.Dot(Gyro.CubeGrid.Physics.AngularVelocity, SubpartOuter.PositionComp.WorldMatrixRef.Down);
            var coreRotation = Matrix.CreateRotationY(angularVelocityCore);
            var coreLocalMatrix = SubpartCore.PositionComp.LocalMatrixRef * coreRotation;
            SubpartCore.PositionComp.SetLocalMatrix(ref coreLocalMatrix);
        }
    
    }

}
