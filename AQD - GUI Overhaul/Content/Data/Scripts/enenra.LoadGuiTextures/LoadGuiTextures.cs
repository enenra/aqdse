using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;

namespace enenra.LoadGuiTextures
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class LoadGuiTextures : MySessionComponentBase
    {
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
    }
}