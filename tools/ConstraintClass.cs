using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;

using System;
using System.Collections.Generic;

using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Constraints
{
  // Add your type and subtype(s) here
  [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CargoContainer), false, "AQD_LG_Hopper", "AQD_SG_Hopper")]
  public class ConstraintClass : MyGameLogicComponent
  {
    MyCubeBlock _block;

    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
      _block = Entity as MyCubeBlock;
      NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
      base.Init(objectBuilder);
    }

    public override void UpdateOnceBeforeFrame()
    {
      try
      {
        // Comment one of the below sections out as needed

        {
          // To add all of a given type
          // Simply use typeof() and pass in the MyObjectBuilder_TypeHere
          // Last arg is the constraint name, in case one needs to be created
          AddConstraintForType(_block, typeof(MyObjectBuilder_Ore), "AQDConstraint");
        }

        //{
          // To add specific ids based on criteria
        //  HashSet<MyDefinitionId> constraintIds = new HashSet<MyDefinitionId>();

        //  foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
        //  {
            // check your criteria here
        //    MyPhysicalItemDefinition physDef = def as MyPhysicalItemDefinition;
        //    if (physDef != null && physDef.IsOre)
        //    {
        //      constraintIds.Add(physDef.Id);
        //    }
        //  }

        //  AddConstraintFromCollection(_block, constraintIds, "AQDConstraint");

        //  constraintIds.Clear();
        //  constraintIds = null;
        //}
      }
      catch (Exception ex)
      {
        MyLog.Default.Error(ex.ToString());
      }
      finally
      {
        base.UpdateOnceBeforeFrame();
      }
    }

    /// <summary>
    /// Adds all of a given object builder type to a constraint. Will create the constraint if needed.
    /// </summary>
    /// <param name="block">The block to add the constraints to</param>
    /// <param name="builderType">The <see cref="MyObjectBuilderType"/> of items to add to the constraint</param>
    /// <param name="constraintName">The name of the constraint, if one needs to be created</param>
    void AddConstraintForType(MyCubeBlock block, MyObjectBuilderType builderType, string constraintName)
    {
      if (block.InventoryCount == 0)
        return;

      for (int i = 0; i < block.InventoryCount; i++)
      {
        var inv = block.GetInventory(i);
        MyInventoryConstraint constraint = inv.Constraint;
        if (constraint == null)
        {
          constraint = new MyInventoryConstraint(constraintName, whitelist: true);
          inv.Constraint = constraint;
        }

        constraint.Clear();
        constraint.AddObjectBuilderType(builderType);
      }
    }

    /// <summary>
    /// Adds IDs from a collection to a constraint. Will create the constraint if needed.
    /// </summary>
    /// <param name="block">The block to add the constraints to</param>
    /// <param name="collection">The collection of IDs to add to the constraint</param>
    /// <param name="constraintName">The name of the constraint, if one needs to be created</param>
    void AddConstraintFromCollection(MyCubeBlock block, IEnumerable<MyDefinitionId> collection, string constraintName)
    {
      if (block.InventoryCount == 0)
        return;

      for (int i = 0; i < block.InventoryCount; i++)
      {
        var inv = block.GetInventory(i);
        MyInventoryConstraint constraint = inv.Constraint;
        if (constraint == null)
        {
          constraint = new MyInventoryConstraint(constraintName, whitelist: true);
          inv.Constraint = constraint;
        }

        constraint.Clear();

        foreach (var id in collection)
          constraint.Add(id);
      }
    }
  }
}
