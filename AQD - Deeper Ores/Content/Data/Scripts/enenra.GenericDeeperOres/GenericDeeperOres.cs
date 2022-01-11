using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace enenra.GenericDeeperOres
{

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class GenericDeeperOres : MySessionComponentBase
    {

        public override void LoadData()
        {

            var allPlanets = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions();

            foreach (var def in allPlanets)
            {

                var planet = def as MyPlanetGeneratorDefinition;
                var oreList = new List<MyPlanetOreMapping>(planet.OreMappings.ToList());

                for (int i = 0; i < oreList.Count; i++) {
                    var oreMap = planet.OreMappings[i];

                    if (oreMap.Type.Contains("Ice_01") == true) { oreMap.Start = 20; oreMap.Depth = 20; }

                    if (oreMap.Type.Contains("Iron_02") == true) { oreMap.Start = 50; oreMap.Depth = 25; }
                    if (oreMap.Type.Contains("Nickel_01") == true) { oreMap.Start = 50; oreMap.Depth = 15; }
                    if (oreMap.Type.Contains("Silicon_01") == true) { oreMap.Start = 50; oreMap.Depth = 20; }

                    if (oreMap.Type.Contains("Cobalt_01") == true) { oreMap.Start = 100; oreMap.Depth = 15; }
                    if (oreMap.Type.Contains("Magnesium_01") == true) { oreMap.Start = 200; oreMap.Depth = 15; }

                    if (oreMap.Type.Contains("Silver_01") == true) { oreMap.Start = 300; oreMap.Depth = 10; }
                    if (oreMap.Type.Contains("Gold_01") == true) { oreMap.Start = 300; oreMap.Depth = 10; }

                    if (oreMap.Type.Contains("Platinum_01") == true) { oreMap.Start = 400; oreMap.Depth = 10; }
                    if (oreMap.Type.Contains("Uraninite_01") == true) { oreMap.Start = 400; oreMap.Depth = 10; }
                }

                planet.OreMappings = oreList.ToArray();

            }

        }


    }

}