using static Scripts.Structure;
using static Scripts.Structure.ArmorDefinition.ArmorType;
namespace Scripts {   
    partial class Parts {
        // Don't edit above this line
        ArmorDefinition Armor1 => new ArmorDefinition
        {
            SubtypeIds = new[] {
                "AQD_LG_ConveyorCornerArmored",
                "AQD_LG_ConveyorStraightArmored",
                "AQD_LG_ConveyorTArmored",
                "AQD_LG_ConveyorXArmored"
            },
            EnergeticResistance = 1.0f, //Resistance to Energy damage. 0.5f = 200% damage, 2f = 50% damage
            KineticResistance = 1.0f, //Resistance to Kinetic damage. Leave these as 1 for no effect
            Kind = Light, //Heavy, Light, NonArmor - which ammo damage multipliers to apply
        };
    }
}