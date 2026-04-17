using System.Collections.Generic;
using System.Xml.Serialization;
using RichHudFramework.UI;

namespace PEPCO
{
    /// <summary>
    /// Configures a single physical slot on the tool wheel (visual index 0–6; index 7 is
    /// reserved for the automatic More.../Back... page-flip button when page-2 slots exist).
    /// A slot may contain more than one <see cref="EasyToolSwap_Session.EquippedToolType"/>; all
    /// matching inventory variants are aggregated into one scrollable list for that slot.
    /// </summary>
    public class SlotConfig
    {
        /// <summary>
        /// Logical slot position across both pages (0–13).
        /// Page 1 = 0–6  (slot 7 is auto-reserved for More...).
        /// Page 2 = 8–13 (slot 7 on page 2 is auto-reserved for Back...).
        /// Index 7 and 15+ are ignored.
        /// </summary>
        [XmlAttribute]
        public int SlotIndex { get; set; }

        /// <summary>
        /// One or more tool/weapon types assigned to this slot.
        /// Variants from all types are merged into a single scrollable list.
        /// </summary>
        [XmlArray("AssignedTypes")]
        [XmlArrayItem("Type")]
        public List<EasyToolSwap_Session.EquippedToolType> AssignedTypes { get; set; }
            = new List<EasyToolSwap_Session.EquippedToolType>();

        /// <summary>
        /// Optional display label override. When empty the label is derived from
        /// the assigned type(s) automatically.
        /// </summary>
        [XmlAttribute]
        public string CustomLabel { get; set; } = "";

        public SlotConfig() { }

        public SlotConfig(int slotIndex, string customLabel,
            params EasyToolSwap_Session.EquippedToolType[] types)
        {
            SlotIndex   = slotIndex;
            CustomLabel = customLabel;
            AssignedTypes.AddRange(types);
        }
    }

    [XmlRoot("UserConfigSettings")]
    public class UserConfigSettings
    {
        public enum CursorSensitivityLevel
        {
            Low    = 0,
            Medium = 1,
            High   = 2
        }

        public enum WheelSelectionMode
        {
            DirectionalFlick = 0,
            CursorTracking   = 1
        }

        public List<BindDefinition> UserConfigKeyBinds = new List<BindDefinition>();

        /// <summary>
        /// When true, the player holds the keybind to keep the wheel open; releasing it
        /// auto-confirms the highlighted selection.
        /// When false, the wheel stays open until the player explicitly left-clicks to
        /// confirm or right-clicks to cancel.
        /// </summary>
        public bool HoldToKeepOpen { get; set; } = true;

        /// <summary>
        /// Dictates how mouse input selects slices on the wheel.
        /// DirectionalFlick uses custom angle-based delta math.
        /// CursorTracking uses RichHudFramework's native position-based selection.
        /// </summary>
        public WheelSelectionMode SelectionMode { get; set; } = WheelSelectionMode.DirectionalFlick;

        /// <summary>
        /// Cursor sensitivity preset for the tool wheel.
        /// Low = 0.35, Medium = 0.4, High = 0.55
        /// </summary>
        public CursorSensitivityLevel WheelCursorSensitivity { get; set; } = CursorSensitivityLevel.Medium;

        /// <summary>
        /// User-configured wheel slot layout. Serialised to XML.
        /// </summary>
        [XmlArray("WheelSlots")]
        [XmlArrayItem("Slot")]
        public List<SlotConfig> WheelSlots { get; set; } = new List<SlotConfig>();

        // Parameterless constructor required for XML serialization
        public UserConfigSettings() { }

        public UserConfigSettings(List<BindDefinition> userConfigKeyBinds)
        {
            UserConfigKeyBinds = userConfigKeyBinds;
        }
    }
}

