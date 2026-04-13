using PEPCO.Utilities;
using RichHudFramework.Client;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.Utils;
using static PEPCO.ScriptHelpers;

namespace PEPCO
{
    /// <summary>
    /// Session-level component for the mod.
    /// Runs very early in the game loop (BeforeSimulation) and is responsible for:
    /// - Establishing a singleton-like access point via <see cref="Instance"/>.
    /// - Detecting runtime context (server/client/dedicated/headless).
    /// - Initializing debugging flags and logging basic mod metadata.
    /// </summary>
    /// <remarks>
    /// Space Engineers instantiates this once per game session.
    /// Use <see cref="Instance"/> to access this component from other parts of the mod after <see cref="LoadData"/> executes.
    /// </remarks>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class EasyToolSwap_Session : MySessionComponentBase
    {
        // Replace "YourCustomBlockSubtypeId" with the actual SubtypeId from your SBC file
        private readonly MyStringHash _targetSubtypeId = MyStringHash.GetOrCompute("CycleToolsFakeBlock");

        public enum EquippedToolType
        {
            None     = 0,
            Welder   = 1,
            Grinder  = 2,
            Drill    = 3,
            Pistol   = 4,
            Rifle    = 5,
            Launcher = 6,
            PaintGun = 7
        }

        // Tracks what is currently physically in the player's hands
        private EquippedToolType _lastEquippedTool = EquippedToolType.None;

        // Remembers the last tool we successfully cycled to (for when hands are empty)
        private EquippedToolType _cycleMemoryTool = EquippedToolType.Welder;

        // Keeps track of the state so we only trigger code on the exact moment of selection/deselection
        private bool _isOurBlockEquipped = false;

        // Cached list to prevent memory allocation when checking inventories (Zero Garbage Generation)
        private readonly List<VRage.Game.ModAPI.Ingame.MyInventoryItem> _inventoryCache = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();

        /// <summary>
        /// Maps every PhysicalGunObject definition ID to its localised display name.
        /// Populated once during <see cref="InitOnce"/> for quick lookup by the weapon wheel.
        /// </summary>
        public static readonly Dictionary<MyDefinitionId, string> GunDisplayNames = new Dictionary<MyDefinitionId, string>(MyDefinitionId.Comparer);

        /// <summary>
        /// Singleton-like reference to the active session component instance.
        /// Set during <see cref="LoadData"/> so other classes can locate session-level state.
        /// </summary>
        public static EasyToolSwap_Session Instance; // the only way to access session comp from other classes

        /// <summary>
        /// True when the Paint Gun mod (Workshop ID 500818376) is active in the current session.
        /// Populated during <see cref="InitOnce"/>.
        /// </summary>
        public static bool IsPaintGunInstalled { get; private set; }

        // Mod metadata
        private readonly string USERCONFIGFILENAME = ModParameter.MODNAME + "_Config.xml";
        public readonly UserConfigSettings Settings = new UserConfigSettings();

        // RHF controls
        private IBindGroup _keyBinds, _keyBindsHidden;
        private RebindPage _bindsPage;
        private ControlPage _settingsPage;
        private ToolWheelMenu _toolWheel;

        // Tracks whether InitOnce has been called
        private bool isInit = false;

        // Tracks whether the RHF terminal was open so we can sync keybinds after it closes
        private bool _terminalOpen = false;
        private int _tickSettingsUpdate = 0;

        // --- DEBUG TRACKING VARIABLES (Prevents Log Spam) ---
        private bool _lastLoggedBuilderState = false;
        private string _lastLoggedBuilderBlock = "";

        private bool _injectComplete = false; // Used to track if we've done our injection yet, to prevent multiple attempts

        // When true the wheel was opened by the fake block rather than the keybind.
        private bool _wheelOpenedViaBlock = false;

        // The toolbar number key that was held when the fake block fired, or None if not found.
        // Used to detect key-release for HoldToKeepOpen auto-confirm via the block path.
        private MyKeys _blockTriggerKey = MyKeys.None;

        private static readonly MyKeys[] ToolbarKeys = new MyKeys[]
        {
            MyKeys.D1, MyKeys.D2, MyKeys.D3, MyKeys.D4, MyKeys.D5,
            MyKeys.D6, MyKeys.D7, MyKeys.D8, MyKeys.D9, MyKeys.D0
        };

        /// <summary>
        /// Called by the game when the session component is created.
        /// Initializes context flags and logs startup information.
        /// </summary>
        public override void LoadData()
        {
            // Expose this instance globally for other mod classes.
            Instance = this;
            LogDebug("[Session] LoadData initialized.");

            if (!_injectComplete)
            {
                _injectComplete = true;
                InjectIntoCharacterMenu();
            }
        }

        public override void BeforeStart()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
                return;

            SetUpdateOrder(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation);

            // Initialize Rich Hud Framework
            if (!RichHudClient.Registered)
            {
                RichHudClient.Init(ModParameter.MODNAME, HudInit, ClientReset);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (!isInit) return;
            HandleClientUpdates();
        }

        public override void UpdateAfterSimulation()
        {
            // Block selection is a purely client-side action; abort if we are on a Dedicated Server
            if (MyAPIGateway.Utilities.IsDedicated)
                return;

            var character = MyAPIGateway.Session?.Player?.Character;
            if (character == null)
                return;

            // 1. Check the general state of the builder
            var builder = MyCubeBuilder.Static;
            bool isBuilderActive = builder != null && builder.IsActivated;

            // Debug Logging for Builder State Changes
            string currentBuilderBlock = builder?.CurrentBlockDefinition?.Id.SubtypeId.ToString() ?? "None";
            if (isBuilderActive != _lastLoggedBuilderState || currentBuilderBlock != _lastLoggedBuilderBlock)
            {
                LogDebug($"[Update] CubeBuilder State Changed: Active={isBuilderActive}, CurrentBlock={currentBuilderBlock}");
                _lastLoggedBuilderState = isBuilderActive;
                _lastLoggedBuilderBlock = currentBuilderBlock;
            }

            bool isOurBlock = isBuilderActive && builder.CurrentBlockDefinition?.Id.SubtypeId == _targetSubtypeId;

            // 2. ONLY track the player's held tool if they don't have the block builder open!
            // This attempts to prevent the transitional "empty hands" animation from erasing our memory.
            if (!isBuilderActive)
            {
                EquippedToolType currentlyDetectedTool = EquippedToolType.None;
                var equippedTool = character?.EquippedTool as IMyHandheldGunObject<MyDeviceBase>;

                if (equippedTool != null)
                {
                    string subtype = equippedTool.DefinitionId.SubtypeName;

                    if (subtype.Contains("Grinder"))               currentlyDetectedTool = EquippedToolType.Grinder;
                    else if (subtype.Contains("Welder"))           currentlyDetectedTool = EquippedToolType.Welder;
                    else if (subtype.Contains("Drill"))            currentlyDetectedTool = EquippedToolType.Drill;
                    else if (subtype.Contains("Pistol") || subtype.Contains("Flare"))  currentlyDetectedTool = EquippedToolType.Pistol;
                    else if (subtype.Contains("Rifle"))            currentlyDetectedTool = EquippedToolType.Rifle;
                    else if (subtype.Contains("HandHeldLauncher")) currentlyDetectedTool = EquippedToolType.Launcher;
                    else if (subtype.Contains("PaintGun"))         currentlyDetectedTool = EquippedToolType.PaintGun;
                }

                // If the tool in their hands actually changed, update our trackers and log it
                if (currentlyDetectedTool != _lastEquippedTool)
                {
                    LogDebug($"[Update] Player hands changed: {_lastEquippedTool} -> {currentlyDetectedTool}");
                    _lastEquippedTool = currentlyDetectedTool;

                    // Keep cycle memory updated to whatever valid tool they just held
                    if (_lastEquippedTool != EquippedToolType.None)
                    {
                        _cycleMemoryTool = _lastEquippedTool;
                        LogDebug($"[Update] Cycle Memory updated to: {_cycleMemoryTool}");
                    }
                }
            }

            // 3. Handle State Changes for Our Specific Block
            if (isOurBlock && !_isOurBlockEquipped)
            {
                LogDebug($"[Update] Triggering OnBlockSelected!");
                _isOurBlockEquipped = true;
                OnBlockSelected();
            }
            else if (!isOurBlock && _isOurBlockEquipped)
            {
                LogDebug($"[Update] Our block was deselected.");
                _isOurBlockEquipped = false;
            }
        }

        #region RHF stuff

        private void HudInit()
        {
            _keyBinds = BindManager.GetOrCreateGroup("Key Binds");
            _keyBindsHidden = BindManager.GetOrCreateGroup("HiddenKeyBinds");
            _keyBindsHidden.RegisterBinds(new BindGroupInitializer()
            {
                { "close", MyKeys.Escape },
                { "shift", MyKeys.Shift },
                { "ctrl", MyKeys.Control },
            });

            RichHudTerminal.Root.Enabled = true;
            RichHudTerminal.Root.AddRange(new IModRootMember[]
            {
                _bindsPage = new RebindPage()
                {
                    Name = "Key bindings",
                    GroupContainer = {}
                },
                _settingsPage = new ControlPage()
                {
                    Name = "Settings",
                    Enabled = true
                }
            });

            LogDebug($"RichHudClient initialized for {DebugName}.");

            InitOnce();
        }

        private void ClientReset()
        {
            LogDebug($"RichHudClient is unloading / resetting for {DebugName}.");
        }

        private void InitOnce()
        {
            isInit = true;

            var defaultKeyBinds = new BindGroupInitializer
            {
                { "Cycle Tool", MyKeys.Control, MyKeys.T } // Empty keybind - user must configure
            }.GetBindDefinitions();

            Settings.UserConfigKeyBinds.AddArray(defaultKeyBinds);

            LoadUserConfigSettings();
            SaveUserConfigSettings("InitOnce - after loading user settings");

            IsPaintGunInstalled = IsModDetected(500818376, "Paint Gun");
            LogDebug($"[Init] Paint Gun mod installed: {IsPaintGunInstalled}");

            _toolWheel = new ToolWheelMenu(HudMain.HighDpiRoot);
            _toolWheel.SelectionConfirmed   += (t, id) => OnToolWheelConfirmed(t, id);
            _toolWheel.SelectionCancelled   += OnToolWheelCancelled;
            _toolWheel.SelectionUnavailable += OnToolWheelUnavailable;
            _toolWheel.WheelClosed          += () => { _wheelOpenedViaBlock = false; };

            CachePhysicalGunDisplayNames();
            ApplyCursorSensitivity();
            SetupKeyBinds();
            SetupSettingsPage();

            LogDebug($"EasyToolSwap_Session initialized for {DebugName}.");
        }

        private void ApplyCursorSensitivity()
        {
            if (_toolWheel == null) return;

            switch (Settings.WheelCursorSensitivity)
            {
                case UserConfigSettings.CursorSensitivityLevel.Low:    _toolWheel.CursorSensitivity = 0.35f; break;
                case UserConfigSettings.CursorSensitivityLevel.High:   _toolWheel.CursorSensitivity = 0.55f; break;
                default:                                                _toolWheel.CursorSensitivity = 0.4f;  break;
            }

            LogDebug($"[Settings] CursorSensitivity applied: {_toolWheel.CursorSensitivity}");
        }

        private void SetupKeyBinds()
        {
            _keyBinds.RegisterBinds(Settings.UserConfigKeyBinds);
            _bindsPage.Add(_keyBinds);

            _keyBinds.GetBind("Cycle Tool").NewPressed += (s, a) =>
            {
                OnCycleToolKeybind();
            };
        }

        private void SetupSettingsPage()
        {
            var generalCategory = new ControlCategory()
            {
                HeaderText = "General",
                SubheaderText = "Behaviour settings for Easy Tool Swap",
            };

            var tile = new ControlTile();

            var holdToKeepOpenToggle = new TerminalOnOffButton()
            {
                Name = "Hold to Keep Open",
                Value = Settings.HoldToKeepOpen,
                Enabled = true,
                ToolTip = new ToolTip()
                {
                    text = new RichText(
                        "When ON: hold the keybind to keep the wheel open; release to confirm.\n" +
                        "When OFF: the wheel stays open until you left-click to confirm or right-click to cancel.\n\n" +
                        "Default: ON",
                        ToolTip.DefaultText
                    )
                }
            };

            holdToKeepOpenToggle.ControlChanged += (sender, args) =>
            {
                Settings.HoldToKeepOpen = holdToKeepOpenToggle.Value;
                SaveUserConfigSettings("HoldToKeepOpen changed");
                LogDebug($"[Settings] HoldToKeepOpen set to {Settings.HoldToKeepOpen}");
            };

            var sensitivityDropdown = new TerminalDropdown<UserConfigSettings.CursorSensitivityLevel>()
            {
                Name = "Wheel Cursor Sensitivity",
                Enabled = true,
                ToolTip = new ToolTip()
                {
                    text = new RichText(
                        "Controls how quickly the wheel responds to mouse movement.\n" +
                        "Low = 0.35 | Medium = 0.4 | High = 0.55\n\n" +
                        "Default: Medium",
                        ToolTip.DefaultText
                    )
                }
            };
            sensitivityDropdown.List.Add("Low",    UserConfigSettings.CursorSensitivityLevel.Low);
            sensitivityDropdown.List.Add("Medium", UserConfigSettings.CursorSensitivityLevel.Medium);
            sensitivityDropdown.List.Add("High",   UserConfigSettings.CursorSensitivityLevel.High);
            sensitivityDropdown.List.SetSelection(Settings.WheelCursorSensitivity);

            sensitivityDropdown.ControlChanged += (sender, args) =>
            {
                if (sensitivityDropdown.Value == null) return;
                Settings.WheelCursorSensitivity = sensitivityDropdown.Value.AssocObject;
                ApplyCursorSensitivity();
                SaveUserConfigSettings("WheelCursorSensitivity changed");
                LogDebug($"[Settings] WheelCursorSensitivity set to {Settings.WheelCursorSensitivity}");
            };

            tile.Add(holdToKeepOpenToggle);
            tile.Add(sensitivityDropdown);
            generalCategory.Add(tile);
            _settingsPage.Add(generalCategory);
        }

        private void HandleClientUpdates()
        {
            // Mouse-wheel scrolling while wheel is open
            if (_toolWheel != null && _toolWheel.Visible)
            {

                // When HoldToKeepOpen is true, releasing the trigger key auto-confirms.
                // Keybind path: watch the bound key. Block path: watch the toolbar number key.
                if (Settings.HoldToKeepOpen)
                {
                    bool triggerReleased;
                    if (_wheelOpenedViaBlock)
                    {
                        // No number key was detected (e.g. mouse click on toolbar) — require a click.
                        triggerReleased = _blockTriggerKey != MyKeys.None
                            && !MyAPIGateway.Input.IsKeyPress(_blockTriggerKey);
                    }
                    else
                    {
                        var bind = _keyBinds?.GetBind("Cycle Tool");
                        triggerReleased = bind != null && !bind.IsPressed;
                    }

                    if (triggerReleased)
                        _toolWheel.NotifyKeyReleased();
                }
            }

            if (RichHudTerminal.Open)
                _terminalOpen = true;

            if (_terminalOpen && --_tickSettingsUpdate <= 0)
            {
                _tickSettingsUpdate = 600;
                _terminalOpen = false;

                if (_bindsPage.BindGroups.Count == 0)
                {
                    Log.Error("HandleClientUpdates: No BindGroups found in RebindPage.");
                    return;
                }

                Settings.UserConfigKeyBinds = _bindsPage.BindGroups[0]
                    .GetBindDefinitions()
                    .ToList();

                SaveUserConfigSettings("Sync");
            }
        }

        #endregion

        #region User Config Settings

        private void LoadUserConfigSettings()
        {
            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(USERCONFIGFILENAME))
                {
                    LogDebug("No UserConfigSettings found, creating with defaults.");
                    SaveUserConfigSettings("Creating with defaults from LoadUserConfigSettings");
                    return;
                }

                using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(USERCONFIGFILENAME))
                {
                    var xml = reader.ReadToEnd();
                    var loaded = MyAPIGateway.Utilities.SerializeFromXML<UserConfigSettings>(xml);

                    if (loaded == null)
                    {
                    LogDebug("UserConfigSettings file exists but failed to deserialize. Rewriting defaults.");
                        SaveUserConfigSettings("Sanitizing from LoadUserConfigSettings");
                        return;
                    }

                    if (loaded.UserConfigKeyBinds != null && loaded.UserConfigKeyBinds.Count > 0)
                    {
                        var tempSettings = Settings.UserConfigKeyBinds;
                        foreach (var def in loaded.UserConfigKeyBinds)
                        {
                            int idx = tempSettings.FindIndex(d => d.name == def.name);
                            if (idx != -1)
                            {
                    LogDebug($"Updating keybind {def.name} from {string.Join(" + ", Settings.UserConfigKeyBinds[idx].controlNames)} to {string.Join(" + ", def.controlNames)}");
                                tempSettings[idx] = def;
                            }
                        }
                        Settings.UserConfigKeyBinds = tempSettings;
                    }

                    Settings.HoldToKeepOpen          = loaded.HoldToKeepOpen;
                    Settings.WheelCursorSensitivity   = loaded.WheelCursorSensitivity;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"LoadUserConfigSettings: Error loading UserConfigSettings: {ex}");
            }
        }

        private void SaveUserConfigSettings(string source)
        {
            try
            {
                var xml = MyAPIGateway.Utilities.SerializeToXML(Settings);

                using (var writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(USERCONFIGFILENAME))
                {
                    writer.Write(xml);
                }
                LogDebug($"Source: {source}. Saved UserConfigSettings to global storage: {USERCONFIGFILENAME}");
            }
            catch (Exception ex)
            {
                Log.Error($"SaveUserConfigSettings: Error saving UserConfigSettings: {ex}");
            }
        }

        #endregion

        private void OnBlockSelected()
        {
            LogDebug("=== OnBlockSelected: Delegating to OnCycleToolKeybind ===");

            // Force the player to put the block away before doing anything else.
            MyCubeBuilder.Static?.Deactivate();
            MyCubeBuilder.Static?.DeactivateBlockCreation();
            MyAPIGateway.CubeBuilder.Deactivate();
            MyAPIGateway.CubeBuilder.DeactivateBlockCreation();

            // Detect which toolbar number key the player is holding right now.
            _blockTriggerKey = MyKeys.None;
            foreach (var key in ToolbarKeys)
            {
                if (MyAPIGateway.Input.IsKeyPress(key))
                {
                    _blockTriggerKey = key;
                    break;
                }
            }
            LogDebug($"[OnBlockSelected] Detected trigger key: {_blockTriggerKey}");

            // Open the wheel.
            _wheelOpenedViaBlock = true;
            OnCycleToolKeybind();
        }

        private int GetToolTier(string subtypeName)
        {
            // Vanilla tiers typically use numbers 2, 3, 4 in the SubtypeId
            if (subtypeName.Contains("4")) return 4;
            if (subtypeName.Contains("3")) return 3;
            if (subtypeName.Contains("2")) return 2;

            // Catch for modded tools that might use descriptive words instead of numbers
            if (subtypeName.IndexOf("Elite", StringComparison.OrdinalIgnoreCase) >= 0) return 4;
            if (subtypeName.IndexOf("Proficient", StringComparison.OrdinalIgnoreCase) >= 0) return 3;
            if (subtypeName.IndexOf("Enhanced", StringComparison.OrdinalIgnoreCase) >= 0) return 2;

            // Default base tier (e.g., WelderItem)
            return 1;
        }

        private MyDefinitionId? FindBestToolInInventory(IMyInventory inventory, EquippedToolType targetType)
        {
            string[] searchTerms = GetSearchTerms(targetType);
            if (searchTerms == null || searchTerms.Length == 0) return null;

            // 1. Clear the cache
            _inventoryCache.Clear();

            // 2. Pass our pre-allocated list to prevent memory allocation
            inventory.GetItems(_inventoryCache);

            MyDefinitionId? bestTool = null;
            int highestTierFound = 0;

            // 3. Iterate over our cache to find the HIGHEST tier matching tool
            foreach (var item in _inventoryCache)
            {
                // Explicitly cast the lightweight Type to a MyDefinitionId
                MyDefinitionId itemDefId = (MyDefinitionId)item.Type;

                if (!itemDefId.TypeId.ToString().Contains("PhysicalGunObject")) continue;

                bool matched = false;
                foreach (var term in searchTerms)
                {
                    if (itemDefId.SubtypeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    { matched = true; break; }
                }
                if (!matched) continue;

                int currentToolTier = GetToolTier(itemDefId.SubtypeName);
                if (currentToolTier > highestTierFound)
                {
                    highestTierFound = currentToolTier;
                    bestTool = itemDefId;
                }
            }

            // 4. Clear the list when done
            _inventoryCache.Clear();

            return bestTool;
        }

        private void FindAllVariantsInInventory(IMyInventory inventory, EquippedToolType targetType, System.Collections.Generic.List<MyDefinitionId> results)
        {
            string[] searchTerms = GetSearchTerms(targetType);
            if (searchTerms == null || searchTerms.Length == 0) return;

            _inventoryCache.Clear();
            inventory.GetItems(_inventoryCache);

            foreach (var item in _inventoryCache)
            {
                MyDefinitionId itemDefId = (MyDefinitionId)item.Type;
                if (!itemDefId.TypeId.ToString().Contains("PhysicalGunObject")) continue;

                foreach (var term in searchTerms)
                {
                    if (itemDefId.SubtypeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        results.Add(itemDefId);
                        break;
                    }
                }
            }

            _inventoryCache.Clear();
        }

        private static string[] GetSearchTerms(EquippedToolType targetType)
        {
            switch (targetType)
            {
                case EquippedToolType.Welder:   return new string[] { "Welder" };
                case EquippedToolType.Grinder:  return new string[] { "Grinder" };
                case EquippedToolType.Drill:    return new string[] { "Drill" };
                case EquippedToolType.Pistol:   return new string[] { "Pistol", "Flare" };
                case EquippedToolType.Rifle:    return new string[] { "Rifle" };
                case EquippedToolType.Launcher: return new string[] { "HandHeldLauncher" };
                case EquippedToolType.PaintGun: return new string[] { "PaintGun" };
                default:                        return null;
            }
        }

        /// <summary>
        /// Iterates all loaded physical item definitions and caches those that are
        /// PhysicalGunObjects (tools and weapons) mapped to their display names.
        /// Called once from <see cref="InitOnce"/> after definitions are guaranteed loaded.
        /// </summary>
        private void CachePhysicalGunDisplayNames()
        {
            GunDisplayNames.Clear();

            var physicalGunType = typeof(MyObjectBuilder_PhysicalGunObject);

            foreach (var def in MyDefinitionManager.Static.GetPhysicalItemDefinitions())
            {
                if (def == null || def.Id.TypeId != physicalGunType)
                    continue;

                string displayName = def.DisplayNameText;
                if (string.IsNullOrEmpty(displayName))
                    displayName = def.Id.SubtypeName;

                GunDisplayNames[def.Id] = displayName;
            }

            LogDebug($"[Init] Cached display names for {GunDisplayNames.Count} PhysicalGunObject definitions.");
        }

        protected override void UnloadData()
        {
            // Clean up your variables when the world is unloaded
            _isOurBlockEquipped = false;
            _lastEquippedTool = EquippedToolType.None;
            _inventoryCache.Clear(); // Just to be extra safe

            if (Instance == this)
                Instance = null;
        }

        private void OnCycleToolKeybind()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
                return;

            var character = MyAPIGateway.Session?.Player?.Character;
            if (character == null) return;

            if (_toolWheel == null || _toolWheel.Visible) return;

            var inventory = character.GetInventory() as IMyInventory;
            var available = new System.Collections.Generic.HashSet<EquippedToolType>();
            var welderVariants    = new System.Collections.Generic.List<MyDefinitionId>();
            var grinderVariants   = new System.Collections.Generic.List<MyDefinitionId>();
            var drillVariants     = new System.Collections.Generic.List<MyDefinitionId>();
            var pistolVariants    = new System.Collections.Generic.List<MyDefinitionId>();
            var rifleVariants     = new System.Collections.Generic.List<MyDefinitionId>();
            var launcherVariants  = new System.Collections.Generic.List<MyDefinitionId>();
            var paintGunVariants  = new System.Collections.Generic.List<MyDefinitionId>();

            if (inventory != null)
            {
                FindAllVariantsInInventory(inventory, EquippedToolType.Welder,   welderVariants);
                FindAllVariantsInInventory(inventory, EquippedToolType.Grinder,  grinderVariants);
                FindAllVariantsInInventory(inventory, EquippedToolType.Drill,    drillVariants);
                FindAllVariantsInInventory(inventory, EquippedToolType.Pistol,   pistolVariants);
                FindAllVariantsInInventory(inventory, EquippedToolType.Rifle,    rifleVariants);
                FindAllVariantsInInventory(inventory, EquippedToolType.Launcher, launcherVariants);
                if (IsPaintGunInstalled)
                    FindAllVariantsInInventory(inventory, EquippedToolType.PaintGun, paintGunVariants);

                // Sort tool variants highest-tier-first so index 0 is always the best tool.
                // Weapons are not sorted; the player's previous selection is preserved instead.
                welderVariants.Sort((a, b)    => GetToolTier(b.SubtypeName).CompareTo(GetToolTier(a.SubtypeName)));
                grinderVariants.Sort((a, b)   => GetToolTier(b.SubtypeName).CompareTo(GetToolTier(a.SubtypeName)));
                drillVariants.Sort((a, b)     => GetToolTier(b.SubtypeName).CompareTo(GetToolTier(a.SubtypeName)));
                paintGunVariants.Sort((a, b)  => GetToolTier(b.SubtypeName).CompareTo(GetToolTier(a.SubtypeName)));

                if (welderVariants.Count    > 0) available.Add(EquippedToolType.Welder);
                if (grinderVariants.Count   > 0) available.Add(EquippedToolType.Grinder);
                if (drillVariants.Count     > 0) available.Add(EquippedToolType.Drill);
                if (pistolVariants.Count    > 0) available.Add(EquippedToolType.Pistol);
                if (rifleVariants.Count     > 0) available.Add(EquippedToolType.Rifle);
                if (launcherVariants.Count  > 0) available.Add(EquippedToolType.Launcher);
                if (paintGunVariants.Count  > 0) available.Add(EquippedToolType.PaintGun);
            }

            _toolWheel.HoldToKeepOpen          = Settings.HoldToKeepOpen;
            _toolWheel.OpenedViaBlock            = _wheelOpenedViaBlock;
            _toolWheel.BlockTriggerKeyDetected   = _blockTriggerKey != MyKeys.None;
            _toolWheel.Open(available, welderVariants, grinderVariants, drillVariants, pistolVariants, rifleVariants, launcherVariants, paintGunVariants);
        }

        private void OnToolWheelConfirmed(EasyToolSwap_Session.EquippedToolType chosenTool, MyDefinitionId? specificId)
        {
            _wheelOpenedViaBlock = false;
            LogDebug($"[Wheel] Confirmed: {chosenTool}, SpecificId: {(specificId.HasValue ? specificId.Value.SubtypeName : "none")}");

            var character = MyAPIGateway.Session?.Player?.Character;
            if (character == null) return;

            var inventory  = character.GetInventory() as IMyInventory;
            var controller = character as Sandbox.Game.Entities.IMyControllableEntity;
            if (inventory == null || controller == null) return;

            MyDefinitionId? toolToEquip = specificId.HasValue ? specificId : FindBestToolInInventory(inventory, chosenTool);
            // Note: specificId is set by SelectedVariant for both tools and weapons when Q/E was used.

            if (toolToEquip.HasValue)
            {
                controller.SwitchToWeapon(toolToEquip.Value);
                _cycleMemoryTool = chosenTool;
                LogDebug($"[Wheel] Equipped {toolToEquip.Value.SubtypeName}. Memory={_cycleMemoryTool}");
            }
            else
            {
                controller.SwitchToWeapon(null);
                MyAPIGateway.Utilities.ShowNotification($"{chosenTool} not found in inventory!", 2000, MyFontEnum.Red);
                LogDebug($"[Wheel] {chosenTool} not in inventory.");
            }
        }

        private void OnToolWheelCancelled()
        {
            _wheelOpenedViaBlock = false;
            LogDebug("[Wheel] Cancelled.");
        }

        private void OnToolWheelUnavailable(string toolName)
        {
            _wheelOpenedViaBlock = false;
            MyAPIGateway.Utilities.ShowNotification($"{toolName} is not in your inventory!", 2500, MyFontEnum.Red);
            LogDebug($"[Wheel] Unavailable selection: {toolName}");
        }

        private void InjectIntoCharacterMenu()
        {
            // 1. Get our block definition and force the settings Digi uses
            MyCubeBlockDefinition ourBlockDef;
            var blockDefId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "CycleToolsFakeBlock");

            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(blockDefId, out ourBlockDef))
            {
                ourBlockDef.Public = true;
                ourBlockDef.GuiVisible = false; // Hides from normal tabs so it ONLY shows in tools
                LogDebug("[Session] Found and configured CycleToolsFakeBlock definition.");
            }
            else
            {
                LogDebug("[Session] ERROR: Could not find CycleToolsFakeBlock in definition manager!");
                return; // If it's not loaded, we can't inject it.
            }

            // 2. Fetch the categories using the exact internal keys Digi uses
            var categories = MyDefinitionManager.Static.GetCategories();
            var toolCategory = categories.GetValueOrDefault("Section0_Position2_CharacterTools");
            var itemCategory = categories.GetValueOrDefault("Section0_Position1_CharacterItems");

            // The exact string format required for this specific hybrid menu
            string injectString = "CubeBlock/CycleToolsFakeBlock";

            // 3. Inject into Character Tools
            if (toolCategory != null)
            {
                toolCategory.SearchBlocks = true; // Ensures it pops up in the search bar
                if (!toolCategory.ItemIds.Contains(injectString))
                {
                    toolCategory.ItemIds.Add(injectString);
                    LogDebug($"[Session] Successfully injected {injectString} into CharacterTools");
                }
            }
            else
            {
                LogDebug("[Session] ERROR: Could not find Section0_Position2_CharacterTools.");
            }

            // 4. (Optional) Inject into Character Items, just like Digi does
            if (itemCategory != null)
            {
                itemCategory.SearchBlocks = true;
                if (!itemCategory.ItemIds.Contains(injectString))
                {
                    itemCategory.ItemIds.Add(injectString);
                    LogDebug($"[Session] Successfully injected {injectString} into CharacterItems");
                }
            }
        }
    }
}