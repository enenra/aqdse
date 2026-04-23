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
        private readonly MyStringHash _targetSubtypeId = MyStringHash.GetOrCompute("AQD_LG_EasyToolAccessFakeBlock");

        public enum EquippedToolType
        {
            None = 0,
            Welder = 1,
            Grinder = 2,
            Drill = 3,
            Pistol = 4,
            Rifle = 5,
            Launcher = 6,
            PaintGun = 7,
            ConcreteTool = 8,
            Binoculars = 9,
            HandScanner = 10,
            BuildInfoMultiTool = 11,
            TerrainTool = 12,
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

        /// <summary>
        /// True when the Concrete Tool mod (Workshop ID 396679430) is active in the current session.
        /// Populated during <see cref="InitOnce"/>.
        /// </summary>
        public static bool IsConcreteToolInstalled { get; private set; }

        /// <summary>
        /// True when the Terrain Tool mod (Workshop ID 2820001381) is active in the current session.
        /// Populated during <see cref="InitOnce"/>.
        /// </summary>
        public static bool IsTerrainToolInstalled { get; private set; }

        /// <summary>
        /// True when the Binoculars mod (Workshop ID 2777644246) is active in the current session.
        /// Populated during <see cref="InitOnce"/>.
        /// </summary>
        public static bool IsBinocularsInstalled { get; private set; }

        // Mod metadata
        private readonly string USERCONFIGFILENAME = ModParameter.MODNAME + "_Config.xml";
        public readonly UserConfigSettings Settings = new UserConfigSettings();

        // RHF controls
        private IBindGroup _keyBinds, _keyBindsHidden;
        private RebindPage _bindsPage;
        private ControlPage _generalSettingsPage;
        private ControlPage _handToolsPage;
        private ControlPage _weaponsPage;
        private ControlPage _moddedToolsPage;
        private ToolWheelMenu _toolWheel;

        // Dictionary to track all dynamically generated dropdowns across all pages
        private readonly Dictionary<TerminalDropdown<int>, EquippedToolType> _allSlotDropdowns = new Dictionary<TerminalDropdown<int>, EquippedToolType>();

        // Tracks whether InitOnce has been called
        private bool isInit = false;

        // Tracks whether the RHF terminal was open so we can sync keybinds after it closes
        private bool _terminalOpen = false;
        private int _tickSettingsUpdate = 0;

        // --- DEBUG TRACKING VARIABLES (Prevents Log Spam) ---
        private bool _lastLoggedBuilderState = false;
        private string _lastLoggedBuilderBlock = "";

        private bool _injectComplete = false; // Used to track if we've done our injection yet, to prevent multiple attempts

        // Buffers log messages produced before BeforeStart() runs (e.g. LoadData, InjectIntoCharacterMenu),
        // where RHF / Log may not yet be fully ready. Flushed and cleared in BeforeStart().
        private readonly List<string> _earlyLogBuffer = new List<string>();

        // When true the wheel was opened by the fake block rather than the keybind.
        private bool _wheelOpenedViaBlock = false;

        // The toolbar number key that was held when the fake block fired, or None if not found.
        // Used to detect key-release for HoldToKeepOpen auto-confirm via the block path.
        private MyKeys _blockTriggerKey = MyKeys.None;

        // Current page shown by the wheel (0 = page 1, 1 = page 2).
        private int _currentWheelPage = 0;

        // Full pre-built selection list for both pages, rebuilt each time the wheel opens.
        private List<WeaponSelectionData> _allSelections = new List<WeaponSelectionData>();

        /// <summary>
        /// Per-type overrides for tool types that are not PhysicalGunObjects in the player's
        /// inventory (e.g. fake CubeBlock tools like the Build Info Multitool).
        /// Registered in <see cref="InitOnce"/> when the relevant mod is detected.
        /// </summary>
        private class ToolTypeOverride
        {
            /// <summary>Returns true when this tool should appear as available on the wheel.</summary>
            public Func<bool> IsAvailable;

            /// <summary>
            /// Called instead of <c>SwitchToWeapon</c> when the player confirms this slot.
            /// Leave null to use the normal inventory-based equip path.
            /// </summary>
            public Action Equip;
        }

        private readonly Dictionary<EquippedToolType, ToolTypeOverride> _toolOverrides
            = new Dictionary<EquippedToolType, ToolTypeOverride>();

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
            _earlyLogBuffer.Add("[Session] LoadData initialized.");

            if (!_injectComplete)
            {
                _injectComplete = true;
                InjectIntoCharacterMenu();
            }
        }

        public override void BeforeStart()
        {
            foreach (var msg in _earlyLogBuffer)
                LogDebug(msg);
            _earlyLogBuffer.Clear();

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

                    if (subtype.Contains("Grinder")) currentlyDetectedTool = EquippedToolType.Grinder;
                    else if (subtype.Contains("Welder")) currentlyDetectedTool = EquippedToolType.Welder;
                    else if (subtype.Contains("Drill")) currentlyDetectedTool = EquippedToolType.Drill;
                    else if (subtype.Contains("Pistol") || subtype.Contains("Flare")) currentlyDetectedTool = EquippedToolType.Pistol;
                    else if (subtype.Contains("Rifle")) currentlyDetectedTool = EquippedToolType.Rifle;
                    else if (subtype.Contains("HandHeldLauncher")) currentlyDetectedTool = EquippedToolType.Launcher;
                    else if (subtype.Contains("PaintGun")) currentlyDetectedTool = EquippedToolType.PaintGun;
                    else if (subtype.Contains("TerrainTool")) currentlyDetectedTool = EquippedToolType.TerrainTool;
                    else if (subtype.Contains("Binoculars")) currentlyDetectedTool = EquippedToolType.Binoculars;
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
                _generalSettingsPage = new ControlPage()
                {
                    Name = "General Settings",
                    Enabled = true
                },
                _handToolsPage = new ControlPage()
                {
                    Name = "Slots: Hand Tools",
                    Enabled = true
                },
                _weaponsPage = new ControlPage()
                {
                    Name = "Slots: Weapons",
                    Enabled = true
                },
                _moddedToolsPage = new ControlPage()
                {
                    Name = "Slots: Modded Tools",
                    Enabled = true
                }
            });

            LogDebug($"RichHudClient initialized for {DebugName}.");

            InitOnce();
        }

        private void ClientReset()
        {
            LogDebug($"RichHudClient is unloading / resetting for {DebugName}.");
            if (_toolWheel != null)
            {
                _toolWheel.SelectionConfirmed -= OnToolWheelConfirmed;
                _toolWheel.SelectionCancelled -= OnToolWheelCancelled;
                _toolWheel.SelectionUnavailable -= OnToolWheelUnavailable;
                _toolWheel.WheelClosed -= OnWheelClosed;
                _toolWheel.PageFlipRequested -= OnWheelPageFlip;
                _toolWheel = null;
            }
            _allSlotDropdowns.Clear();
            isInit = false;
        }

        private void InitOnce()
        {
            var defaultKeyBinds = new BindGroupInitializer
            {
                { "Cycle Tool", MyKeys.Control, MyKeys.T } // Empty keybind - user must configure
            }.GetBindDefinitions();

            Settings.UserConfigKeyBinds.AddArray(defaultKeyBinds);

            LoadUserConfigSettings();

            IsPaintGunInstalled = IsModDetected(500818376, "Paint Gun");
            LogDebug($"[Init] Paint Gun mod installed: {IsPaintGunInstalled}");

            IsConcreteToolInstalled = IsModDetected(396679430, "Concrete Tool");
            LogDebug($"[Init] Concrete Tool mod installed: {IsConcreteToolInstalled}");

            IsTerrainToolInstalled = IsModDetected(2820001381, "Terrain Tool");
            LogDebug($"[Init] Terrain Tool mod installed: {IsTerrainToolInstalled}");

            IsBinocularsInstalled = IsModDetected(2777644246, "Binoculars");
            LogDebug($"[Init] Binoculars mod installed: {IsBinocularsInstalled}");

            bool isBuildInfoInstalled = IsModDetected(514062285, "Build Info");
            LogDebug($"[Init] Build Info mod installed: {isBuildInfoInstalled}");
            if (isBuildInfoInstalled)
            {
                _toolOverrides[EquippedToolType.BuildInfoMultiTool] = new ToolTypeOverride
                {
                    IsAvailable = () => true,
                    Equip = ActivateBuildInfoMultiTool,
                };
            }

            _toolWheel = new ToolWheelMenu(HudMain.HighDpiRoot);
            _toolWheel.SelectionConfirmed += (t, id) => OnToolWheelConfirmed(t, id);
            _toolWheel.SelectionCancelled += OnToolWheelCancelled;
            _toolWheel.SelectionUnavailable += OnToolWheelUnavailable;
            _toolWheel.WheelClosed += OnWheelClosed;
            _toolWheel.PageFlipRequested += OnWheelPageFlip;

            CachePhysicalGunDisplayNames();
            ApplyCursorSensitivity();
            SetupKeyBinds();
            SetupSettingsPage();
            SetupWheelSlotsSettingsPage();

            // ONLY set this to true if we survived the entire setup process without throwing!
            isInit = true;
            LogDebug($"EasyToolSwap_Session initialized for {DebugName}.");
        }

        private void ApplyCursorSensitivity()
        {
            if (_toolWheel == null) return;

            switch (Settings.WheelCursorSensitivity)
            {
                case UserConfigSettings.CursorSensitivityLevel.Low:
                    _toolWheel.CursorSensitivity = 0.35f;
                    _toolWheel.AngleSelectionThreshold = 900;  // ~30 px
                    break;
                case UserConfigSettings.CursorSensitivityLevel.High:
                    _toolWheel.CursorSensitivity = 0.55f;
                    _toolWheel.AngleSelectionThreshold = 400;  // ~20 px
                    break;
                default:
                    _toolWheel.CursorSensitivity = 0.4f;
                    _toolWheel.AngleSelectionThreshold = 144f;  // ~12 px
                    break;
            }

            LogDebug($"[Settings] CursorSensitivity applied: {_toolWheel.CursorSensitivity}, AngleThreshold: {_toolWheel.AngleSelectionThreshold}");
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
                SubheaderText = "Behaviour settings for Easy Tool Access",
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
                        "Controls how far you need to move the mouse to switch between wheel slices.\n" +
                        "Low  = large movements required (more stable, less accidental switching).\n" +
                        "High = small movements required (snappier, but easier to mis-select).\n\n" +
                        "Default: Medium",
                        ToolTip.DefaultText
                    )
                }
            };
            sensitivityDropdown.List.Add("Low", UserConfigSettings.CursorSensitivityLevel.Low);
            sensitivityDropdown.List.Add("Medium", UserConfigSettings.CursorSensitivityLevel.Medium);
            sensitivityDropdown.List.Add("High", UserConfigSettings.CursorSensitivityLevel.High);
            sensitivityDropdown.List.SetSelection(Settings.WheelCursorSensitivity);

            sensitivityDropdown.ControlChanged += (sender, args) =>
            {
                if (sensitivityDropdown.Value == null) return;
                Settings.WheelCursorSensitivity = sensitivityDropdown.Value.AssocObject;
                ApplyCursorSensitivity();
                SaveUserConfigSettings("WheelCursorSensitivity changed");
                LogDebug($"[Settings] WheelCursorSensitivity set to {Settings.WheelCursorSensitivity}");
            };

            var selectionModeDropdown = new TerminalDropdown<UserConfigSettings.WheelSelectionMode>()
            {
                Name = "Wheel Selection Mode",
                Enabled = true,
                ToolTip = new ToolTip()
                {
                    text = new RichText(
                        "Directional Flick: move the mouse in a direction to select a slice (custom angle math).\n" +
                        "Cursor Tracking: move the cursor over a slice to select it (RHF native math).\n\n" +
                        "Default: Directional Flick",
                        ToolTip.DefaultText
                    )
                }
            };
            selectionModeDropdown.List.Add("Directional Flick", UserConfigSettings.WheelSelectionMode.DirectionalFlick);
            selectionModeDropdown.List.Add("Cursor Tracking", UserConfigSettings.WheelSelectionMode.CursorTracking);
            selectionModeDropdown.List.SetSelection((int)Settings.SelectionMode);

            selectionModeDropdown.ControlChanged += (sender, args) =>
            {
                if (selectionModeDropdown.Value == null) return;
                Settings.SelectionMode = selectionModeDropdown.Value.AssocObject;
                SaveUserConfigSettings("SelectionMode changed");
                LogDebug($"[Settings] SelectionMode set to {Settings.SelectionMode}");

                if (Settings.SelectionMode == UserConfigSettings.WheelSelectionMode.DirectionalFlick)
                {
                    sensitivityDropdown.Name = "Flick Sensitivity";
                    sensitivityDropdown.ToolTip.text = new RichText(
                        "Controls the mouse distance required to register a flick.\n" +
                        "Low = Requires a large, deliberate swipe.\n" +
                        "High = Registers with very small micro-movements.",
                        ToolTip.DefaultText
                    );
                }
                else
                {
                    sensitivityDropdown.Name = "Cursor Tracking Speed";
                    sensitivityDropdown.ToolTip.text = new RichText(
                        "Controls the speed of the virtual cursor on the wheel.\n" +
                        "Low = Cursor moves slowly, requiring more physical mouse movement.\n" +
                        "High = Cursor moves quickly across the wheel slices.",
                        ToolTip.DefaultText
                    );
                }
            };

            // Force an initial trigger to set the correct text when the menu first loads
            selectionModeDropdown.List.SetSelection((int)Settings.SelectionMode);

            var tile2 = new ControlTile();

            tile.Add(holdToKeepOpenToggle);
            tile2.Add(selectionModeDropdown);
            tile2.Add(sensitivityDropdown);
            generalCategory.Add(tile);
            generalCategory.Add(tile2);
            _generalSettingsPage.Add(generalCategory);
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
                var defaultSlots = new List<SlotConfig>
                {
                    new SlotConfig(0, "", EquippedToolType.Grinder),  // Visually: Page 1 - Slot 1
                    new SlotConfig(1, "", EquippedToolType.Welder),   // Visually: Page 1 - Slot 2
                    new SlotConfig(2, "", EquippedToolType.Drill),    // Visually: Page 1 - Slot 3
                    new SlotConfig(3, "", EquippedToolType.PaintGun), // Visually: Page 1 - Slot 4
                    new SlotConfig(4, "", EquippedToolType.Pistol),   // Visually: Page 1 - Slot 5
                    new SlotConfig(5, "", EquippedToolType.Rifle),    // Visually: Page 1 - Slot 6
                    new SlotConfig(6, "", EquippedToolType.Launcher), // Visually: Page 1 - Slot 7
                };

                if (!MyAPIGateway.Utilities.FileExistsInGlobalStorage(USERCONFIGFILENAME))
                {
                    LogDebug("No UserConfigSettings found, creating with defaults.");
                    Settings.WheelSlots = defaultSlots;
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
                        Settings.WheelSlots = defaultSlots;
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

                    Settings.HoldToKeepOpen = loaded.HoldToKeepOpen;
                    Settings.SelectionMode = loaded.SelectionMode;
                    Settings.WheelCursorSensitivity = loaded.WheelCursorSensitivity;

                    Settings.WheelSlots = (loaded.WheelSlots != null && loaded.WheelSlots.Count > 0)
                        ? loaded.WheelSlots
                        : defaultSlots;
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
                // Clean up and structure the order before serializing to XML
                if (Settings.WheelSlots != null)
                {
                    // 1. Sort the slots numerically (0, 1, 2, 3...)
                    Settings.WheelSlots.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

                    // 2. Sort the tools inside each slot alphabetically
                    foreach (var slot in Settings.WheelSlots)
                    {
                        if (slot.AssignedTypes != null)
                        {
                            slot.AssignedTypes.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }

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
                case EquippedToolType.Welder: return new string[] { "Welder" };
                case EquippedToolType.Grinder: return new string[] { "Grinder" };
                case EquippedToolType.Drill: return new string[] { "Drill" };
                case EquippedToolType.Pistol: return new string[] { "Pistol", "Flare" };
                case EquippedToolType.Rifle: return new string[] { "Rifle" };
                case EquippedToolType.Launcher: return new string[] { "HandHeldLauncher" };
                case EquippedToolType.PaintGun: return new string[] { "PaintGun" };
                case EquippedToolType.ConcreteTool: return new string[] { "ConcreteTool" };
                case EquippedToolType.Binoculars: return new string[] { "Binoculars" };
                case EquippedToolType.HandScanner: return new string[] { "HandDrill", "Scanner" };
                case EquippedToolType.TerrainTool: return new string[] { "TerrainTool" };
                default: return null;
            }
        }

        private bool IsToolInstalled(EquippedToolType type)
        {
            switch (type)
            {
                case EquippedToolType.PaintGun: return IsPaintGunInstalled;
                case EquippedToolType.ConcreteTool: return IsConcreteToolInstalled;
                case EquippedToolType.TerrainTool: return IsTerrainToolInstalled;
                case EquippedToolType.Binoculars: return IsBinocularsInstalled;
                case EquippedToolType.BuildInfoMultiTool: return _toolOverrides.ContainsKey(EquippedToolType.BuildInfoMultiTool);
                default: return true; // Vanilla tools are always considered installed
            }
        }

        private void RefreshAllDropdownVisuals()
        {
            foreach (var kvp in _allSlotDropdowns)
            {
                var dropdown = kvp.Key;
                var type = kvp.Value;

                int currentSlot = -1;
                foreach (var cfg in Settings.WheelSlots)
                {
                    if (cfg.AssignedTypes != null && cfg.AssignedTypes.Contains(type))
                    {
                        currentSlot = cfg.SlotIndex;
                        break;
                    }
                }

                bool isInstalled = IsToolInstalled(type);
                string baseName = GetToolDisplayName(type);

                // --- Name Logic ---
                if (!isInstalled)
                    dropdown.Name = baseName + " (Not installed)";
                else if (currentSlot == -1)
                    dropdown.Name = baseName + " (Unassigned)";
                else
                    dropdown.Name = baseName;

                // --- Tooltip Logic ---
                if (!isInstalled)
                {
                    dropdown.ToolTip.text = new RichText("This mod is not loaded in the current world.");
                }
                else if (currentSlot == -1)
                {
                    dropdown.ToolTip.text = new RichText("Tool is currently unassigned.");
                }
                else
                {
                    var sharedTools = new List<string>();
                    var slotCfg = Settings.WheelSlots.Find(s => s.SlotIndex == currentSlot);

                    if (slotCfg != null && slotCfg.AssignedTypes != null)
                    {
                        foreach (var sharedType in slotCfg.AssignedTypes)
                        {
                            if (sharedType == type) continue;

                            string sharedName = GetToolDisplayName(sharedType);
                            if (!IsToolInstalled(sharedType))
                                sharedName += " (Not installed)";

                            sharedTools.Add(sharedName);
                        }
                    }

                    if (sharedTools.Count > 0)
                        dropdown.ToolTip.text = new RichText("Shared with:\n- " + string.Join("\n- ", sharedTools));
                    else
                        dropdown.ToolTip.text = new RichText("No other tools in this slot.");
                }
            }
        }

        /// <summary>
        /// Adds a "Wheel Slots" category to the RHF settings page.
        /// One dropdown per tool type — the player chooses which slot (0-6 on page 1,
        /// 7-13 on page 2) to place it on, or "None" to leave it unassigned.
        /// Multiple types may share the same slot; they are cycled with the scroll wheel
        /// alongside the normal per-tool sub-variants.
        /// </summary>
        private void SetupWheelSlotsSettingsPage()
        {
            // --- PAGE 1: VANILLA HAND TOOLS ---
            var handTools = new EquippedToolType[]
            {
                EquippedToolType.Welder, EquippedToolType.Grinder,
                EquippedToolType.Drill
            };
            BuildDropdownsForCategory(_handToolsPage, "Vanilla Hand Tools",
                "Stacked tools cycle via scroll wheel.", handTools);

            // --- PAGE 2: WEAPONS ---
            var weapons = new EquippedToolType[]
            {
                EquippedToolType.Pistol, EquippedToolType.Rifle,
                EquippedToolType.Launcher
            };
            BuildDropdownsForCategory(_weaponsPage, "Weapons",
                "", weapons);

            // --- PAGE 3: MODDED TOOLS & OPTICS ---
            var moddedTools = new EquippedToolType[]
            {
                EquippedToolType.PaintGun,      EquippedToolType.ConcreteTool,
                EquippedToolType.TerrainTool,   EquippedToolType.Binoculars,
                EquippedToolType.BuildInfoMultiTool
            };
            BuildDropdownsForCategory(_moddedToolsPage, "Modded Tools",
                "Map tools added by other workshop mods.", moddedTools);

            // Setup complete, calculate the initial labels and tooltips
            RefreshAllDropdownVisuals();
        }

        private void BuildDropdownsForCategory(ControlPage page, string header, string subheader, EquippedToolType[] types)
        {
            ControlCategory currentCategory = null;

            for (int i = 0; i < types.Length; i++)
            {
                EquippedToolType capturedType = types[i];

                // Force a new "row" (Category) every 2 tiles.
                // Only the first row gets the actual header text; the rest are seamless continuations.
                if (i % 2 == 0)
                {
                    currentCategory = new ControlCategory()
                    {
                        HeaderText = (i == 0) ? header : "",
                        SubheaderText = (i == 0) ? subheader : ""
                    };
                    page.Add(currentCategory);
                }

                // Find current slot (-1 = None)
                int currentSlot = -1;
                foreach (var cfg in Settings.WheelSlots)
                {
                    if (cfg.AssignedTypes != null && cfg.AssignedTypes.Contains(capturedType))
                    {
                        currentSlot = cfg.SlotIndex;
                        break;
                    }
                }

                bool isInstalled = IsToolInstalled(capturedType);

                var slotDropdown = new TerminalDropdown<int>()
                {
                    Name = GetToolDisplayName(capturedType),
                    ToolTip = new ToolTip(),
                    Enabled = isInstalled // Disable the dropdown directly to dim the text if uninstalled
                };

                _allSlotDropdowns[slotDropdown] = capturedType;

                if (isInstalled)
                {
                    // Mod is installed: Populate all options normally
                    slotDropdown.List.Add("None", -1);

                    // Page 1 Slots (Indices 0 to 6)
                    for (int s = 0; s <= 6; s++)
                        slotDropdown.List.Add("Page 1 - Slot " + (s + 1), s);

                    // Page 2 Slots (Indices 7 to 13)
                    for (int s = 7; s <= 13; s++)
                        slotDropdown.List.Add("Page 2 - Slot " + (s - 6), s);

                    int listIndex = currentSlot >= 0 && currentSlot <= 13 ? currentSlot + 1 : 0;
                    slotDropdown.List.SetSelection(listIndex);
                }
                else
                {
                    // Mod is missing: Hard-lock the dropdown with a single "N/A" option
                    slotDropdown.List.Add("N/A", currentSlot);
                    slotDropdown.List.SetSelection(0);
                }

                slotDropdown.ControlChanged += (sender, args) =>
                {
                    // Bulletproof guard clause: abort if ghost click gets through the disabled tile
                    if (!IsToolInstalled(capturedType)) return;

                    if (slotDropdown.Value == null) return;
                    int chosenSlot = slotDropdown.Value.AssocObject;

                    // Remove from old slot
                    foreach (var cfg in Settings.WheelSlots)
                    {
                        if (cfg.AssignedTypes != null) cfg.AssignedTypes.Remove(capturedType);
                    }
                    Settings.WheelSlots.RemoveAll(s => s.AssignedTypes == null || s.AssignedTypes.Count == 0);

                    // Add to new slot
                    if (chosenSlot >= 0)
                    {
                        SlotConfig target = Settings.WheelSlots.Find(s => s.SlotIndex == chosenSlot);
                        if (target == null)
                        {
                            Settings.WheelSlots.Add(new SlotConfig(chosenSlot, "", capturedType));
                        }
                        else
                        {
                            target.AssignedTypes.Add(capturedType);
                        }
                    }

                    SaveUserConfigSettings($"Reassigned {capturedType} to slot {chosenSlot}");
                    LogDebug($"[Settings] {capturedType} assigned to slot {chosenSlot}");
                    RefreshAllDropdownVisuals();
                };

                var tile = new ControlTile();
                tile.Add(slotDropdown);
                currentCategory.Add(tile);
            }
        }

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

            // Register fake display names for override-based tools (e.g. BuildInfoMultiTool)
            // that have no PhysicalGunObject in inventory. The subtype is prefixed with
            // "Override_" so GetCentreText() can identify them and adjust label formatting.
            foreach (var kvp in _toolOverrides)
            {
                var fakeId = new MyDefinitionId(physicalGunType, "Override_" + kvp.Key.ToString());
                GunDisplayNames[fakeId] = GetToolDisplayName(kvp.Key);
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

            _currentWheelPage = 0;
            _allSelections = BuildSelections(character.GetInventory() as IMyInventory);

            _toolWheel.HoldToKeepOpen = Settings.HoldToKeepOpen;
            _toolWheel.SelectionMode = Settings.SelectionMode;
            _toolWheel.OpenedViaBlock = _wheelOpenedViaBlock;
            _toolWheel.BlockTriggerKeyDetected = _blockTriggerKey != MyKeys.None;
            _toolWheel.Open(GetPageSelections(_currentWheelPage, _allSelections));
        }

        private void OnWheelPageFlip()
        {
            _currentWheelPage = _currentWheelPage == 0 ? 1 : 0;
            _toolWheel.Open(GetPageSelections(_currentWheelPage, _allSelections));
        }

        /// <summary>
        /// Builds the complete list of <see cref="WeaponSelectionData"/> for all configured
        /// slots across both pages, querying the player's inventory once.
        /// </summary>
        private List<WeaponSelectionData> BuildSelections(IMyInventory inventory)
        {
            var result = new List<WeaponSelectionData>();
            var physicalGunType = typeof(MyObjectBuilder_PhysicalGunObject);

            foreach (var slotCfg in Settings.WheelSlots)
            {
                // With the strict 7-slot per page design, physical index 7 is always the page-flip
                // button when pagination is active. Valid user-configurable indices are 0 to 13.
                if (slotCfg.SlotIndex < 0 || slotCfg.SlotIndex > 13 || slotCfg.AssignedTypes == null || slotCfg.AssignedTypes.Count == 0)
                    continue;

                EquippedToolType primaryType = slotCfg.AssignedTypes[0];
                var variants = new List<MyDefinitionId>();
                bool anyWeapon = false;

                foreach (var toolType in slotCfg.AssignedTypes)
                {
                    ToolTypeOverride ovr;
                    if (_toolOverrides.TryGetValue(toolType, out ovr) && ovr.Equip != null)
                    {
                        // Override tool (e.g. BuildInfoMultiTool): inject a fake ID so it
                        // participates in scroll-wheel cycling alongside vanilla tools on the
                        // same slot. The "Override_" prefix lets OnToolWheelConfirmed and
                        // GetCentreText() identify and handle it correctly.
                        if (ovr.IsAvailable != null && ovr.IsAvailable())
                            variants.Add(new MyDefinitionId(physicalGunType, "Override_" + toolType.ToString()));
                    }
                    else
                    {
                        if (inventory != null)
                            FindAllVariantsInInventory(inventory, toolType, variants);
                        if (IsWeaponType(toolType))
                            anyWeapon = true;
                    }
                }

                // Check if this slot contains any modded tools (anything that isn't a vanilla hand tool or weapon)
                bool hasModdedTools = slotCfg.AssignedTypes.Exists(t =>
                    t != EquippedToolType.Welder &&
                    t != EquippedToolType.Grinder &&
                    t != EquippedToolType.Drill &&
                    !IsWeaponType(t));

                if (!anyWeapon)
                {
                    if (hasModdedTools)
                    {
                        // Sort alphabetically for modded tools
                        variants.Sort((a, b) =>
                        {
                            string nameA = GunDisplayNames.ContainsKey(a) ? GunDisplayNames[a] : a.SubtypeName;
                            string nameB = GunDisplayNames.ContainsKey(b) ? GunDisplayNames[b] : b.SubtypeName;
                            return string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
                        });
                    }
                    else
                    {
                        // Sort vanilla hand tools highest-tier-first
                        variants.Sort((a, b) => GetToolTier(b.SubtypeName).CompareTo(GetToolTier(a.SubtypeName)));
                    }
                }

                string label = string.IsNullOrEmpty(slotCfg.CustomLabel)
                    ? GetDefaultLabel(slotCfg.AssignedTypes)
                    : slotCfg.CustomLabel;

                result.Add(new WeaponSelectionData
                {
                    Index = slotCfg.SlotIndex,
                    ToolType = primaryType,
                    DisplayLabel = label,
                    IsWeapon = anyWeapon,
                    Icon = ToolWheelMenu.GetIconForType(primaryType),
                    Variants = variants,
                    IsAvailable = variants.Count > 0,
                    IsPageFlip = false,
                });
            }

            return result;
        }

        /// <summary>
        /// Returns the visible slice of <paramref name="all"/> for the requested <paramref name="page"/>.
        /// <para>
        /// Pagination is only activated when at least one entry occupies a slot on page 2 (index >= 7). 
        /// When active, physical slot 7 is reserved for the More... or Back... buttons.
        /// </para>
        /// </summary>
        private List<WeaponSelectionData> GetPageSelections(int page, List<WeaponSelectionData> all)
        {
            // Pagination is needed only when at least one entry uses a slot on page 2 (index >= 7).
            bool needsPagination = all.Exists(s => s.Index >= 7);

            var visible = new List<WeaponSelectionData>();

            if (!needsPagination)
            {
                // Single-page mode: logical indices 0-6 map directly to physical 0-6.
                foreach (var s in all)
                {
                    if (s.Index <= 6)
                    {
                        visible.Add(s);
                    }
                }
                return visible;
            }

            // Paginated mode — physical slot 7 is always a navigation button.
            if (page == 0)
            {
                foreach (var s in all)
                    if (s.Index <= 6)
                        visible.Add(s);

                visible.Add(new WeaponSelectionData
                {
                    Index = 7,
                    DisplayLabel = "Next Page",
                    IsPageFlip = true,
                    Icon = ToolWheelMenu.MatNextPage,
                    IsAvailable = true,
                });
            }
            else
            {
                foreach (var s in all)
                {
                    if (s.Index >= 7 && s.Index <= 13)
                    {
                        // Remap logical slot indices 7–13 to physical wheel positions 0–6.
                        var remapped = s;
                        remapped.Index = s.Index - 7;
                        visible.Add(remapped);
                    }
                }

                visible.Add(new WeaponSelectionData
                {
                    Index = 7,
                    DisplayLabel = "Previous Page",
                    IsPageFlip = true,
                    Icon = ToolWheelMenu.MatPrevPage,
                    IsAvailable = true,
                });
            }

            return visible;
        }

        private static bool IsWeaponType(EquippedToolType t)
        {
            return t == EquippedToolType.Pistol
                || t == EquippedToolType.Rifle
                || t == EquippedToolType.Launcher;
        }

        private static string GetToolDisplayName(EquippedToolType type)
        {
            switch (type)
            {
                case EquippedToolType.PaintGun: return "Paint Gun";
                case EquippedToolType.ConcreteTool: return "Concrete Tool";
                case EquippedToolType.HandScanner: return "Hand Scanner";
                case EquippedToolType.BuildInfoMultiTool: return "Build Info Multi Tool";
                case EquippedToolType.TerrainTool: return "Terrain Tool";
                default: return type.ToString();
            }
        }

        private static string GetDefaultLabel(List<EquippedToolType> types)
        {
            if (types == null || types.Count == 0) return "";
            if (types.Count == 1) return GetToolDisplayName(types[0]);
            // For stacked slots join with '/'.
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < types.Count; i++)
            {
                if (i > 0) sb.Append('/');
                sb.Append(GetToolDisplayName(types[i]));
            }
            return sb.ToString();
        }

        private void OnToolWheelConfirmed(EasyToolSwap_Session.EquippedToolType chosenTool, MyDefinitionId? specificId)
        {
            _wheelOpenedViaBlock = false;

            // 1. If the player scrolled to a fake Override_ variant on a mixed slot, resolve it
            //    directly to the correct equip delegate before touching the inventory at all.
            if (specificId.HasValue &&
                specificId.Value.SubtypeName.StartsWith("Override_", StringComparison.OrdinalIgnoreCase))
            {
                string typeName = specificId.Value.SubtypeName.Substring("Override_".Length);
                EquippedToolType overrideType;
                if (Enum.TryParse(typeName, out overrideType))
                {
                    ToolTypeOverride resolvedOvr;
                    if (_toolOverrides.TryGetValue(overrideType, out resolvedOvr) && resolvedOvr.Equip != null)
                    {
                        resolvedOvr.Equip();
                        _cycleMemoryTool = overrideType;
                        LogDebug($"[Wheel] Equipped via override delegate (scrolled variant): {overrideType}. Memory={_cycleMemoryTool}");
                        return;
                    }
                }
            }

            LogDebug($"[Wheel] Confirmed: {chosenTool}, SpecificId: {(specificId.HasValue ? specificId.Value.SubtypeName : "none")}");

            var character = MyAPIGateway.Session?.Player?.Character;
            if (character == null) return;

            // 2. If no specific variant was chosen (e.g. the slot has no variants or the player
            //    confirmed without scrolling), fall back to the override delegate for chosenTool.
            if (!specificId.HasValue)
            {
                ToolTypeOverride fallbackOvr;
                if (_toolOverrides.TryGetValue(chosenTool, out fallbackOvr) && fallbackOvr.Equip != null)
                {
                    fallbackOvr.Equip();
                    _cycleMemoryTool = chosenTool;
                    LogDebug($"[Wheel] Equipped via override delegate (fallback): {chosenTool}. Memory={_cycleMemoryTool}");
                    return;
                }
            }

            // 3. Normal inventory path — equip the chosen or best-available PhysicalGunObject.
            var inventory = character.GetInventory() as IMyInventory;
            var controller = character as Sandbox.Game.Entities.IMyControllableEntity;
            if (inventory == null || controller == null) return;

            MyDefinitionId? toolToEquip = specificId.HasValue ? specificId : FindBestToolInInventory(inventory, chosenTool);

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

        private void ActivateBuildInfoMultiTool()
        {
            var blockDefId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "BuildInfo_MultiTool_Large");

            MyCubeBlockDefinition blockDef;
            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(blockDefId, out blockDef))
            {
                MyCubeBuilder.Static.Activate(blockDefId);
                LogDebug("[Wheel] Activated Build Info Multitool via CubeBuilder.");
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("Build Info Multitool definition not found!", 2000, MyFontEnum.Red);
                LogDebug("[Wheel] ERROR: BuildInfo_MultiTool_Large definition not found in DefinitionManager.");
            }
        }

        private void OnWheelClosed()
        {
            _wheelOpenedViaBlock = false;
        }

        private void OnToolWheelCancelled()
        {
            _wheelOpenedViaBlock = false;
            LogDebug("[Wheel] Cancelled.");

            var character = MyAPIGateway.Session?.Player?.Character;
            var controller = character as Sandbox.Game.Entities.IMyControllableEntity;
            controller?.SwitchToWeapon(null);
        }

        private void OnToolWheelUnavailable(string toolName)
        {
            _wheelOpenedViaBlock = false;
            MyAPIGateway.Utilities.ShowNotification($"{toolName} is not in your inventory!", 2500, MyFontEnum.Red);
            LogDebug($"[Wheel] Unavailable selection: {toolName}");

            var character = MyAPIGateway.Session?.Player?.Character;
            var controller = character as Sandbox.Game.Entities.IMyControllableEntity;
            controller?.SwitchToWeapon(null);
        }

        private void InjectIntoCharacterMenu()
        {
            // 1. Get our block definition and force the settings Digi uses
            MyCubeBlockDefinition ourBlockDef;
            var blockDefId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "AQD_LG_EasyToolAccessFakeBlock");

            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(blockDefId, out ourBlockDef))
            {
                ourBlockDef.Public = true;
                ourBlockDef.GuiVisible = false; // Hides from normal tabs so it ONLY shows in tools
                _earlyLogBuffer.Add("[Session] Found and configured AQD_LG_EasyToolAccessFakeBlock definition.");
            }
            else
            {
                _earlyLogBuffer.Add("[Session] ERROR: Could not find AQD_LG_EasyToolAccessFakeBlock in definition manager!");
                return; // If it's not loaded, we can't inject it.
            }

            // 2. Fetch the categories using the exact internal keys Digi uses
            var categories = MyDefinitionManager.Static.GetCategories();

            var weaponCategory = categories.GetValueOrDefault("Section0_Position2_CharacterWeapons");
            var toolCategory = categories.GetValueOrDefault("Section0_Position2_CharacterTools");
            var itemCategory = categories.GetValueOrDefault("Section0_Position1_CharacterItems");

            // The exact string format required for this specific hybrid menu
            string injectString = "CubeBlock/AQD_LG_EasyToolAccessFakeBlock";

            // 3. Inject into Character Tools
            if (toolCategory != null)
            {
                toolCategory.SearchBlocks = true; // Ensures it pops up in the search bar
                if (!toolCategory.ItemIds.Contains(injectString))
                {
                    toolCategory.ItemIds.Add(injectString);
                    _earlyLogBuffer.Add("[Session] Successfully injected " + injectString + " into CharacterTools");
                }
            }
            else
            {
                _earlyLogBuffer.Add("[Session] ERROR: Could not find Section0_Position2_CharacterTools.");
            }

            // 4. (Optional) Inject into Character Items, just like Digi does
            if (itemCategory != null)
            {
                itemCategory.SearchBlocks = true;
                if (!itemCategory.ItemIds.Contains(injectString))
                {
                    itemCategory.ItemIds.Add(injectString);
                    _earlyLogBuffer.Add("[Session] Successfully injected " + injectString + " into CharacterItems");
                }
            }

            // 5. (Optional) Inject into Character Weapons, just like Enenra asked
            if (weaponCategory != null)
            {
                weaponCategory.SearchBlocks = true;
                if (!weaponCategory.ItemIds.Contains(injectString))
                {
                    weaponCategory.ItemIds.Add(injectString);
                    _earlyLogBuffer.Add("[Session] Successfully injected " + injectString + " into CharacterWeapons");
                }
            }
        }
    }
}