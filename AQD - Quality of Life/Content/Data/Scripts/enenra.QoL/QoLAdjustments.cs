using VRage.Game.Components;
using Sandbox.Definitions;
using VRage.Game;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System;

namespace enenra.QoL
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class QoLAdjustments : MySessionComponentBase
    {
        public override void BeforeStart()
        {
            base.BeforeStart();
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
                // This ensures that LyleCorp's / Novar's docking cameras with their specific overlay don't get replaced.
                if (myCubeBlockDefinition.Id.SubtypeId.String.Contains("DockingCamera"))
                {
                    break;
                }
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

        private void ChangeAmmos()
        {
            foreach (var def in GetAmmoDefinitions())
            {

                var caliberDef = def as MyProjectileAmmoDefinition;

                if (caliberDef != null)
                {

                    if (caliberDef.Id.SubtypeName == "SmallCaliber")
                    {
                        caliberDef.ProjectileTrailScale = 0.25f;
                        caliberDef.ProjectileTrailColor.X = 0.25f;
                        caliberDef.ProjectileTrailColor.Y = 0.125f;
                        caliberDef.ProjectileTrailColor.Z = 0.1f;
                        caliberDef.ProjectileTrailProbability = 1.0f;
                    }
                    else if (caliberDef.Id.SubtypeName == "LargeCaliber")
                    {
                        caliberDef.ProjectileTrailScale = 0.75f;
                        caliberDef.ProjectileTrailColor.X = 0.25f;
                        caliberDef.ProjectileTrailColor.Y = 0.125f;
                        caliberDef.ProjectileTrailColor.Z = 0.1f;
                        caliberDef.ProjectileTrailProbability = 1.0f;
                    }
                    else if (caliberDef.Id.SubtypeName == "AutocannonShell")
                    {
                        caliberDef.ProjectileTrailScale = 0.75f;
                        caliberDef.ProjectileTrailColor.X = 0.25f;
                        caliberDef.ProjectileTrailColor.Y = 0.125f;
                        caliberDef.ProjectileTrailColor.Z = 0.1f;
                        caliberDef.ProjectileTrailProbability = 1.0f;
                    }
                }
            }
        }

        public List<MyAmmoDefinition> GetAmmoDefinitions()
        {

            var allItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
            var ammoDefinitions = new List<MyAmmoDefinition>();

            foreach (var item in allItems.Where(x => x as MyAmmoMagazineDefinition != null))
            {

                var ammoMag = item as MyAmmoMagazineDefinition;

                try
                {

                    var ammoDef = MyDefinitionManager.Static.GetAmmoDefinition(ammoMag.AmmoDefinitionId);
                    ammoDefinitions.Add(ammoDef);

                }
                catch (Exception)
                {



                }

            }

            return ammoDefinitions;

        }
    }
}