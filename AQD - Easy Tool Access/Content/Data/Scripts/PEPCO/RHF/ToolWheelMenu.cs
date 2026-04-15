using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Input;
using VRageMath;

namespace PEPCO
{
    /// <summary>
    /// Describes a single slot in the tool wheel.
    /// Build a <c>List&lt;WeaponSelectionData&gt;</c> (max 8 entries) and pass it to
    /// <see cref="ToolWheelMenu.Open"/> each time the wheel is opened.
    /// </summary>
    public struct WeaponSelectionData
    {
        /// <summary>Zero-based position index (0–7). Entries with Index &gt; 7 are ignored.</summary>
        public int Index;

        /// <summary>The logical tool/weapon category this slot represents.</summary>
        public EasyToolSwap_Session.EquippedToolType ToolType;

        /// <summary>Human-readable label shown in the centre area when this slot is highlighted.</summary>
        public string DisplayLabel;

        /// <summary>
        /// True for weapons (Pistol/Rifle/Launcher) — preserves variant selection across opens.
        /// False for tools (Welder/Grinder/Drill/PaintGun) — always resets to the highest tier.
        /// </summary>
        public bool IsWeapon;

        /// <summary>Icon material rendered inside the slice.</summary>
        public Material Icon;

        /// <summary>
        /// All matching <see cref="VRage.Game.MyDefinitionId"/> variants found in the player's
        /// inventory, sorted highest-tier-first for tools or in discovery order for weapons.
        /// </summary>
        public List<MyDefinitionId> Variants;

        /// <summary>True when at least one variant was found in the player's inventory.</summary>
        public bool IsAvailable;
    }

    /// <summary>
    /// A scalable, radial selection wheel displayed on the HUD for picking a tool type.
    /// Mathematically identical to BuildVision 2.
    /// </summary>
    public class ToolWheelMenu : HudElementBase
    {
        // =====================================================================
        // MASTER SCALE
        // =====================================================================
        private const float MasterWheelDiameter = 512f;

        // --- Exact BuildVision 2 Constants ---
        private const float WheelInnerDiamScale = 0.6f;
        private const float WheelBodyMaxDiamMult = 1.05f;
        private const float DefaultOpacity = 0.8f;

        // --- Exact BV2 Colors ---
        private static readonly Color BgColor = new Color(70, 78, 86, (int)(255 * DefaultOpacity));  // Mid grey
        private static readonly Color HighlightColor = new Color(41, 54, 62, (int)(255 * DefaultOpacity));  // Dark slate grey (Hover)
        private static readonly Color FocusColor = new Color(142, 188, 206, (int)(255 * DefaultOpacity)); // Mint (Selected)

        private static readonly Color LabelNormal = new Color(210, 210, 210, 255);
        private static readonly Color LabelSelected = new Color(255, 255, 255, 255);

        // --- Dynamic Text Formats ---
        private readonly GlyphFormat _fmtLabel;
        private readonly GlyphFormat _fmtSelected;
        private readonly GlyphFormat _fmtHeader;
        private readonly GlyphFormat _fmtSub;
        private readonly GlyphFormat _fmtDimmed;
        private readonly GlyphFormat _fmtUnavailable;

        // Per-tool-type icon materials — each maps to its own texture SubtypeId.
        // Exposed as internal so WeaponSelectionData instances built in the session can reference them.
        internal static readonly Material MatWelder   = new Material("WelderIcon",           new Vector2(256f, 256f));
        internal static readonly Material MatGrinder  = new Material("GrinderIcon",          new Vector2(256f, 256f));
        internal static readonly Material MatDrill    = new Material("DrillIcon",            new Vector2(256f, 256f));
        internal static readonly Material MatPistol   = new Material("PistolIcon",           new Vector2(256f, 256f));
        internal static readonly Material MatRifle    = new Material("RifleIcon",            new Vector2(256f, 256f));
        internal static readonly Material MatLauncher = new Material("HandheldLauncherIcon", new Vector2(256f, 256f));
        internal static readonly Material MatPaintGun = new Material("PaintGunIcon",         new Vector2(256f, 256f));
        internal static readonly Material MatHideTool = new Material("HideWeaponIcon",       new Vector2(256f, 256f));

