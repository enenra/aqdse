using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Definitions;
using VRage.Game.Entity;
using VRage.Game.Entity.UseObject;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

using MyWaypointInfo = Sandbox.ModAPI.Ingame.MyWaypointInfo;

namespace aqd.AppCustomAdvertisements
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LCDPanelsBlock), false, "LargeBlockBillboard", "LargeBlockBillboardRound")]
    class AllTextPanels : AppCustomAdvertisements_Gamelogic
    {
        public override string ReplaceUseobjectName { get; } = "billboard";
    }

    public abstract class AppCustomAdvertisements_Gamelogic : MyGameLogicComponent
    {
        const bool Debug = false;
        const string AppToDetect = App_CustomAdvertisements.Id;

        public abstract string ReplaceUseobjectName { get; }

        IMyTerminalBlock Block;
        IMyTextSurfaceProvider SurfaceProvider;
        MyUseObjectsComponent UseObjectsComp;

        bool ReplacedUseObject = false;
        uint DetectorId = uint.MaxValue;
        MyUseObjectsComponent.DetectorData OriginalData;
        IMyUseObject CustomUseObject;
        private string LastCustomData;
        public bool CustomDataPresent = false;
        public string Image;
        public string BackgroundImage;
        public string MonospaceImage;
        public int MonospacePosX;
        public int MonospacePosY;
        public float SizeX;
        public float SizeY;
        public MyWaypointInfo GPS;
        public Color GPSColor = new Color(255, 255, 255);
        public bool ValidGPS = false;
        public Dictionary<int, TextEntry> TextEntries = new Dictionary<int, TextEntry>{};
        public class TextEntry
        {
            public string Text;
            public int PosX;
            public int PosY;
            public float FontSize;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            Block = Entity as IMyTerminalBlock;
            if (Block?.CubeGrid?.Physics == null)
                return;

            SurfaceProvider = (IMyTextSurfaceProvider)Entity;
            if (SurfaceProvider.SurfaceCount == 0)
                return;

            UseObjectsComp = Block.Components.Get<MyUseObjectsComponentBase>() as MyUseObjectsComponent;
            if (UseObjectsComp == null)
                return;

            var internalBlock = (MyCubeBlock)Entity;
            internalBlock.OnBlockModelChange += BlockModelChanged;
            BlockModelChanged(internalBlock);
        }

        #region Useobject replacement
        void BlockModelChanged(MyEntity _)
        {
            ReplacedUseObject = false;
            DetectorId = uint.MaxValue;

            if (FindDetector())
            {
                CreateReplacementUseObject();
                RefreshReplace();
            }
        }

        bool FindDetector()
        {
            foreach (var kv in UseObjectsComp.DetectorInteractiveObjects)
            {
                if (kv.Value.DetectorName == ReplaceUseobjectName)
                {
                    DetectorId = kv.Key;
                    OriginalData = kv.Value;
                    return true;
                }
            }

            return false;
        }

        void CreateReplacementUseObject()
        {
            if (CustomUseObject != null)
                return;

            var dummies = new Dictionary<string, IMyModelDummy>();
            Block.Model.GetDummies(dummies);
            IMyModelDummy dummy = null;

            foreach (var d in dummies.Values)
            {
                if (d.Matrix.EqualsFast(ref OriginalData.Matrix))
                {
                    dummy = d;
                    break;
                }
            }

            if (dummy == null)
            {
                if (Debug)
                    MyAPIGateway.Utilities.ShowMessage(GetType().Name, $"can't find dummy for detector {ReplaceUseobjectName}!");
                return;
            }

            uint count = (uint)UseObjectsComp.DetectorInteractiveObjects.Count;
            var useObject = new UseObject_AppInteract(Block, dummy.Name, dummy, count);
            CustomUseObject = useObject;
        }

        public void RefreshReplace()
        {
            if (DetectorId == uint.MaxValue)
                return;

            bool replace = false;

            for (int i = 0; i < SurfaceProvider.SurfaceCount; i++)
            {
                var surface = SurfaceProvider.GetSurface(i);
                if (surface.Script == AppToDetect)
                {
                    replace = true;
                    break;
                }
            }

            if (ReplacedUseObject != replace)
            {
                ReplacedUseObject = replace;
                if (replace)
                {
                    UseObjectsComp.DetectorInteractiveObjects[DetectorId] = new MyUseObjectsComponent.DetectorData(CustomUseObject, OriginalData.Matrix, OriginalData.DetectorName);

                    if (Debug)
                        MyAPIGateway.Utilities.ShowMessage(GetType().Name, "swapped to new useobject");
                }
                else
                {
                    UseObjectsComp.DetectorInteractiveObjects[DetectorId] = OriginalData;

                    if (Debug)
                        MyAPIGateway.Utilities.ShowMessage(GetType().Name, "restored original useobject");
                }
            }
        }
        #endregion

        public void ParseCustomData()
        {
            MyIni _ini = new MyIni();
            var result = new List<string>();
            var customData = Block.CustomData;

            if (!object.ReferenceEquals(LastCustomData, customData))
            {
                LastCustomData = customData;

                MyIniParseResult parsed;
                if (!_ini.TryParse(customData, out parsed))
                {
                    CustomDataPresent = false;
                }
                else
                {
                    if (!_ini.ContainsSection("CustomAds"))
                    {
                        CustomDataPresent = false;
                        ValidGPS = false;
                        return;
                    }

                    CustomDataPresent = true;

                    Image = "";
                    var imgText = _ini.Get("CustomAds", "image").ToString();
                    var lcdTextures = MyDefinitionManager.Static.GetLCDTexturesDefinitions();
                    foreach (var texture in lcdTextures)
                    {
                        if (texture.Id.SubtypeId.String == imgText)
                        {
                            Image = imgText;
                            break;
                        }
                        else if (texture.DisplayNameText == imgText)
                        {
                            _ini.Set("CustomAds", "image", texture.Id.SubtypeId.String);
                            Block.CustomData = _ini.ToString();
                            Image = texture.Id.SubtypeId.String;
                            break;
                        }
                    }

                    BackgroundImage = "";
                    var bgImgText = _ini.Get("CustomAds", "bg_img").ToString();
                    foreach (var texture in lcdTextures)
                    {
                        if (texture.Id.SubtypeId.String == bgImgText)
                        {
                            BackgroundImage = bgImgText;
                            break;
                        }
                        else if (texture.DisplayNameText == bgImgText)
                        {
                            _ini.Set("CustomAds", "bg_img", texture.Id.SubtypeId.String);
                            Block.CustomData = _ini.ToString();
                            BackgroundImage = texture.Id.SubtypeId.String;
                            break;
                        }
                    }

                    MonospaceImage = "";
                    if (!string.IsNullOrEmpty(_ini.EndContent)) MonospaceImage = _ini.EndContent;
                    var mono_pos_x = int.TryParse(_ini.Get("CustomAdsMono", "pos_x").ToString(), out MonospacePosX);
                    if (!mono_pos_x) MonospacePosX = 100;
                    var mono_pos_y = int.TryParse(_ini.Get("CustomAdsMono", "pos_y").ToString(), out MonospacePosY);
                    if (!mono_pos_y) MonospacePosY = 0;

                    var size_x = float.TryParse(_ini.Get("CustomAds", "size_x").ToString(), out SizeX);
                    if (!size_x) SizeX = 0.85f;
                    var size_y = float.TryParse(_ini.Get("CustomAds", "size_y").ToString(), out SizeY);
                    if (!size_y) SizeX = 1.1f;

                    var gpsText = "";
                    var split = _ini.Get("CustomAds", "gps").ToString().Split(':');

                    if (split.Length == 7)
                    {
                        gpsText = $"GPS:{split[1]}:{split[2]}:{split[3]}:{split[4]}:";
                        GPSColor = new ColorDefinitionRGBA(split[5].Replace("#", ""));
                        ValidGPS = true;
                    }
                    else if (split.Length == 6)
                    {
                        gpsText = $"GPS:{split[1]}:{split[2]}:{split[3]}:{split[4]}:";
                        ValidGPS = true;
                    }
                    else
                    {
                        ValidGPS = false;
                        return;
                    }

                    ValidGPS = MyWaypointInfo.TryParse(gpsText, out GPS);

                    TextEntries.Clear();

                    for (int i = 0; i < 10; i++)
                    {
                        var text = "";
                        text = _ini.Get("CustomAdsText:" + i, "text").ToString();
                        if (text == "") break;
                        var entry = new TextEntry();

                        entry.Text = text;

                        var posX = int.TryParse(_ini.Get("CustomAdsText:" + i, "pos_x").ToString(), out entry.PosX);
                        if (!posX) entry.PosX = 100;
                        var posY = int.TryParse(_ini.Get("CustomAdsText:" + i, "pos_y").ToString(), out entry.PosY);
                        if (!posY) entry.PosY = 0;
                        var fontSize = float.TryParse(_ini.Get("CustomAdsText:" + i, "font_size").ToString(), out entry.FontSize);
                        if (!fontSize) entry.FontSize = 1.0f;

                        TextEntries.Add(i, entry);
                    }
                }
            }
        }

        public void Interacted(IMyEntity user)
        {
            if (!Block.IsWorking)
                return;

            var chr = (IMyCharacter)user;
            IMyAccessAnyoneCanUse anyoneCanUseComp;
            if (Block.GetUserRelationToOwner(chr.ControllerInfo.ControllingIdentityId).IsFriendly()
            || MyAPIGateway.Session.IsUserUseAllTerminals(MyAPIGateway.Multiplayer.MyId)
            || (Block.Components.TryGet(out anyoneCanUseComp) && anyoneCanUseComp.AnyoneCanUse))
            {
                if (CustomDataPresent && ValidGPS)
                {
                    MyAPIGateway.Utilities.ShowNotification("GPS Downloaded");

                    var gps = MyAPIGateway.Session.GPS.Create(GPS.Name, "", GPS.Coords, true, false);
                    gps.GPSColor = GPSColor;
                    MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.Player.IdentityId, gps);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("Data not found");
                }
            }
            else
            {
                if (chr.ControllerInfo.IsLocallyHumanControlled())
                    MyAPIGateway.Utilities.ShowNotification(MyTexts.GetString(MyStringId.GetOrCompute("AccessDenied")), 2500, MyFontEnum.Red);
            }
        }
    }

    public class UseObject_AppInteract : MyUseObjectBase
    {
        public override UseActionEnum PrimaryAction { get; } = UseActionEnum.Manipulate;
        public override UseActionEnum SecondaryAction { get; } = UseActionEnum.OpenTerminal;

        public UseObject_AppInteract(IMyEntity owner, string dummyName, IMyModelDummy dummyData, uint shapeKey) : base(owner, dummyData)
        {
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            return default(MyActionDescription);
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity user)
        {
            switch (actionEnum)
            {
                case UseActionEnum.Manipulate:
                    {
                        Owner?.GameLogic?.GetAs<AppCustomAdvertisements_Gamelogic>()?.Interacted(user);
                        break;
                    }

                case UseActionEnum.OpenTerminal:
                    MyAPIGateway.Gui.ShowTerminalPage(MyTerminalPageEnum.ControlPanel, user as IMyCharacter, Owner);
                    break;
            }
        }
    }

    [MyTextSurfaceScript(Id, "Custom Advertisements")]
    public class App_CustomAdvertisements : MyTSSCommon
    {
        public const string Id = "CustomAdvertisements";
        public override ScriptUpdate NeedsUpdate { get; } = ScriptUpdate.Update100;

        readonly IMyTerminalBlock TerminalBlock;
        readonly AppCustomAdvertisements_Gamelogic LogicComp;

        public App_CustomAdvertisements(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            TerminalBlock = (IMyTerminalBlock)block;
            TerminalBlock.OnMarkForClose += BlockMarkedForClose;

            LogicComp = TerminalBlock.GameLogic.GetAs<AppCustomAdvertisements_Gamelogic>();
            LogicComp?.RefreshReplace();

            if (TerminalBlock.CustomData == "")
            {
                MyIni _ini = new MyIni();
                _ini.Set("CustomAds", "image", "Grid");
                _ini.Set("CustomAds", "bg_img", "Background01");
                _ini.Set("CustomAds", "size_x", "0.85");
                _ini.Set("CustomAds", "size_y", "1.1");
                _ini.Set("CustomAds", "gps", "");

                _ini.Set("CustomAdsText:0", "text", "My text");
                _ini.Set("CustomAdsText:0", "pos_x", "100");
                _ini.Set("CustomAdsText:0", "pos_y", "0");
                _ini.Set("CustomAdsText:0", "font_size", "1.0");

                _ini.Set("CustomAdsMono", "pos_x", "100");
                _ini.Set("CustomAdsMono", "pos_y", "0");
                TerminalBlock.CustomData = _ini.ToString();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            TerminalBlock.OnMarkForClose -= BlockMarkedForClose;

            LogicComp?.RefreshReplace();
        }

        void Draw()
        {
            Vector2 screenSize = Surface.SurfaceSize;
            Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

            var imgSize = new Vector2(Surface.SurfaceSize.X * LogicComp.SizeX, Surface.SurfaceSize.Y * LogicComp.SizeY);
            var bgImgSize = new Vector2(Surface.SurfaceSize.X, Surface.SurfaceSize.Y);

            LogicComp.ParseCustomData();

            var frame = Surface.DrawFrame();

            if (LogicComp == null)
            {
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Unsupported Screen",
                    Alignment = TextAlignment.CENTER,
                    FontId = MyFontEnum.Red,
                    Color = null,
                    Position = null,
                    Size = null,
                    RotationOrScale = 1,
                });
            }

            else if (!LogicComp.CustomDataPresent)
            {
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Custom Data Invalid",
                    Alignment = TextAlignment.CENTER,
                    FontId = MyFontEnum.Red,
                    Color = null,
                    Position = null,
                    Size = null,
                    RotationOrScale = 1,
                });
            }

            else if (LogicComp.Image == "")
            {
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Invalid Image",
                    Alignment = TextAlignment.CENTER,
                    FontId = MyFontEnum.Red,
                    Color = null,
                    Position = null,
                    Size = null,
                    RotationOrScale = 1,
                });
            }

            else if (!LogicComp.ValidGPS)
            {
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Invalid GPS",
                    Alignment = TextAlignment.CENTER,
                    FontId = MyFontEnum.Red,
                    Color = null,
                    Position = null,
                    Size = null,
                    RotationOrScale = 1,
                });
            }

            else
            {
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = LogicComp.BackgroundImage,
                    Alignment = TextAlignment.CENTER,
                    Color = null,
                    Position = null,
                    Size = bgImgSize,
                });
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = LogicComp.Image,
                    Alignment = TextAlignment.CENTER,
                    Color = null,
                    Position = null,
                    Size = imgSize,
                });

                if (LogicComp.MonospaceImage != "")
                {
                    frame.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = LogicComp.MonospaceImage,
                        Alignment = TextAlignment.LEFT,
                        Color = null,
                        Position = new Vector2(LogicComp.MonospacePosX, LogicComp.MonospacePosY),
                        RotationOrScale = 0.1f,
                        FontId = "Monospace"
                    });
                }

                foreach (var e in LogicComp.TextEntries)
                {
                    frame.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = e.Value.Text,
                        Alignment = TextAlignment.LEFT,
                        Color = Surface.ScriptForegroundColor,
                        Position = new Vector2(e.Value.PosX, e.Value.PosY),
                        RotationOrScale = e.Value.FontSize,
                    });
                }
            }

            frame.Dispose();
        }

        void DrawError(Exception e)
        {
            MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

            try
            {
                Vector2 screenSize = Surface.SurfaceSize;
                Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

                var frame = Surface.DrawFrame();

                var bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", null, null, Color.Black);
                frame.Add(bg);

                var text = MySprite.CreateText($"ERROR: {e.Message}\n{e.StackTrace}\n\nPlease send screenshot of this to mod author.\n{MyAPIGateway.Utilities.GamePaths.ModScopeName}", "White", Color.Red, 0.7f, TextAlignment.LEFT);
                text.Position = screenCorner + new Vector2(16, 16);
                frame.Add(text);

                frame.Dispose();
            }
            catch (Exception e2)
            {
                LogError(e2);
            }
        }

        void BlockMarkedForClose(IMyEntity ent) => Dispose();

        public override void Run()
        {
            try
            {
                base.Run();
                Draw();
            }
            catch (Exception e)
            {
                DrawError(e);
            }
        }

        void LogError(Exception e)
        {
            MyLog.Default.WriteLineAndConsole(e.ToString());

            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification($"[ ERROR in {GetType().FullName} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        }
    }
}
