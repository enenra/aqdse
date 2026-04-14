using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Components;

namespace SISK.LoadLocalization

{    /// <summary>
     ///     Main session component.
     /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class Mod : MySessionComponentBase
    {

        /// <summary>
        ///     Language used to localize this mod.
        /// </summary>
        public MyLanguagesEnum? Language { get; private set; }

        /// <summary>
        ///     Create localizations.
        /// </summary>
        public override void LoadData()
        {
            LoadLocalization();

            MyAPIGateway.Gui.GuiControlRemoved += OnGuiControlRemoved;
        }

        /// <summary>
        ///     Unloads all data.
        /// </summary>
        protected override void UnloadData()
        {
            MyAPIGateway.Gui.GuiControlRemoved -= OnGuiControlRemoved;
        }

        /// <summary>
        ///     Load localizations for this mod.
        /// </summary>
        private void LoadLocalization()
        {
            var path = Path.Combine(ModContext.ModPathData, "Localization");
            var supportedLanguages = new HashSet<MyLanguagesEnum>();
            MyTexts.LoadSupportedLanguages(path, supportedLanguages);

            var currentLanguage = supportedLanguages.Contains(MyAPIGateway.Session.Config.Language) ? MyAPIGateway.Session.Config.Language : MyLanguagesEnum.English;
            if (Language != null && Language == currentLanguage)
            {
                return;
            }

            Language = currentLanguage;
            var languageDescription = MyTexts.Languages.Where(x => x.Key == currentLanguage).Select(x => x.Value).FirstOrDefault();
            if (languageDescription != null)
            {
                var cultureName = string.IsNullOrWhiteSpace(languageDescription.CultureName) ? null : languageDescription.CultureName;
                var subcultureName = string.IsNullOrWhiteSpace(languageDescription.SubcultureName) ? null : languageDescription.SubcultureName;

                MyTexts.LoadTexts(path, cultureName, subcultureName);
            }
        }

        /// <summary>
        ///     Event triggered on gui control removed.
        ///     Used to detect if Option screen is closed and then to reload localization.
        /// </summary>
        private void OnGuiControlRemoved(object obj)
        {
            if (obj.ToString().EndsWith("ScreenOptionsSpace"))
            {
                LoadLocalization();
            }
        }
    }
}