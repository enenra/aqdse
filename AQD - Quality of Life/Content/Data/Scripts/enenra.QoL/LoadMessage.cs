using Sandbox.ModAPI;
using VRage.Game.Components;

// This code was written for me by SISK - many thanks to him!
namespace enenra.QoL
{

    public static class LoadMessage
    {
        private const string SENDER = "enenra";
        private const string VARIABLE_NAME = "displayCount2";
        private const int MAX_TIMES = 2;
        private const string MESSAGE1 = "PSA for AQD - Quality of Life: Update 1.5 contains three new mods. \nTool Switcher by avaness - allows you to bind several tools to the same slot and use the scroll wheel to switch between them. \nTeamSpot by Klime - Shift+R to set GPS points where you're pointing that are visible to faction members. \nClean Assembler Tab by Arstraea - organizes the production tab in a more clean manner. Please visit the mod page and its changelog for more details.";
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