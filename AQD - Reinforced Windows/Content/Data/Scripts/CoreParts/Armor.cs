using static Scripts.Structure;
using static Scripts.Structure.ArmorDefinition.ArmorType;
namespace Scripts {   
    partial class Parts {
        // Don't edit above this line
        ArmorDefinition AQD_ReinforcedWindows => new ArmorDefinition
        {
            SubtypeIds = new[] {
                "AQD_LG_ReinforcedWindow_1x1",
                "AQD_SG_ReinforcedWindow_1x1",
                "AQD_LG_ReinforcedWindow_1x1Triangle",
                "AQD_SG_ReinforcedWindow_1x1Triangle",
                "AQD_LG_ReinforcedWindow_1x1TriangleOffset",
                "AQD_SG_ReinforcedWindow_1x1TriangleOffset",
                "AQD_LG_ReinforcedWindow_1x1Corner",
                "AQD_SG_ReinforcedWindow_1x1Corner",
                "AQD_LG_ReinforcedWindow_1x1CornerInv",
                "AQD_SG_ReinforcedWindow_1x1CornerInv",
                "AQD_LG_ReinforcedWindow_1x1Slope",
                "AQD_SG_ReinforcedWindow_1x1Slope",
                "AQD_LG_ReinforcedWindow_1x2Corner",
                "AQD_SG_ReinforcedWindow_1x2Corner",
                "AQD_LG_ReinforcedWindow_1x2CornerInv",
                "AQD_SG_ReinforcedWindow_1x2CornerInv",
                "AQD_LG_ReinforcedWindow_1x2Slope",
                "AQD_SG_ReinforcedWindow_1x2Slope"
            },
            EnergeticResistance = 1.0f, //Resistance to Energy damage. 0.5f = 200% damage, 2f = 50% damage
            KineticResistance = 1.0f, //Resistance to Kinetic damage. Leave these as 1 for no effect
            Kind = Heavy, //Heavy, Light, NonArmor - which ammo damage multipliers to apply
        };
    }
}