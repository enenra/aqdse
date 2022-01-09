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

    private const string EMISSIVE_MATERIAL_NAME = "Emissive";
    private Color GREEN = new Color(0, 255, 0);
    private Color RED = new Color(255, 0, 0);

    bool IsWorking => m_block.IsWorking;

    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
      m_block = Entity as IMyThrust;

      var subtype = m_block.BlockDefinition.SubtypeName;
      if (!subtype.StartsWith("AQD_LG_") && !subtype.StartsWith("AQD_SG_"))
      {
        m_block = null;
        return;
      }

      NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

      m_block.IsWorkingChanged += OnIsWorkingChanged;
      base.Init(objectBuilder);
    }

    public override void UpdateOnceBeforeFrame()
    {
      bool aqdVisualsPresent = false;
      bool emissiveColorsPresent = false;

      foreach (var mod in MyAPIGateway.Session.Mods)
      {
        if (aqdVisualsPresent && emissiveColorsPresent)
          break;

        if (mod.PublishedFileId == 2711430394) // AQD - Emissive Colors
          aqdVisualsPresent = true;
        else if (mod.PublishedFileId == 2212516940) // Emissive Colors - Red / Green Color Vision Deficiency
          emissiveColorsPresent = true;
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

      base.UpdateOnceBeforeFrame();
    }

    public override void Close()
    {
      if (m_block != null)
      {
        m_block.IsWorkingChanged -= OnIsWorkingChanged;
        m_block = null;
      }

      base.Close();
    }

    private void OnIsWorkingChanged(VRage.Game.ModAPI.IMyCubeBlock obj)
    {
      if (MyAPIGateway.Session == null)
        return;

      if (MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
        return;

      var color = IsWorking ? GREEN : RED;
      m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, color, 1f);
    }
  }
}