        // Size constants for the per-slice icon/label layout (at the 512px base diameter).
        // Actual sizes and offsets are scaled by fontScale inside ToolEntry.Init.
        // Icons are always the large size regardless of selection state.
        private const float IconSize     = 72f;  // icon side (always large)
        private const float IconLabelGap = 4f;   // (unused for slice text, kept for offset calc)
        private const float LineHeight   = 18f;  // approximate px height of one text line at fontScale=1

        // --- Simplified Entry Type ---
        // Label is the direct ScrollBoxEntry element (positioned by RadialSelectionBox).
        // The icon is a child of the label, offset upward by an amount computed from the
        // current line count so it always sits cleanly above the text block.
        private class ToolEntry : ScrollBoxEntry<Label>
        {
            public EasyToolSwap_Session.EquippedToolType ToolType { get; private set; }
            public bool Available { get; private set; }

            // True for Pistol/Rifle/Launcher — preserves variant selection across opens.
            // False for Welder/Grinder/Drill — always resets to index 0 (highest tier).
            public bool IsWeapon { get; private set; }

            // Weapon-specific variant cycling (null for non-weapon entries)
            public List<MyDefinitionId> Variants { get; private set; }
            public int VariantIndex { get; private set; }
            public MyDefinitionId? SelectedVariant
            {
                get
                {
                    if (Variants != null && Variants.Count > 0)
                        return Variants[VariantIndex];
                    return null;
                }
            }

            private string _baseName;
            private GlyphFormat _normalFmt, _selectedFmt, _dimmedFmt;
            private float _fontScale;
            private bool _isSelected;

            public string BaseName { get { return _baseName; } }

            private readonly TexturedBox _icon;

            private static readonly Color IconNormal   = new Color(255, 255, 255, 255); // fully opaque white — let the texture speak for itself
            private static readonly Color IconDimmed   = new Color(255, 255, 255,  60);

            public ToolEntry() : base()
            {
                SetElement(new Label()
                {
                    AutoResize     = true,
                    BuilderMode    = TextBuilderModes.Lined,
                    VertCenterText = true,
                });

                // Icon is a child of the label so it shares the same parent-space origin.
                // Material, size, and offset are set in Init() once the tool type is known.
                _icon = new TexturedBox(Element)
                {
                    Color = IconNormal,
                };

                Enabled = true;
            }

            public void Init(EasyToolSwap_Session.EquippedToolType toolType, string displayLabel, bool isWeapon, float fontScale, Material iconMaterial, GlyphFormat normal, GlyphFormat selected, GlyphFormat dimmed)
            {
                ToolType     = toolType;
                Available    = true;
                IsDummy      = false;
                IsWeapon     = isWeapon;
                _baseName    = displayLabel;
                _normalFmt   = normal;
                _selectedFmt = selected;
                _dimmedFmt   = dimmed;
                _fontScale   = fontScale;
                Variants     = null;

                _icon.Material = iconMaterial;
                float scaledIcon = IconSize * fontScale;
                _icon.Size   = new Vector2(scaledIcon, scaledIcon);

                Element.TextBoard.SetText("", _normalFmt);
                _icon.Offset   = Vector2.Zero;
            }

            public void SetAvailable(bool available)
            {
                Available = available;
                // Re-apply the full highlight state so visibility and colours stay consistent.
                SetHighlighted(_isSelected);
            }

            // Called on wheel open to refresh the variant list; preserves VariantIndex if the
            // previously selected subtype is still present and this is a weapon entry.
            // Tool entries always reset to index 0 (caller sorts variants highest-tier-first).
            public void SetVariants(List<MyDefinitionId> variants)
            {
                MyDefinitionId? prev = IsWeapon ? SelectedVariant : (MyDefinitionId?)null;
                Variants = variants;

                if (prev.HasValue && variants != null)
                {
                    int found = variants.FindIndex(v => v == prev.Value);
                    VariantIndex = found >= 0 ? found : 0;
                }
                else
                {
                    VariantIndex = 0;
                }
            }

