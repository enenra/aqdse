using static Scripts.Structure;
using static Scripts.Structure.ArmorDefinition.ArmorType;
namespace Scripts {   
    partial class Parts {
        // Don't edit above this line
        ArmorDefinition AQD_ConveyorExpansion_ArmoredConveyors => new ArmorDefinition
        {
            SubtypeIds = new[] {
                "AQD_LG_AtmoThrusterL_Armored",
                "AQD_LG_AtmoThrusterL_ArmoredSlope",
                "AQD_LG_AtmoThrusterL_ArmoredSlopeRev",
                "AQD_SG_AtmoThrusterL_Armored",
                "AQD_SG_AtmoThrusterL_ArmoredSlope",
                "AQD_SG_AtmoThrusterL_ArmoredSlopeRev",
                "AQD_LG_AtmoThrusterS_Armored",
                "AQD_LG_AtmoThrusterS_ArmoredSlope",
                "AQD_LG_AtmoThrusterS_ArmoredSlopeRev",
                "AQD_SG_AtmoThrusterS_Armored",
                "AQD_SG_AtmoThrusterS_ArmoredSlope",
                "AQD_SG_AtmoThrusterS_ArmoredSlopeRev",

                "AQD_LG_HydroThrusterL_Armored",
                "AQD_LG_HydroThrusterL_ArmoredSlope",
                "AQD_SG_HydroThrusterL_Armored",
                "AQD_SG_HydroThrusterL_ArmoredSlope",
                "AQD_LG_HydroThrusterS_Armored",
                "AQD_LG_HydroThrusterS_ArmoredSlope",
                "AQD_SG_HydroThrusterS_Armored",
                "AQD_SG_HydroThrusterS_ArmoredSlope",

                "AQD_LG_IonThrusterL_Armored",
                "AQD_LG_IonThrusterL_ArmoredSlope",
                "AQD_SG_IonThrusterL_Armored",
                "AQD_SG_IonThrusterL_ArmoredSlope",
                "AQD_LG_IonThrusterS_Armored",
                "AQD_LG_IonThrusterS_ArmoredSlope",
                "AQD_SG_IonThrusterS_Armored",
                "AQD_SG_IonThrusterS_ArmoredSlope"
            },
            EnergeticResistance = 1.0f, //Resistance to Energy damage. 0.5f = 200% damage, 2f = 50% damage
            KineticResistance = 1.0f, //Resistance to Kinetic damage. Leave these as 1 for no effect
            Kind = Light, //Heavy, Light, NonArmor - which ammo damage multipliers to apply
        };
    }
}