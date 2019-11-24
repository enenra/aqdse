using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

// Code is based on Gauge's Balanced Deformation code, but heavily modified for more control. 
namespace enenra.ArmorBalance
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class ArmorBalance : MySessionComponentBase
    {
        public const float lightArmorLargeDamageMod = 0.5f;
        public const float lightArmorLargeDeformationMod = 0.2f;
        public const float lightArmorSmallDamageMod = 0.5f;
        public const float lightArmorSmallDeformationMod = 0.2f;

        public const float heavyArmorLargeDamageMod = 0.25f;
        public const float heavyArmorLargeDeformationMod = 0.15f;
        public const float heavyArmorSmallDamageMod = 0.25f;
        public const float heavyArmorSmallDeformationMod = 0.15f;

        private bool isInit = false;

        private void DoWork()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null || blockDef.BlockTopology == MyBlockTopology.TriangleMesh) continue;

                if (blockDef.EdgeType == "Light")
                {
                    if (blockDef.CubeSize == MyCubeSize.Large)
                    {
                        blockDef.GeneralDamageMultiplier = lightArmorLargeDamageMod;
                        blockDef.DeformationRatio = lightArmorLargeDeformationMod;
                    }

                    if (blockDef.CubeSize == MyCubeSize.Small)
                    {
                        blockDef.GeneralDamageMultiplier = lightArmorSmallDamageMod;
                        blockDef.DeformationRatio = lightArmorSmallDeformationMod;
                    }
                }

                if (blockDef.EdgeType == "Heavy")
                {
                    if (blockDef.CubeSize == MyCubeSize.Large)
                    {
                        blockDef.GeneralDamageMultiplier = heavyArmorLargeDamageMod;
                        blockDef.DeformationRatio = heavyArmorLargeDeformationMod;
                    }

                    if (blockDef.CubeSize == MyCubeSize.Small)
                    {
                        blockDef.GeneralDamageMultiplier = heavyArmorSmallDamageMod;
                        blockDef.DeformationRatio = heavyArmorSmallDeformationMod;
                    }
                }
            }
        }
        
        public override bool UpdatedBeforeInit()
        {
            DoWork();
            return true;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            DoWork();
        }

        public override void UpdateBeforeSimulation()
        {
            if (!isInit && MyAPIGateway.Session == null)
            {
                DoWork();
                isInit = true;
            }
        }
    }
}
