using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace AQD.LcdColorProfiles
{

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

    public class LcdColorProfilesCore : MySessionComponentBase
    {

        public bool SetupDone = false;

        public List<IMyTerminalControl> ControlsListMaster = new List<IMyTerminalControl>();
        public SortedDictionary<string, LcdColorProfile> LcdColorProfiles = new SortedDictionary<string, LcdColorProfile>();

        public StringBuilder SelectedProfile = new StringBuilder("None");
        public StringBuilder ProfileName = new StringBuilder("");

        public LcdColorProfileStorage StoredData = new LcdColorProfileStorage();

        public override void UpdateBeforeSimulation()
        {

            if (SetupDone == false)
            {

                SetupDone = true;
                CreateControls();

                if (MyAPIGateway.Utilities.FileExistsInLocalStorage("LcdColorProfiles.mes", typeof(LcdColorProfileStorage)) == true)
                {

                    try
                    {

                        LcdColorProfileStorage config = null;
                        var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage("LcdColorProfiles.mes", typeof(LcdColorProfileStorage));
                        string configcontents = reader.ReadToEnd();
                        config = MyAPIGateway.Utilities.SerializeFromXML<LcdColorProfileStorage>(configcontents);

                        if (config != null)
                        {

                            StoredData = config;
                            var byteData = Convert.FromBase64String(config.StoredData);
                            var storedProfile = MyAPIGateway.Utilities.SerializeFromBinary<SortedDictionary<string, LcdColorProfile>>(byteData);

                            if (storedProfile != null)
                            {

                                LcdColorProfiles = storedProfile;

                            }

                        }


                    }
                    catch (Exception exc)
                    {



                    }

                }

                MyAPIGateway.TerminalControls.CustomControlGetter += AddControls;
                MyAPIGateway.Utilities.InvokeOnGameThread(() => { this.SetUpdateOrder(MyUpdateOrder.NoUpdate); });

            }

        }

        public void CreateControls()
        {

            //Separator
            var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("LCDCP_Separator");
            separator.Enabled = (Block) => { return true; };
            separator.SupportsMultipleBlocks = true;
            ControlsListMaster.Add(separator);

            //Label
            var label = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("LCDCP_Label");
            label.Enabled = (Block) => { return true; };
            label.SupportsMultipleBlocks = true;
            label.Label = MyStringId.GetOrCompute("LCD Color Profiles");
            ControlsListMaster.Add(label);

            //LcdColorProfiles
            var profiles = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyTerminalBlock>("LCDCP_Profiles");
            profiles.Enabled = (Block) => { return true; };
            profiles.Visible = (Block) => { return true; };
            profiles.SupportsMultipleBlocks = true;
            profiles.VisibleRowsCount = 5;
            profiles.Multiselect = false;
            profiles.Title = MyStringId.GetOrCompute("Profiles");
            profiles.ListContent = DisplayProfiles;
            profiles.ItemSelected = SelectProfile;
            ControlsListMaster.Add(profiles);

            //LoadFromProfile
            var button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("LCDCP_LoadFromProfile");
            button.Enabled = (Block) => { return true; };
            button.Visible = (Block) => { return true; };
            button.SupportsMultipleBlocks = true;
            button.Title = MyStringId.GetOrCompute("Load From Profile");
            button.Tooltip = MyStringId.GetOrCompute("Loads LCD color settings from a saved profile.");
            button.Action = LoadProfile;
            ControlsListMaster.Add(button);

            //SaveToProfile
            var buttonB = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("LCDCP_SaveToProfile");
            buttonB.Enabled = (Block) => { return true; };
            buttonB.Visible = (Block) => { return true; };
            buttonB.SupportsMultipleBlocks = false;
            buttonB.Title = MyStringId.GetOrCompute("Save To Profile");
            buttonB.Tooltip = MyStringId.GetOrCompute("Save LCD color settings to a profile.");
            buttonB.Action = SaveProfile;
            ControlsListMaster.Add(buttonB);

            //DeleteProfile
            var buttonC = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("LCDCP_DeleteProfile");
            buttonC.Enabled = (Block) => { return true; };
            buttonC.Visible = (Block) => { return true; };
            buttonC.SupportsMultipleBlocks = false;
            buttonC.Title = MyStringId.GetOrCompute("Delete Profile");
            buttonC.Tooltip = MyStringId.GetOrCompute("Deletes the currently selected profile.");
            buttonC.Action = DeleteProfile;
            ControlsListMaster.Add(buttonC);

            //ProfileName
            var profileName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTerminalBlock>("LCDCP_ProfileName");
            profileName.Enabled = (Block) => { return true; };
            profileName.Visible = (Block) => { return true; };
            profileName.SupportsMultipleBlocks = false;
            profileName.Title = MyStringId.GetOrCompute("New Profile Name");
            profileName.Getter = (block) => { return ProfileName; };
            profileName.Setter = (block, sb) => { ProfileName = sb; };
            ControlsListMaster.Add(profileName);


        }

        public void DisplayProfiles(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> items, List<MyTerminalControlListBoxItem> selected)
        {

            var profile = SelectedProfile.ToString();
            var firstItem = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("None"), MyStringId.GetOrCompute(""), profile);
            items.Add(firstItem);

            if (profile == "None")
                selected.Add(firstItem);

            foreach (var profileName in LcdColorProfiles.Keys)
            {

                var item = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(profileName), MyStringId.GetOrCompute(""), profileName);
                items.Add(item);

                if (profile == profileName)
                    selected.Add(item);

            }

        }

        public void SelectProfile(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> selected)
        {

            if (selected.Count == 0)
                return;

            SelectedProfile.Clear();
            SelectedProfile.Append((string)selected[0].UserData);

        }

        public void LoadProfile(IMyTerminalBlock block)
        {

            LcdColorProfile profile = null;

            if (!LcdColorProfiles.TryGetValue(SelectedProfile.ToString(), out profile))
                return;

            var SurfaceProvider = block as IMyTextSurfaceProvider;
            if (SurfaceProvider == null)
                return;

            if (SurfaceProvider.SurfaceCount == 0)
                return;

            for (int i = 0; i < SurfaceProvider.SurfaceCount; i++)
            {
                var surface = SurfaceProvider.GetSurface(i);
                surface.FontColor = profile.FontColor;
                surface.BackgroundColor = profile.BackgroundColor;
                surface.ScriptForegroundColor = profile.ScriptForegroundColor;
                surface.ScriptBackgroundColor = profile.ScriptBackgroundColor;
            }

        }

        public void SaveProfile(IMyTerminalBlock block)
        {

            var name = ProfileName.ToString();

            if (string.IsNullOrWhiteSpace(name) || name == "None")
                return;

            if (LcdColorProfiles.ContainsKey(name))
                LcdColorProfiles[name] = new LcdColorProfile(block as IMyTerminalBlock);
            else
                LcdColorProfiles.Add(name, new LcdColorProfile(block as IMyTerminalBlock));

            SaveToFile();

            RefreshMenu(block);

        }

        public void DeleteProfile(IMyTerminalBlock block)
        {

            var name = SelectedProfile.ToString();

            if (string.IsNullOrWhiteSpace(name) || name == "None")
                return;

            LcdColorProfiles.Remove(name);
            SaveToFile();
            RefreshMenu(block);


        }

        public void SaveToFile()
        {

            var byteData = MyAPIGateway.Utilities.SerializeToBinary<SortedDictionary<string, LcdColorProfile>>(LcdColorProfiles);
            StoredData.StoredData = Convert.ToBase64String(byteData);

            try
            {

                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage("LcdColorProfiles.mes", typeof(LcdColorProfileStorage)))
                {

                    writer.Write(MyAPIGateway.Utilities.SerializeToXML<LcdColorProfileStorage>(StoredData));

                }

            }
            catch (Exception exc)
            {



            }

        }

        public void RefreshMenu(IMyTerminalBlock block)
        {

            foreach (var control in ControlsListMaster)
                control.UpdateVisual(); //

        }

        public void AddControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {

            if (block as IMyTerminalBlock == null)
                return;

            var SurfaceProvider = block as IMyTextSurfaceProvider;
            if (SurfaceProvider == null)
                return;

            if (SurfaceProvider.SurfaceCount == 0)
                return;

            foreach (var control in ControlsListMaster)
                controls.Add(control);

        }


    }

}
