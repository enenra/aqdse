using static Scripts.Structure;
using static Scripts.Structure.ArmorDefinition.ArmorType;
namespace Scripts {   
    partial class Parts {
        // Don't edit above this line
        ArmorDefinition AQD_ArmorExpansion_LightArmor => new ArmorDefinition
        {
            SubtypeIds = new[] {
                "AQD_LG_LA_Corner_Split_2x1x1_Base", 
                "AQD_LG_LA_Corner_Split_2x1x1_Tip", 
                "AQD_LG_LA_HalfPlate", 
                "AQD_LG_LA_Plate_1x1", 
                "AQD_LG_LA_Plate_2x1Base", 
                "AQD_LG_LA_Plate_2x1Tip", 
                "AQD_LG_LA_Plate_Slope", 
                "AQD_LG_LA_Plate_Slope2x1", 
                "AQD_LG_LA_Plate_Triangle", 
                "AQD_LG_LA_Slab_Corner_Split", 
                "AQD_LG_LA_Slab_Half_Corner_Split", 
                "AQD_LG_LA_Slab_InvCorner_Split", 
                "AQD_LG_LA_Slab_RaisedCorner_Split", 
                "AQD_LG_LA_Slab_RaisedCorner_Inset", 
                "AQD_LG_LA_Slope3x1", 
                "AQD_LG_LA_Slope3x1_Corner", 
                "AQD_LG_LA_Slope3x1_InvCorner", 
                "AQD_LG_LA_Slope4x1", 
                "AQD_LG_LA_Slope4x1_Corner", 
                "AQD_LG_LA_Slope4x1_InvCorner",
                "AQD_LG_LA_HalfSlopeTransition",
                "AQD_LG_LA_Slab_SlopeTransition",
                "AQD_LG_LA_SlopeTransition",
                
                "AQD_SG_LA_Corner_Split_2x1x1_Base", 
                "AQD_SG_LA_Corner_Split_2x1x1_Tip", 
                "AQD_SG_LA_HalfPlate", 
                "AQD_SG_LA_Plate_1x1", 
                "AQD_SG_LA_Plate_2x1Base", 
                "AQD_SG_LA_Plate_2x1Tip", 
                "AQD_SG_LA_Plate_Slope", 
                "AQD_SG_LA_Plate_Slope2x1", 
                "AQD_SG_LA_Plate_Triangle", 
                "AQD_SG_LA_Slab_Corner_Split", 
                "AQD_SG_LA_Slab_Half_Corner_Split", 
                "AQD_SG_LA_Slab_InvCorner_Split", 
                "AQD_SG_LA_Slab_RaisedCorner_Split", 
                "AQD_SG_LA_Slab_RaisedCorner_Inset", 
                "AQD_SG_LA_Slope3x1", 
                "AQD_SG_LA_Slope3x1_Corner", 
                "AQD_SG_LA_Slope3x1_InvCorner", 
                "AQD_SG_LA_Slope4x1", 
                "AQD_SG_LA_Slope4x1_Corner", 
                "AQD_SG_LA_Slope4x1_InvCorner",
                "AQD_SG_LA_HalfSlopeTransition",
                "AQD_SG_LA_Slab_SlopeTransition",
                "AQD_SG_LA_SlopeTransition"
            },
            EnergeticResistance = 1.0f, //Resistance to Energy damage. 0.5f = 200% damage, 2f = 50% damage
            KineticResistance = 1.0f, //Resistance to Kinetic damage. Leave these as 1 for no effect
            Kind = Light, //Heavy, Light, NonArmor - which ammo damage multipliers to apply
        };
        ArmorDefinition AQD_ArmorExpansion_HeavyArmor => new ArmorDefinition
        {
            SubtypeIds = new[] {
                "AQD_LG_HA_Corner_Split_2x1x1_Base", 
                "AQD_LG_HA_Corner_Split_2x1x1_Tip", 
                "AQD_LG_HA_HalfPlate", 
                "AQD_LG_HA_Plate_1x1", 
                "AQD_LG_HA_Plate_2x1Base", 
                "AQD_LG_HA_Plate_2x1Tip", 
                "AQD_LG_HA_Plate_Slope", 
                "AQD_LG_HA_Plate_Slope2x1", 
                "AQD_LG_HA_Plate_Triangle", 
                "AQD_LG_HA_Slab_Corner_Split", 
                "AQD_LG_HA_Slab_Half_Corner_Split", 
                "AQD_LG_HA_Slab_InvCorner_Split", 
                "AQD_LG_HA_Slab_RaisedCorner_Split", 
                "AQD_LG_HA_Slab_RaisedCorner_Inset", 
                "AQD_LG_HA_Slope3x1", 
                "AQD_LG_HA_Slope3x1_Corner", 
                "AQD_LG_HA_Slope3x1_InvCorner", 
                "AQD_LG_HA_Slope4x1", 
                "AQD_LG_HA_Slope4x1_Corner", 
                "AQD_LG_HA_Slope4x1_InvCorner",
                "AQD_LG_HA_HalfSlopeTransition",
                "AQD_LG_HA_Slab_SlopeTransition",
                "AQD_LG_HA_SlopeTransition",
                
                "AQD_SG_HA_Corner_Split_2x1x1_Base", 
                "AQD_SG_HA_Corner_Split_2x1x1_Tip", 
                "AQD_SG_HA_HalfPlate", 
                "AQD_SG_HA_Plate_1x1", 
                "AQD_SG_HA_Plate_2x1Base", 
                "AQD_SG_HA_Plate_2x1Tip", 
                "AQD_SG_HA_Plate_Slope", 
                "AQD_SG_HA_Plate_Slope2x1", 
                "AQD_SG_HA_Plate_Triangle", 
                "AQD_SG_HA_Slab_Corner_Split", 
                "AQD_SG_HA_Slab_Half_Corner_Split", 
                "AQD_SG_HA_Slab_InvCorner_Split", 
                "AQD_SG_HA_Slab_RaisedCorner_Split", 
                "AQD_SG_HA_Slab_RaisedCorner_Inset", 
                "AQD_SG_HA_Slope3x1", 
                "AQD_SG_HA_Slope3x1_Corner", 
                "AQD_SG_HA_Slope3x1_InvCorner", 
                "AQD_SG_HA_Slope4x1", 
                "AQD_SG_HA_Slope4x1_Corner", 
                "AQD_SG_HA_Slope4x1_InvCorner",
                "AQD_SG_HA_HalfSlopeTransition",
                "AQD_SG_HA_Slab_SlopeTransition",
                "AQD_SG_HA_SlopeTransition"
            },
            EnergeticResistance = 1.0f, //Resistance to Energy damage. 0.5f = 200% damage, 2f = 50% damage
            KineticResistance = 1.0f, //Resistance to Kinetic damage. Leave these as 1 for no effect
            Kind = Light, //Heavy, Light, NonArmor - which ammo damage multipliers to apply
        };
    }
}