            public bool IsDummy { get; private set; }

            public void MarkAsDummy()
            {
                IsDummy   = true;
                Available = false;
                // Leave _icon.Visible = true so the HideWeaponIcon is rendered at dimmed opacity.
            }

            public void CycleVariant(int step)
            {
                if (Variants == null || Variants.Count <= 1) return;
                VariantIndex = ((VariantIndex + step) % Variants.Count + Variants.Count) % Variants.Count;
            }

            public void SetHighlighted(bool highlighted)
            {
                _isSelected = highlighted;

                if (IsDummy)
                {
                    _icon.Color = IconNormal;
                    ApplySizeMode();
                }
                else if (!Available)
                {
                    _icon.Color = IconDimmed;
                    ApplySizeMode();
                }
                else if (highlighted)
                {
                    _icon.Color = IconNormal;
                    ApplySizeMode();
                }
                else
                {
                    _icon.Color = IconNormal;
                    ApplySizeMode();
                }
            }

            // Slices always show icon only — text is displayed in the centre label.
            private void ApplySizeMode()
            {
                float scaledIcon = IconSize * _fontScale;
                _icon.Size   = new Vector2(scaledIcon, scaledIcon);

                // Clear slice text and centre the icon.
                Element.TextBoard.SetText("", _normalFmt);
                _icon.Offset   = Vector2.Zero;
            }

            // Returns the display text for the currently selected entry,
            // to be written into the centre label by RefreshHighlight.
            public string GetCentreText()
            {
                if (Variants != null && Variants.Count > 0)
                {
                    MyDefinitionId variantId = Variants[VariantIndex];
                    string displayName;
                    if (!EasyToolSwap_Session.GunDisplayNames.TryGetValue(variantId, out displayName))
                        displayName = variantId.SubtypeName;

                    if (IsWeapon)
                        return _baseName + "\n" + displayName + "\n[" + (VariantIndex + 1) + "/" + Variants.Count + "]";

                    if (Variants.Count > 1)
                        return displayName + "\n[" + (VariantIndex + 1) + "/" + Variants.Count + "]";

                    return displayName;
                }

                return _baseName;
            }
        }

        // --- Layout Fields ---
        private readonly TexturedBox _centerBackground;
        private readonly Label _centreLabel;
        private readonly Label _subLabel;
        private readonly RadialSelectionBox<ToolEntry, Label> _wheel;

        // --- State ---
        private int _lastSelection = -1;

        /// <summary>
        /// When true (default), the wheel stays open until the player explicitly confirms or
        /// cancels. When false, releasing the keybind auto-confirms the highlighted entry.
        /// Set by the session from <see cref="UserConfigSettings.HoldToKeepOpen"/>.
        /// </summary>
        public bool HoldToKeepOpen { get; set; } = true;

        /// <summary>
        /// When true the wheel was opened by the fake block rather than the keybind.
        /// The key-release auto-confirm is suppressed; the player must click to confirm.
        /// </summary>
        public bool OpenedViaBlock { get; set; } = false;

        /// <summary>
        /// When true, a toolbar number key was detected when the block triggered the wheel.
        /// Allows hold-to-release auto-confirm even on the block path.
        /// </summary>
        public bool BlockTriggerKeyDetected { get; set; } = false;

        /// <summary>
        /// Mouse cursor sensitivity for the radial wheel. Applied each time the wheel is opened.
        /// </summary>
        public float CursorSensitivity
        {
            get { return _wheel.CursorSensitivity; }
            set { _wheel.CursorSensitivity = value; }
        }

        public event Action<EasyToolSwap_Session.EquippedToolType, VRage.Game.MyDefinitionId?> SelectionConfirmed;
        public event Action SelectionCancelled;
        public event Action<string> SelectionUnavailable;

