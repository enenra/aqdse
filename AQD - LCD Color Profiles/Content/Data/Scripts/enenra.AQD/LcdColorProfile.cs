using ProtoBuf;
using Sandbox.Definitions;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace AQD.LcdColorProfiles {

	[ProtoContract]
	public class LcdColorProfile {

		[ProtoMember(1)] public Color FontColor;
		[ProtoMember(2)] public Color BackgroundColor;
		[ProtoMember(3)] public Color ScriptForegroundColor;
		[ProtoMember(4)] public Color ScriptBackgroundColor;

		public LcdColorProfile() {



		}

		public LcdColorProfile(IMyTerminalBlock block) {

            var SurfaceProvider = block as IMyTextSurfaceProvider;
            if (SurfaceProvider == null)
                return;

            if (SurfaceProvider.SurfaceCount == 0)
                return;

            if (block as IMyTextPanel == null)
            {
                IMyMultiTextPanelComponentOwner ActiveSurfaceOwner = (IMyMultiTextPanelComponentOwner)SurfaceProvider;

                var activeIndex = ActiveSurfaceOwner.MultiTextPanel.SelectedPanelIndex;

                FontColor = SurfaceProvider.GetSurface(activeIndex).FontColor;
                BackgroundColor = SurfaceProvider.GetSurface(activeIndex).BackgroundColor;
                ScriptForegroundColor = SurfaceProvider.GetSurface(activeIndex).ScriptForegroundColor;
                ScriptBackgroundColor = SurfaceProvider.GetSurface(activeIndex).ScriptBackgroundColor;
            }
            else
            {
                IMyTextSurface Surface = (IMyTextSurface)block;

                FontColor = Surface.FontColor;
                BackgroundColor = Surface.BackgroundColor;
                ScriptForegroundColor = Surface.ScriptForegroundColor;
                ScriptBackgroundColor = Surface.ScriptBackgroundColor;
            }

		}

	}

	public class LcdColorProfileStorage {

		public string StoredData;

		public LcdColorProfileStorage() {

			StoredData = "";

		}

	}
}
