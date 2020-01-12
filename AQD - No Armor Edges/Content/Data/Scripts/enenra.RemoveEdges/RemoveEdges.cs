using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace enenra.RemoveEdges
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class RemoveEdges : MySessionComponentBase
    {
        private List<MyCubeBlockDefinition> revertShowEdges = new List<MyCubeBlockDefinition>();

        public override void LoadData()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null)
                    continue;

                if (blockDef.BlockTopology == null)
                    continue;

                if (blockDef.CubeDefinition == null)
                    continue;

                if (blockDef.BlockTopology == MyBlockTopology.TriangleMesh)
                    continue;

                if (!blockDef.CubeDefinition.ShowEdges)
                    continue;

                blockDef.CubeDefinition.ShowEdges = false;
                revertShowEdges.Add(blockDef);
            }
        }

        protected override void UnloadData()
        {
            foreach (var def in revertShowEdges)
            {
                def.CubeDefinition.ShowEdges = true;
            }

            revertShowEdges.Clear();
        }
    }
}