        public ToolWheelMenu(HudParentBase parent = null) : base(parent)
        {
            // --- EXACT BUILDVISION 2 GEOMETRY CALCULATIONS ---
            float fontScale = MasterWheelDiameter / 512f;

            // The exact diameter of the center body (1.05 * 0.6 * Diameter)
            float centerDiameter = WheelBodyMaxDiamMult * WheelInnerDiamScale * MasterWheelDiameter;

            // Initialize Fonts dynamically scaled against 512f baseline
            _fmtLabel       = new GlyphFormat(LabelNormal,                       TextAlignment.Center, 0.85f * fontScale);
            _fmtSelected    = new GlyphFormat(LabelSelected,                      TextAlignment.Center, 0.95f * fontScale);
            _fmtHeader      = new GlyphFormat(Color.White,                        TextAlignment.Center, 1.14f * fontScale);
            _fmtSub         = new GlyphFormat(new Color(150, 150, 150, 200),      TextAlignment.Center, 0.70f * fontScale);
            _fmtDimmed      = new GlyphFormat(new Color(100, 100, 100, 120),      TextAlignment.Center, 0.85f * fontScale);
            _fmtUnavailable = new GlyphFormat(new Color(220,  60,  60, 220),      TextAlignment.Center, 0.80f * fontScale);

            Size = new Vector2(MasterWheelDiameter, MasterWheelDiameter);

            // 1. Center Background Circle 
            _centerBackground = new TexturedBox(this)
            {
                Material = Material.CircleMat,
                Color = BgColor,
                Size = new Vector2(centerDiameter, centerDiameter),
                ZOffset = -1,
            };

            // 2. Radial selection wheel
            _wheel = new RadialSelectionBox<ToolEntry, Label>(this)
            {
                Size = new Vector2(MasterWheelDiameter, MasterWheelDiameter),
                BackgroundColor = BgColor,
                HighlightColor = HighlightColor,
                // CRITICAL FIX: Set to 8 to prevent centroid math from pulling the text inward!
                MaxEntryCount = 8,
                CursorSensitivity = 0.4f,
                IsInputEnabled = true,
                Visible = true,
            };

            // Target polyboard directly to avoid compiler errors on older RHF builds
            _wheel.polyBoard.InnerRadius = WheelInnerDiamScale;

            // Pre-allocate exactly 8 reusable ToolEntry slots (MaxEntryCount = 8).
            // All slots start as dummies; Open() re-initialises them from a WeaponSelectionData list.
            for (int i = 0; i < 8; i++)
            {
                var slot = new ToolEntry();
                slot.Init(EasyToolSwap_Session.EquippedToolType.Welder, "", false, fontScale, MatHideTool, _fmtLabel, _fmtSelected, _fmtDimmed);
                slot.MarkAsDummy();
                _wheel.Add(slot);
            }

            // Centre label — large white text showing the selected entry name.
            _centreLabel = new Label(this)
            {
                AutoResize  = true,
                BuilderMode = TextBuilderModes.Lined,
                Offset      = new Vector2(0f, 35f * fontScale),
            };
            _centreLabel.TextBoard.SetText("", _fmtHeader);

            // Sub label — small grey hint text (confirm/cancel instructions).
            _subLabel = new Label(this)
            {
                AutoResize  = true,
                BuilderMode = TextBuilderModes.Lined,
                Offset      = new Vector2(0f, -35f * fontScale),
            };
            _subLabel.TextBoard.SetText("Move mouse to select", _fmtSub);

            Visible = false;
        }

