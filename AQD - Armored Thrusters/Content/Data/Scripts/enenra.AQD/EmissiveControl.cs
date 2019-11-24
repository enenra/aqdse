using VRage.Game.Components;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;

namespace enenra.EmissiveControl
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false)]

    public class Container : MyGameLogicComponent
    {
        IMyThrust m_block;

        private string EMISSIVE_MATERIAL_NAME = "Emissive";
        private Color GREEN = new Color(0, 255, 0);
        private Color RED = new Color(255, 0, 0);
        private Color lastColor;


        bool IsWorking
        {
            get
            {
                return m_block.IsWorking;
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;

            m_block = Entity as IMyThrust;
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
                m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, GREEN, 1f);
                lastColor = GREEN;
            }

            else
            {
                m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, RED, 1f);
                lastColor = RED;
            }

        }
    }
}