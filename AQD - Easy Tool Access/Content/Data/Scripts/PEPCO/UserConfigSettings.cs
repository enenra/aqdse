using System.Collections.Generic;
using System.Xml.Serialization;
using RichHudFramework.UI;

namespace PEPCO
{
    [XmlRoot("UserConfigSettings")]
    public class UserConfigSettings
    {
        public enum CursorSensitivityLevel
        {
            Low    = 0,
            Medium = 1,
            High   = 2
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
        /// Cursor sensitivity preset for the tool wheel.
        /// Low = 0.35, Medium = 0.4, High = 0.55
        /// </summary>
        public CursorSensitivityLevel WheelCursorSensitivity { get; set; } = CursorSensitivityLevel.Medium;

        // Parameterless constructor required for XML serialization
        public UserConfigSettings() { }

        public UserConfigSettings(List<BindDefinition> userConfigKeyBinds)
        {
            UserConfigKeyBinds = userConfigKeyBinds;
        }
    }
}