        /// <summary>
        /// Opens the wheel using the supplied selection data.
        /// Entries are mapped to wheel slots by <see cref="WeaponSelectionData.Index"/> (0–7).
        /// Any entry whose Index is greater than 7 is silently ignored.
        /// Slots not covered by <paramref name="selections"/> are shown as hidden dummy sectors.
        /// </summary>
        public void Open(List<WeaponSelectionData> selections)
        {
            HudMain.EnableCursor = false;
            BindManager.BlacklistMode = SeBlacklistModes.MouseAndCam;
            _wheel.IsInputEnabled = true;

            float fontScale = MasterWheelDiameter / 512f;

            // Reset all 8 slots to dummy state before applying new data.
            for (int i = 0; i < _wheel.EntryList.Count; i++)
            {
                var slot = _wheel.EntryList[i] as ToolEntry;
                if (slot == null) continue;
                slot.Init(EasyToolSwap_Session.EquippedToolType.Welder, "", false, fontScale, MatHideTool, _fmtLabel, _fmtSelected, _fmtDimmed);
                slot.MarkAsDummy();
            }

            // Populate slots from the selection list (max 8; index > 7 is ignored).
            int defaultIndex = -1;
            if (selections != null)
            {
                for (int s = 0; s < selections.Count; s++)
                {
                    WeaponSelectionData data = selections[s];
                    if (data.Index > 7) continue;

                    var slot = _wheel.EntryList[data.Index] as ToolEntry;
                    if (slot == null) continue;

                    slot.Init(data.ToolType, data.DisplayLabel, data.IsWeapon, fontScale, data.Icon, _fmtLabel, _fmtSelected, _fmtDimmed);
                    slot.SetVariants(data.Variants);
                    slot.SetAvailable(data.IsAvailable);

                    // Default highlight: first available Welder slot, then first available slot.
                    if (defaultIndex < 0 && data.IsAvailable)
                        defaultIndex = data.Index;
                    if (data.ToolType == EasyToolSwap_Session.EquippedToolType.Welder && data.IsAvailable)
                        defaultIndex = data.Index;
                }
            }

            _lastSelection = defaultIndex;
            if (defaultIndex >= 0)
                _wheel.SetSelectionAt(defaultIndex);

            Visible = true;
            RefreshHighlight();
        }

        public void Close()
        {
            Visible = false;
            _wheel.IsInputEnabled = false;
            HudMain.EnableCursor = false;
            BindManager.BlacklistMode = SeBlacklistModes.None;
            OpenedViaBlock = false;
            BlockTriggerKeyDetected = false;
            WheelClosed?.Invoke();
        }

        /// <summary>
        /// Fired whenever the wheel is closed by any path (confirm, cancel, or direct close).
        /// </summary>
        public event Action WheelClosed;

        /// <summary>
        /// Called by the session when the keybind is released while the wheel is open.
        /// In hold-to-keep-open=true mode this auto-confirms the current selection.
        /// </summary>
        public void NotifyKeyReleased()
        {
            if (!Visible || !HoldToKeepOpen) return;

            int sel = _wheel.SelectionIndex;
            if (sel >= 0)
            {
                var entry = _wheel.EntryList[sel] as ToolEntry;
                if (entry != null && entry.IsDummy)
                    entry = null;
                if (entry != null && entry.Available)
                {
                    MyDefinitionId? specificId = entry.SelectedVariant;
                    Close();
                    SelectionConfirmed?.Invoke(entry.ToolType, specificId);
                    return;
                }
                if (entry != null && !entry.Available)
                {
                    string name = entry.BaseName;
                    Close();
                    SelectionUnavailable?.Invoke(name);
                    return;
                }
            }

            // Nothing valid highlighted — treat as cancel.
            Close();
            SelectionCancelled?.Invoke();
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (!Visible) return;

            int sel = _wheel.SelectionIndex;
            if (sel != _lastSelection)
            {
                _lastSelection = sel;
                RefreshHighlight();
            }

            bool leftClicked  = SharedBinds.LeftButton.IsNewPressed;
            bool rightClicked = SharedBinds.RightButton.IsNewPressed;
            bool escPressed   = MyAPIGateway.Input.IsNewKeyPressed(MyKeys.Escape);

            // Scroll wheel cycles variants on the currently hovered entry.
            if (sel >= 0)
            {
                var hoveredEntry = _wheel.EntryList[sel] as ToolEntry;
                if (hoveredEntry != null && hoveredEntry.Available && hoveredEntry.Variants != null)
                {
                    int scroll = MyAPIGateway.Input.DeltaMouseScrollWheelValue();
                    if (scroll > 0)
                    {
                        hoveredEntry.CycleVariant(-1);
                        RefreshHighlight();
                    }
                    else if (scroll < 0)
                    {
                        hoveredEntry.CycleVariant(1);
                        RefreshHighlight();
                    }
                }
            }

            // Give visual feedback on click (Mint color)
            if (SharedBinds.LeftButton.IsPressed && sel >= 0)
            {
                _wheel.HighlightColor = FocusColor;
                _centerBackground.Color = FocusColor;
            }
            else
            {
                _wheel.HighlightColor = HighlightColor;
            }

            if (leftClicked && sel >= 0)
            {
                var entry = _wheel.EntryList[sel] as ToolEntry;
                if (entry != null && !entry.IsDummy && entry.Available)
                {
                    MyDefinitionId? specificId = entry.SelectedVariant;
                    Close();
                    SelectionConfirmed?.Invoke(entry.ToolType, specificId);
                }
                else if (entry != null && !entry.IsDummy)
                {
                    string name = entry.BaseName;
                    Close();
                    SelectionUnavailable?.Invoke(name);
                }
                else if (entry != null && entry.IsDummy)
                {
                    Close();
                    SelectionCancelled?.Invoke();
                }
            }
            else if (rightClicked || escPressed)
            {
                Close();
                SelectionCancelled?.Invoke();
            }
        }

