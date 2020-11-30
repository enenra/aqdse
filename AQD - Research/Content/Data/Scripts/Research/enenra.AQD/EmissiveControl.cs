using VRage.Game.Components;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;

namespace enenra.EmissiveControl
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), false,
        new string[]
        {
            "AQD_LG_ResearchLab"
        }
    )]
    public class Container : MyGameLogicComponent
    {
        Sandbox.ModAPI.IMyAssembler m_block;

        private string EMISSIVE_MATERIAL_NAME = "Emissive";
        private Color GREEN = new Color(0, 255, 0);
        private Color LIGHTBLUE = new Color(0, 255, 255, 255);
        private Color RED = new Color(255, 0, 0);
        private Color lastColor;


        bool IsWorking
        {
            get
            {
                return m_block.IsWorking;
            }
        }

        bool IsProducing
        {
            get
            {
                return m_block.IsProducing;
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                if (mod.PublishedFileId == 2244563617) // AQD - Visuals
                {
                    GREEN.R = 60;
                    GREEN.G = 163;
                    GREEN.B = 33;
                    RED.R = 171;
                    RED.G = 42;
                    RED.B = 29;
                    LIGHTBLUE.R = 54;
                    LIGHTBLUE.G = 90;
                    LIGHTBLUE.B = 161;
                }
                else if (mod.PublishedFileId == 2212516940) // Emissive Colors - Red / Green Color Vision Deficiency
                {
                    GREEN.R = 10;
                    GREEN.G = 255;
                    GREEN.B = 25;
                    LIGHTBLUE.R = 0;
                    LIGHTBLUE.G = 255;
                    LIGHTBLUE.B = 255;
                }
            }

            m_block = (Sandbox.ModAPI.IMyAssembler)Entity;
        }

        public override void Close()
        {
            m_block = null;
        }

        public override void UpdateAfterSimulation10()
        {
            if (MyAPIGateway.Session == null)
                return;

            if (MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
                return;

            if (IsWorking)
            {
                if (IsProducing)
                {
                    m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, LIGHTBLUE, 1f);
                    lastColor = LIGHTBLUE;
                }

                else
                {
                    m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, GREEN, 1f);
                    lastColor = GREEN;
                }
            }

            else
            {
                m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, RED, 1f);
                lastColor = RED;
            }

        }
    }
}