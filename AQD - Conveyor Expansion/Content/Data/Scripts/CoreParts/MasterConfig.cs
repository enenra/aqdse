namespace Scripts
{
    partial class Parts
    {
        internal Parts()
        {
            // naming convention: WeaponDefinition Name
            //
            // Enable your definitions using the follow syntax:
            // PartDefinitions(Your1stDefinition, Your2ndDefinition, Your3rdDefinition);
            // PartDefinitions includes both weapons and phantoms
            PartDefinitions(Weapon75, Phantom01);
            ArmorDefinitions(Armor1, Armor2);
            SupportDefinitions(ArmorEnhancer1A);
            UpgradeDefinitions(Upgrade75a, Upgrade75b);
        }
    }
}