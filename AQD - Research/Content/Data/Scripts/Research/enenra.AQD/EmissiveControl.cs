using VRage.Game.Components;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;

namespace enenra.EmissiveControl
{
  [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), false, "AQD_LG_ResearchLab")]
  public class Container : MyGameLogicComponent
  {
    Sandbox.ModAPI.IMyAssembler m_block;

    private const string EMISSIVE_MATERIAL_NAME = "Emissive";
    private Color GREEN = new Color(0, 255, 0);
    private Color LIGHTBLUE = new Color(0, 255, 255, 255);
    private Color RED = new Color(255, 0, 0);

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
      NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
      m_block = (Sandbox.ModAPI.IMyAssembler)Entity;
      m_block.StartedProducing += OnStateChanged;
      m_block.StoppedProducing += OnStateChanged;
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

        if (mod.PublishedFileId == 2244563617) // AQD - Visuals
          aqdVisualsPresent = true;
        else if (mod.PublishedFileId == 2711430394) // Emissive Colors - Red / Green Color Vision Deficiency
          emissiveColorsPresent = true;
      }

      if (aqdVisualsPresent)
      {
        RED = new Color(171, 42, 29);

        GREEN = emissiveColorsPresent ? new Color(10, 255, 25) : new Color(60, 163, 33);
        LIGHTBLUE = emissiveColorsPresent ? new Color(0, 255, 255) : new Color(54, 90, 161);
      }
      else if (emissiveColorsPresent)
      {
        GREEN = new Color(10, 255, 25);
        LIGHTBLUE = new Color(0, 255, 255);
      }

      base.UpdateOnceBeforeFrame();
    }

    public override void Close()
    {
      m_block.StartedProducing -= OnStateChanged;
      m_block.StoppedProducing -= OnStateChanged;
      m_block.IsWorkingChanged -= OnIsWorkingChanged;
      m_block = null;
      base.Close();
    }

    private void OnIsWorkingChanged(VRage.Game.ModAPI.IMyCubeBlock obj)
    {
      OnStateChanged();
    }

    private void OnStateChanged()
    {
      if (MyAPIGateway.Session == null)
        return;

      if (MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer)
        return;

      var color = IsWorking ? (IsProducing ? LIGHTBLUE : GREEN) : RED;
      m_block.SetEmissiveParts(EMISSIVE_MATERIAL_NAME, color, 1f);
    }
  }
}