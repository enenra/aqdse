using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;

namespace GyroAnimations
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EmissiveValues :MySessionComponentBase
    {
        internal static Color GREEN = new Color(0, 255, 0);
        internal static Color RED = new Color(255, 0, 0);

        public override void BeforeStart()
        {
            if (MyAPIGateway.Utilities.IsDedicated) return;

            UpdateEmissiveValues();

        }

        private void UpdateEmissiveValues()
        {
            bool aqdVisualsPresent = false;
            bool emissiveColorsPresent = false;

            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                if (mod.PublishedFileId == 2711430394) // AQD - Emissive Colors
                    aqdVisualsPresent = true;
                else if (mod.PublishedFileId == 2212516940) // Emissive Colors - Red / Green Color Vision Deficiency
                    emissiveColorsPresent = true;

                if (aqdVisualsPresent && emissiveColorsPresent)
                    break;
            }

            if (aqdVisualsPresent)
            {
                RED = new Color(171, 42, 29);
                GREEN = emissiveColorsPresent ? new Color(10, 255, 25) : new Color(60, 163, 33);
            }
            else if (emissiveColorsPresent)
            {
                GREEN = new Color(10, 255, 25);
            }
        }
    }
}
