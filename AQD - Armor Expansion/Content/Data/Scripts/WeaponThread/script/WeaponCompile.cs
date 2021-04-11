using System.Collections.Generic;
using VRageMath;
using static WeaponThread.WeaponStructure;
using static WeaponThread.WeaponStructure.WeaponDefinition;
using static WeaponThread.WeaponStructure.WeaponDefinition.AnimationDef;
using static WeaponThread.WeaponStructure.WeaponDefinition.AnimationDef.RelMove;
using static WeaponThread.WeaponStructure.WeaponDefinition.AnimationDef.PartAnimationSetDef;
using static WeaponThread.WeaponStructure.WeaponDefinition.AnimationDef.PartAnimationSetDef.EventTriggers;

namespace WeaponThread
{
    partial class Weapons
    {
        internal List<WeaponDefinition> Weapon = new List<WeaponDefinition>();
        internal List<ArmorCompatibilityDef> ArmorBlocks = new List<ArmorCompatibilityDef>();
        internal void ConfigFiles(params WeaponDefinition[] defs)
        {
            foreach (var def in defs) Weapon.Add(def);
        }

        internal void ArmorDefinitions(params ArmorCompatibilityDef[] defs)
        {
            foreach (var def in defs) ArmorBlocks.Add(def);
        }

        internal void HeavyArmorSubtypes(params string[] subtypes)
        {
            foreach (var subtype in subtypes)
            {
                ArmorDefinitions(new ArmorCompatibilityDef { Kind = ArmorCompatibilityDef.ArmorType.Heavy, SubtypeId = subtype });
            }
        }

        internal void LightArmorSubtypes(params string[] subtypes)
        {
            foreach (var subtype in subtypes)
            {
                ArmorDefinitions(new ArmorCompatibilityDef { Kind = ArmorCompatibilityDef.ArmorType.Light, SubtypeId = subtype });
            }
        }

        internal WeaponDefinition[] ReturnDefs()
        {
            var weaponDefinitions = new WeaponDefinition[Weapon.Count];
            for (int i = 0; i < Weapon.Count; i++) weaponDefinitions[i] = Weapon[i];
            Weapon.Clear();
            return weaponDefinitions;
        }

        internal ArmorCompatibilityDef[] ReturnArmorDefs()
        {
            var armorDefinitions = new ArmorCompatibilityDef[ArmorBlocks.Count];
            for (int i = 0; i < ArmorBlocks.Count; i++) armorDefinitions[i] = ArmorBlocks[i];
            ArmorBlocks.Clear();
            return armorDefinitions;
        }

        internal AmmoDef.Randomize Random(float start, float end)
        {
            return new AmmoDef.Randomize { Start = start, End = end };
        }

        internal Vector4 Color(float red, float green, float blue, float alpha)
        {
            return new Vector4(red, green, blue, alpha);
        }

        internal Vector3D Vector(double x, double y, double z)
        {
            return new Vector3D(x, y, z);
        }

        internal XYZ Transformation(double X, double Y, double Z)
        {
            return new XYZ { x = X, y = Y, z = Z };
        }

        internal Dictionary<EventTriggers, uint> Delays(uint FiringDelay = 0, uint ReloadingDelay = 0, uint OverheatedDelay = 0, uint TrackingDelay = 0, uint LockedDelay = 0, uint OnDelay = 0, uint OffDelay = 0, uint BurstReloadDelay = 0, uint OutOfAmmoDelay = 0, uint PreFireDelay = 0, uint StopFiringDelay = 0, uint StopTrackingDelay = 0)
        {
            return new Dictionary<EventTriggers, uint>
            {
                [Firing] = FiringDelay,
                [Reloading] = ReloadingDelay,
                [Overheated] = OverheatedDelay,
                [Tracking] = TrackingDelay,
                [TurnOn] = OnDelay,
                [TurnOff] = OffDelay,
                [BurstReload] = BurstReloadDelay,
                [OutOfAmmo] = OutOfAmmoDelay,
                [PreFire] = PreFireDelay,
                [EmptyOnGameLoad] = 0,
                [StopFiring] = StopFiringDelay,
                [StopTracking] = StopTrackingDelay,
            };
        }

        internal WeaponEmissive Emissive(string EmissiveName, bool CycleEmissiveParts, bool LeavePreviousOn, Vector4[] Colors, float IntensityFrom, float IntensityTo, string[] EmissivePartNames)
        {
            return new WeaponEmissive()
            {
                EmissiveName = EmissiveName,
                Colors = Colors,
                CycleEmissivesParts = CycleEmissiveParts,
                LeavePreviousOn = LeavePreviousOn,
                EmissivePartNames = EmissivePartNames,
                IntensityRange = new[]{ IntensityFrom, IntensityTo }
            };
        }

        internal EventTriggers[] Events(params EventTriggers[] events)
        {
            return events;
        }

        internal string[] Names(params string[] names)
        {
            return names;
        }

        internal string[] AmmoRounds(params string[] names)
        {
            return names;
        }
    }
}
