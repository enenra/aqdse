using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GyroUpgrade
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Gyro), false)]
    public class MyGyroUpgradeLogic : MyGameLogicComponent
    {
        private IMyGyro m_gyro;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            m_gyro = Entity as IMyGyro;

            if (m_gyro.BlockDefinition.SubtypeName == "AQD_LG_LargeGyro")
            {
                m_gyro.AddUpgradeValue("GyroUpgrade", 1f);
                m_gyro.OnUpgradeValuesChanged += OnUpgradeValuesChangedUpgrade;
            }
            else
            {
                m_gyro.AddUpgradeValue("GyroBoost", 1f);
                m_gyro.OnUpgradeValuesChanged += OnUpgradeValuesChangedBooster;
            }

        }

        private void OnUpgradeValuesChangedUpgrade()
        {
            m_gyro.GyroStrengthMultiplier = m_gyro.UpgradeValues["GyroUpgrade"];
            m_gyro.PowerConsumptionMultiplier = m_gyro.UpgradeValues["GyroUpgrade"];
        }

        private void OnUpgradeValuesChangedBooster()
        {
            m_gyro.GyroStrengthMultiplier = m_gyro.UpgradeValues["GyroBoost"];
            m_gyro.PowerConsumptionMultiplier = m_gyro.UpgradeValues["GyroBoost"];
        }
    }
}
