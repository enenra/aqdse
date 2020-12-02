using System;
using System.Text;

using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;

using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;

namespace AQDResearch
{
  [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
  public class Session : MySessionComponentBase
  {
    Logger _logger;
    ResearchGroupSettings _researchSettings;
    MyDefinitionId _researchLab, _dataStorage;
    public bool IsServer, IsDedicatedServer;

    protected override void UnloadData()
    {
      try
      {
        UnloadLocal();
      }
      finally
      {
        base.UnloadData();
      }
    }

    void UnloadLocal()
    {
      MyEntities.OnEntityCreate -= MyEntities_OnEntityCreate;

      _logger?.Close();
      _researchSettings?.Close();
    }

    public override void BeforeStart()
    {
      try
      {
        IsServer = MyAPIGateway.Multiplayer.IsServer;
        IsDedicatedServer = MyAPIGateway.Utilities.IsDedicated;

        _logger = new Logger("AQDResearch.log", IsDedicatedServer);
        _researchLab = new MyDefinitionId(typeof(MyObjectBuilder_Assembler), "AQD_LG_ResearchLab");
        _dataStorage = new MyDefinitionId(typeof(MyObjectBuilder_CargoContainer), "AQD_LG_DataStorage");

        _researchSettings = new ResearchGroupSettings();

        StringBuilder sb = new StringBuilder();
        foreach (var def in MyDefinitionManager.Static.GetEntityComponentDefinitions())
        {
          if (def.Id.SubtypeName.StartsWith("ProgressionFramework"))
          {
            sb.Clear();
            foreach (var ch in def.DescriptionText)
            {
              if (ch == '[')
                sb.Append('<');
              else if (ch == ']')
                sb.Append('>');
              else
                sb.Append(ch);
            }

            var buffer = sb.ToString().Trim();
            try
            {
              var settings = MyAPIGateway.Utilities.SerializeFromXML<ResearchGroupSettings>(buffer);

              foreach (var group in settings.ResearchGroupList)
              {
                _researchSettings.AddResearchGroup(group);
              }
            }
            catch (Exception ex)
            {
              _logger.Log($"Error trying to deserialize EntityComponentDefinition: {ex.Message}\n{ex.StackTrace}\nBuffer = \n{buffer}\n", MessageType.ERROR);
            }
          }
        }

        var ents = MyEntities.GetEntities();
        foreach (var e in ents)
          ProcessEntity(e);

        MyEntities.OnEntityCreate += MyEntities_OnEntityCreate;
      }
      catch(Exception ex)
      {
        _logger?.Log($"Exception in AQDResearch.BeforeStart: {ex.Message}\n{ex.StackTrace}", MessageType.ERROR);
        UnloadLocal();
      }

      base.BeforeStart();
    }

    void ProcessEntity(MyEntity obj)
    {
      var grid = obj as MyCubeGrid;
      if (grid == null)
        return;

      foreach (var block in grid.GetFatBlocks())
      {
        if (block.InventoryCount > 0)
          MyEntities_OnEntityCreate(block);
      }
    }

    private void MyEntities_OnEntityCreate(MyEntity obj)
    {
      var cubeblock = obj as MyCubeBlock;
      if (cubeblock == null || cubeblock.InventoryCount == 0)
        return;

      bool isDataBlock = cubeblock.BlockDefinition.Id == _dataStorage;
      bool allowed = isDataBlock || cubeblock.BlockDefinition.Id == _researchLab;

      for (int i = 0; i < cubeblock.InventoryCount; i++)
      {
        var inv = cubeblock.GetInventory(i);
        MyInventoryConstraint constraint = inv.Constraint;
        if (constraint == null)
        {
          constraint = new MyInventoryConstraint("AQDConstraint", whitelist: isDataBlock);
          inv.Constraint = constraint;
        }

        if (constraint.IsWhitelist)
        {
          if (allowed)
          {
            foreach (var group in _researchSettings.ResearchGroupList)
              constraint.Add(group.ComponentId.DefinitionId);
          }
        }
        else if (!allowed)
        {
          foreach (var group in _researchSettings.ResearchGroupList)
            constraint.Add(group.ComponentId.DefinitionId);
        }
      }
    }
  }
}