        private void RefreshHighlight()
        {
            for (int i = 0; i < _wheel.EntryList.Count; i++)
            {
                var entry = _wheel.EntryList[i] as ToolEntry;
                entry?.SetHighlighted(i == _lastSelection);
            }

            ToolEntry sel = _lastSelection >= 0 ? _wheel.EntryList[_lastSelection] as ToolEntry : null;
            if (sel != null && sel.IsDummy)
                sel = null;

            // Centre label: tool name always shown; append red UNAVAILABLE when not in inventory.
            if (sel != null && sel.Available)
            {
                _centreLabel.TextBoard.SetText(sel.GetCentreText(), _fmtHeader);
            }
            else if (sel != null)
            {
                var centreText = new RichText();
                centreText.Add(new RichText(sel.GetCentreText(), _fmtHeader));
                centreText.Add(new RichText("\nUNAVAILABLE", _fmtUnavailable));
                _centreLabel.TextBoard.SetText(centreText);
            }
            else if (_lastSelection < 0)
            {
                _centreLabel.TextBoard.SetText(
                    HoldToKeepOpen ? "Hold to keep open" : "Click to confirm",
                    _fmtHeader);
            }
            else
            {
                // Dummy sector hovered — show placeholder title.
                _centreLabel.TextBoard.SetText("Hide Tool", _fmtHeader);
            }

            // Sub label: dismiss/variant hints only — no "Move mouse to select" at any point.
            bool holdModeActive = HoldToKeepOpen && (!OpenedViaBlock || BlockTriggerKeyDetected);
            bool hasSelection = sel != null && sel.Available;
            bool hasMultipleVariants = hasSelection && sel.Variants != null && sel.Variants.Count > 1;

            string dismissHint = holdModeActive ? "Release to confirm\nRight-click to cancel"
                                             : "Left-click to confirm\nRight-click to cancel";
            string variantHint = hasMultipleVariants ? "\nScroll to cycle variants" : "";
            _subLabel.TextBoard.SetText(dismissHint + variantHint, _fmtSub);

            // Sync the center circle background color with the highlighted slice
            if (_lastSelection >= 0 && !SharedBinds.LeftButton.IsPressed)
            {
                _centerBackground.Color = HighlightColor;
            }
            else if (!SharedBinds.LeftButton.IsPressed)
            {
                _centerBackground.Color = BgColor;
            }
        }
    }
}