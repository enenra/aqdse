using Sandbox.ModAPI;
using VRage.Game.Components;

// This code was written for me by SISK - many thanks to him!
namespace enenra.QoL
{

    public static class LoadMessage
    {
        private const string SENDER = "enenra";
        private const string VARIABLE_NAME = "displayCount";
        private const int MAX_TIMES = 2;
        private const string MESSAGE1 = "PSA for AQD - Quality of Life mod users: I've decided to refocus the mod. The following mods have been removed from the pack: BuildVision, HUD Compass, Sleep Mod. They will be moved to my new modpack, Vanilla Extension, instead. Please visit the QoL mod page for more details.";
        private const string MESSAGE2 = "This message will only show twice on load per world, and will be removed completely in a week or so.";
        private static int _amountDisplayed;

        public static void LoadData()
        {
            MyAPIGateway.Utilities.GetVariable(VARIABLE_NAME, out _amountDisplayed);
            MyAPIGateway.Session.OnSessionReady += OnSessionReady;
        }

        public static void UnloadData()
        {
            MyAPIGateway.Session.OnSessionReady -= OnSessionReady;
        }

        private static void OnSessionReady()
        {
            MyAPIGateway.Session.OnSessionReady -= OnSessionReady;
            if (_amountDisplayed < MAX_TIMES)
            {
                _amountDisplayed++;
                MyAPIGateway.Utilities.ShowMessage(SENDER, MESSAGE1);
                MyAPIGateway.Utilities.ShowMessage(SENDER, MESSAGE2);
                MyAPIGateway.Utilities.SetVariable(VARIABLE_NAME, _amountDisplayed);
            }
        }
    }
}