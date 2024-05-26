using System;
using System.Collections.Generic;
using System.IO;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRageRender.Messages;

namespace Digi.Experiments
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Example_AppendToSkin : MySessionComponentBase
    {
        void SetupMaterials()
        {
            SetTexture("Weldless", "PanelingColorable_RemovedByWeldless", TextureType.Alphamask, @"Textures\Debug\Black.dds");
            
            SetTexture("Golden_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Gold\Decals_Atlas_A_01_Gold_cm.DDS");
            SetTexture("Golden_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Gold\Decals_Atlas_A_01_Gold_ng.DDS");
            
            SetTexture("Silver_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Silver\Decals_Atlas_A_01_Silver_cm.DDS");
            SetTexture("Silver_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Silver\Decals_Atlas_A_01_Silver_ng.DDS");
            
            SetTexture("Glamour_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Glamour\Decals_Atlas_A_01_Glamour_cm.DDS");
            SetTexture("Glamour_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Glamour\Decals_Atlas_A_01_Glamour_ng.DDS");
            
            SetTexture("Disco_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Disco\Decals_Atlas_A_01_Disco_cm.DDS");
            SetTexture("Disco_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Disco\Decals_Atlas_A_01_Disco_ng.DDS");
            
            SetTexture("Mossy_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Moss\Decals_Atlas_A_01_Mossy_cm.DDS");
            SetTexture("Mossy_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Moss\Decals_Atlas_A_01_Mossy_ng.DDS");
            SetTexture("Mossy_Armor", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\Moss\Decals_Atlas_A_01_Mossy_add.DDS");
            
            SetTexture("RustNonColorable_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\RustNonColorable\Decals_Atlas_A_01_RustNonColorable_cm.DDS");
            SetTexture("RustNonColorable_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\RustNonColorable\Decals_Atlas_A_01_RustNonColorable_ng.DDS");
            SetTexture("RustNonColorable_Armor", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\RustNonColorable\Decals_Atlas_A_01_RustNonColorable_add.DDS");
            
            SetTexture("Heavy_Rust_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\HeavyRust\Decals_Atlas_A_01_RustColorable_cm.DDS");
            SetTexture("Heavy_Rust_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\HeavyRust\Decals_Atlas_A_01_RustColorable_ng.DDS");
            SetTexture("Heavy_Rust_Armor", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\HeavyRust\Decals_Atlas_A_01_RustColorable_add.DDS");
            
            SetTexture("Rusty_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\RustColorable\Decals_Atlas_A_01_RustColorable_cm.DDS");
            SetTexture("Rusty_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\RustColorable\Decals_Atlas_A_01_RustColorable_ng.DDS");
            SetTexture("Rusty_Armor", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\RustColorable\Decals_Atlas_A_01_RustColorable_add.DDS");
            
            SetTexture("Frozen_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Frozen\Decals_Atlas_A_01_cm.DDS");
            SetTexture("Frozen_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Frozen\Decals_Atlas_A_01_ng.DDS");
            SetTexture("Frozen_Armor", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\Frozen\Decals_Atlas_A_01_add.DDS");
            
            SetTexture("Neon_Colorable_Surface", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\NeonColorableSurface\Decals_Neon_cm.DDS");
            SetTexture("Neon_Colorable_Surface", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\NeonColorableSurface\Decals_Neon_ng.DDS");
            SetTexture("Neon_Colorable_Surface", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\NeonColorableSurface\Decals_Neon_add.DDS");
            
            SetTexture("Neon_Colorable_Lights", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\NeonColorableLights\Decals_Neon_cm.DDS");
            SetTexture("Neon_Colorable_Lights", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\NeonColorableLights\Decals_Neon_ng.DDS");
            SetTexture("Neon_Colorable_Lights", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\NeonColorableLights\Decals_Neon_add.DDS");
            
            SetTexture("Neon_Colorable_Lights", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\NeonColorableLights\Decals_Neon_cm.DDS");
            SetTexture("Neon_Colorable_Lights", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\NeonColorableLights\Decals_Neon_ng.DDS");
            SetTexture("Neon_Colorable_Lights", "PanelingColorable", TextureType.Extensions, @"Textures\Models\Cubes\armor\Skins\NeonColorableLights\Decals_Neon_add.DDS");
            
            SetTexture("Retrofuture_Armor", "PanelingColorable", TextureType.ColorMetal, @"Textures\Models\Cubes\armor\Skins\Disco\Decals_Atlas_A_01_Disco_cm.DDS");
            SetTexture("Retrofuture_Armor", "PanelingColorable", TextureType.NormalGloss, @"Textures\Models\Cubes\armor\Skins\Disco\Decals_Atlas_A_01_Disco_ng.DDS");
        }

        /// <summary>
        /// cloned from VRageRender.Messages.MyTextureType because not whitelisted.
        /// does not seem to actually be used as flags though.
        /// </summary>
        [Flags]
        enum TextureType
        {
            Unspecified = 0x0,
            ColorMetal = 0x1,
            NormalGloss = 0x2,
            Extensions = 0x4,
            Alphamask = 0x8
        }

        Dictionary<string, SkinBackup> BackupSkins;

        public override void LoadData()
        {
            if(MyAPIGateway.Utilities.IsDedicated)
                return; // DS doesn't need any of this

            BackupSkins = new Dictionary<string, SkinBackup>();
            SetupMaterials();
        }

        protected override void UnloadData()
        {
            if(BackupSkins != null)
            {
                foreach(var skinKv in BackupSkins)
                {
                    string skin = skinKv.Key;
                    var data = skinKv.Value;
                    MyDefinitionManager.MyAssetModifiers assetRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(skin);

                    if(assetRender.SkinTextureChanges == null)
                        continue;

                    assetRender.SkinTextureChanges.Clear();

                    foreach(var texKv in data.TextureChanges)
                    {
                        assetRender.SkinTextureChanges.Add(texKv.Key, texKv.Value);
                    }

                    // reminder that assetRender.MetalnessColorable is not settable here
                }
            }
        }

        /// <summary>
        /// Adds or overrides the material for the specified skin.
        /// Skin must be an existing subtype, this does not add new skins (and adding new ones won't work anyway, they don't show up in color picker).
        /// </summary>
        void SetTexture(string skin, string material, TextureType type, string filePath)
        {
            try
            {
                MyDefinitionId skinId = new MyDefinitionId(typeof(MyObjectBuilder_AssetModifierDefinition), skin);
                MyAssetModifierDefinition assetDef = MyDefinitionManager.Static.GetAssetModifierDefinition(skinId);

                if(assetDef == null)
                {
                    MyDefinitionErrors.Add((MyModContext)ModContext, $"Cannot find asset modifier definition: {skin}", TErrorSeverity.Error);
                    return;
                }

                MyDefinitionManager.MyAssetModifiers assetRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(skin);

                if(assetRender.SkinTextureChanges == null)
                {
                    MyDefinitionErrors.Add((MyModContext)ModContext, $"Cannot asset nodifier for render definition: {skin}\nLikely a change in game code, report to author.", TErrorSeverity.Error);
                    return;
                }

                if(!BackupSkins.ContainsKey(skin))
                    BackupSkins[skin] = new SkinBackup(assetRender);

                // not really necessary and complex to undo (because it certainly leaks) so it's disabled.
                #region Modify asset definition
                //int textureDefIndex = -1;
                //MyObjectBuilder_AssetModifierDefinition.MyAssetTexture textureDef = default(MyObjectBuilder_AssetModifierDefinition.MyAssetTexture);
                //
                //for(int i = 0; i < assetDef.Textures.Count; i++)
                //{
                //    MyObjectBuilder_AssetModifierDefinition.MyAssetTexture td = assetDef.Textures[i];
                //    if(td.Location == material && (int)td.Type == (int)type)
                //    {
                //        textureDef = td;
                //        textureDefIndex = i;
                //        break;
                //    }
                //}
                //
                //if(textureDefIndex == -1)
                //{
                //    // not found, add it
                //    textureDef = new MyObjectBuilder_AssetModifierDefinition.MyAssetTexture()
                //    {
                //        Location = material,
                //        Filepath = filePath,
                //        Type = CastHax(textureDef.Type, type),
                //    };
                //
                //    assetDef.Textures.Add(textureDef);
                //}
                //else
                //{
                //    // exists, only override path to file
                //    textureDef.Filepath = filePath;
                //    assetDef.Textures[textureDefIndex] = textureDef;
                //}
                #endregion

                #region Modify asset definition for render
                MyStringId materialId = MyStringId.GetOrCompute(material);
                MyTextureChange changesCopy;
                if(!assetRender.SkinTextureChanges.TryGetValue(materialId, out changesCopy))
                {
                    changesCopy = new MyTextureChange();
                }

                // MyDefinitionManager.InitAssetModifiersForRender() also uses this flag enum with a switch(), so it's not going to be multiple in one
                switch(type)
                {
                    case TextureType.ColorMetal: changesCopy.ColorMetalFileName = filePath; break;
                    case TextureType.NormalGloss: changesCopy.NormalGlossFileName = filePath; break;
                    case TextureType.Extensions: changesCopy.ExtensionsFileName = filePath; break;
                    case TextureType.Alphamask: changesCopy.AlphamaskFileName = filePath; break;
                    default: MyDefinitionErrors.Add((MyModContext)ModContext, $"Unknown type given: {type}", TErrorSeverity.Error); break;
                }

                // add or replace the changes because MyTextureChange is a struct
                assetRender.SkinTextureChanges[materialId] = changesCopy;
                #endregion

                // NOTE: MetalnessColorable cannot be changed from defRender because it's a struct copy; it only works with the SkinTextureChanges because that is a reference.
            }
            catch(Exception e)
            {
                MyDefinitionErrors.Add((MyModContext)ModContext, $"Code error/exception: {e}", TErrorSeverity.Critical);
            }
        }

        /// <summary>
        /// All asset paths need to be full paths and this one appends the full path of the current mod to the relative path given
        /// </summary>
        string PathInMod(string relativePath)
        {
            if(!MyAPIGateway.Utilities.FileExistsInModLocation(relativePath, ModContext.ModItem))
                MyDefinitionErrors.Add((MyModContext)ModContext, $"Cannot find texture relative to mod: {relativePath}", TErrorSeverity.Error);

            return Path.Combine(ModContext.ModPath, relativePath);
        }

        /// <summary>
        /// A little hack to input a prohibited type (typeRef) if you have an object that can cast to it (obj).
        /// </summary>
        //static T CastHax<T>(T typeRef, object obj) => (T)obj;
    }

    struct SkinBackup
    {
        public readonly DictionaryReader<MyStringId, MyTextureChange> TextureChanges;

        public SkinBackup(MyDefinitionManager.MyAssetModifiers assetRender)
        {
            // needs to be cloned
            TextureChanges = new Dictionary<MyStringId, MyTextureChange>(assetRender.SkinTextureChanges);
        }
    }
}
