﻿using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace enenra
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class EmissivePresetChange : MySessionComponentBase
    { 
        private bool isInit = false;

        private void DoWork()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null) continue;

                if (blockDef.EmissiveColorPreset.String == "Default")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "Extended")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "Timer")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "Welder")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "Beacon")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "GravityBlock")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "ConnectBlock")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "UnpoweredOccupancy")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
                }
                else if (blockDef.EmissiveColorPreset.String == "Basic")
                {
                    blockDef.EmissiveColorPreset = MyStringHash.GetOrCompute("AQD_" + blockDef.EmissiveColorPreset.String);
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