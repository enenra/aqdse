using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace enenra.AdjustConcreteBlockWeight
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class WeightAdjustment : MySessionComponentBase
    {
        public const float weightModifier = 1000.0f;

        private bool isInit = false;

        private void DoWork()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null) continue;

                if (blockDef.BlockTopology != MyBlockTopology.Cube) continue;

                bool overrideWeight = false;
                foreach (var comp in blockDef.Components)
                {
                    if (comp.Definition.Id.SubtypeName == "AQD_Comp_Concrete")
                    {
                        overrideWeight = true;
                        break;
                    }
                }

                if (overrideWeight)
                {
                    blockDef.Mass *= weightModifier;
                }
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {

        }

        public override void LoadData()
        {
            DoWork();
        }
    }
}
