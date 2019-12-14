using VRage.Game.Components;
using Sandbox.Definitions;
using VRage.Game;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;

namespace enenra.QoL
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class QoLAdjustments : MySessionComponentBase
    {
        public override void BeforeStart()
        {
            base.BeforeStart();
            if (MyAPIGateway.Multiplayer.IsServer) return;
            ChangeCubeBlocks();
            ChangeAmmos();
        }

        /*
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            // place method calls here
        }
        */

        private void ChangeCubeBlocks()
        {
            var camTexturePath = @"\Textures\GUI\Screens\camera_overlay.dds";
            var turretTexturePath = @"\Textures\GUI\Screens\turret_overlay.dds";
            var camTextureFullPath = ModContext.ModPath + camTexturePath;
            var turretTextureFullPath = ModContext.ModPath + turretTexturePath;

            foreach (MyCubeBlockDefinition myCubeBlockDefinition in MyDefinitionManager.Static.GetAllDefinitions().Select(myDefinitionBase => myDefinitionBase as MyCubeBlockDefinition).Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
            {
                if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_CameraBlock))
                {
                    var camDef = myCubeBlockDefinition as MyCameraBlockDefinition;
                    camDef.OverlayTexture = camTextureFullPath;
                }
                else if (myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_InteriorTurret) || myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeGatlingTurret) || myCubeBlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeMissileTurret))
                {
                    var turretDef = myCubeBlockDefinition as MyLargeTurretBaseDefinition;
                    turretDef.OverlayTexture = turretTextureFullPath;
                }
            }
        }

        private static void ChangeAmmos()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyAmmoDefinition ammoDef = def as MyAmmoDefinition;
                if (ammoDef == null) continue;

                if (ammoDef.Id.TypeId == typeof(MyProjectileAmmoDefinition))
                {
                    var caliberDef = ammoDef as MyProjectileAmmoDefinition;

                    if (caliberDef.Id.SubtypeName == "SmallCaliber")
                    {
                        caliberDef.ProjectileTrailColor.X = 0.25f;
                        caliberDef.ProjectileTrailColor.Y = 0.125f;
                        caliberDef.ProjectileTrailColor.Z = 0.1f;
                    }
                    else if (caliberDef.Id.SubtypeName == "LargeCaliber")
                    {
                        caliberDef.ProjectileTrailColor.X = 0.25f;
                        caliberDef.ProjectileTrailColor.Y = 0.125f;
                        caliberDef.ProjectileTrailColor.Z = 0.1f;
                    }
                }
            }
        }
    }
}