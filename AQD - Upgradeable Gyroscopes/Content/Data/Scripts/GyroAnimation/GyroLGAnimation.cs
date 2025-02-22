﻿using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using static GyroAnimations.EmissiveValues;

namespace GyroAnimations
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Gyro), false, new string[] { "LargeBlockGyro" })]
    class GyroLGAnimation : MyGameLogicComponent
    {
        private const string OUTER = "ring_1";
        private const string INNER = "ring_2";

        private const string EMISSIVE_MATERIAL_NAME = "Emissive";

        private MyGyro Gyro;
        private MyEntitySubpart SubpartOuter;
        private MyEntitySubpart SubpartInner;

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
            }

            Gyro.IsWorkingChanged += SetEmissiveColor;

            SetEmissiveColor(Gyro);

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            if (!Visible || !Gyro.IsFunctional || Gyro.MarkedForClose) return;

            if (SubpartOuter == null || SubpartInner == null)
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
            SubpartInner.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, emissiveColor, 1f);
        }

        private bool TryGetSubparts()
        {
            if ((Entity as MyEntity).Subparts == null)
                return false;

            if (Entity.TryGetSubpart(OUTER, out SubpartOuter))
                if (SubpartOuter.TryGetSubpart(INNER, out SubpartInner))
                    return true;

            return false;
        }

        private void RotateSubparts()
        {
            var angularVelocityRight = Vector3.Dot(Gyro.CubeGrid.Physics.AngularVelocity, Gyro.PositionComp.WorldMatrixRef.Left);
            var outerRotation = Matrix.CreateRotationX(angularVelocityRight);
            var outerLocalMatrix = SubpartOuter.PositionComp.LocalMatrixRef * outerRotation;
            outerLocalMatrix.Translation = SubpartOuter.PositionComp.LocalMatrixRef.Translation; // Account for subpart not being centred
            SubpartOuter.PositionComp.SetLocalMatrix(ref outerLocalMatrix);
            
            var angularVelocityInner = Vector3.Dot(Gyro.CubeGrid.Physics.AngularVelocity, SubpartOuter.PositionComp.WorldMatrixRef.Down);
            var innerRotation = Matrix.CreateRotationY(angularVelocityInner);
            var innerLocalMatrix = SubpartInner.PositionComp.LocalMatrixRef * innerRotation;
            SubpartInner.PositionComp.SetLocalMatrix(ref innerLocalMatrix);
        }

    }

}
