using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using VRage.Game;
using VRage.ObjectBuilders;

namespace AQDResearch
{

  public class SerialId
  {
    [XmlAttribute("Type")]
    public string TypeId;

    [XmlAttribute("SubtypeId")]
    public string SubtypeId;

    [XmlAttribute("UseBlockVariantsGroup")]
    public bool UseBlockVariantsGroup;

    [XmlIgnore]
    MyDefinitionId _definitionId = new MyDefinitionId();

    public SerialId() { }

    public SerialId(SerializableDefinitionId id)
    {
      bool isNull = id.IsNull();
      TypeId = isNull ? "MyObjectBuilder_TypeGoesHere" : id.TypeIdString;
      SubtypeId = isNull ? "SubtypeGoesHere" : id.SubtypeId;
    }

    public MyDefinitionId DefinitionId
    {
      get
      {
        if (_definitionId.TypeId.IsNull)
        {
          MyObjectBuilderType typeId;
          if (!MyObjectBuilderType.TryParse(TypeId, out typeId))
            throw new Exception($"Incorrect TypeId given for Definition: '{TypeId}'");

          _definitionId = new MyDefinitionId(typeId, SubtypeId);
        }

        return _definitionId;
      }
    }
  }

  public class ResearchGroup
  {
    [XmlElement("Item")]
    public SerialId ComponentId;

    [XmlElement("OverwriteOthers")]
    public bool OverwriteAllOthers;

    [XmlArrayItem("Id")]
    public List<SerialId> BlockDefinitons { get; set; } = new List<SerialId>();

    public ResearchGroup() { }

    public ResearchGroup(SerializableDefinitionId comp, List<SerializableDefinitionId> defs)
    {
      ComponentId = new SerialId(comp);

      if (defs?.Count > 0)
      {
        foreach (var item in defs)
          BlockDefinitons.Add(new SerialId(item));
      }
    }
  }

  [XmlType("ResearchGroups")]
  public class ResearchGroupSettings
  {
    public bool AutoAdjustAssemblerEfficiency { get; set; } = true;

    [XmlElement("ResearchGroup", typeof(ResearchGroup))]
    public List<ResearchGroup> ResearchGroupList { get; set; } = new List<ResearchGroup>();

    [XmlIgnore]
    public Dictionary<SerializableDefinitionId, ResearchGroup> ResearchGroups { get; set; } = new Dictionary<SerializableDefinitionId, ResearchGroup>();

    public void AddResearchGroup(ResearchGroup group)
    {
      MyObjectBuilderType typeId;
      if (!MyObjectBuilderType.TryParse(group.ComponentId.TypeId, out typeId))
        return;

      var compDef = new MyDefinitionId(typeId, group.ComponentId.SubtypeId);

      ResearchGroup existing;
      if (!group.OverwriteAllOthers && ResearchGroups.TryGetValue(compDef, out existing))
      {
        foreach (var item in group.BlockDefinitons)
        {
          if (!existing.BlockDefinitons.Contains(item))
            existing.BlockDefinitons.Add(item);
        }
      }
      else
      {
        ResearchGroups[compDef] = group;
        ResearchGroupList.Add(group);
      }
    }

    public void AddResearchGroup(SerializableDefinitionId compDef, List<SerializableDefinitionId> defList)
    {
      var group = new ResearchGroup(compDef, defList);
      ResearchGroups[compDef] = group;
      ResearchGroupList.Add(group);
    }

    public void Close()
    {
      if (ResearchGroups != null)
      {
        foreach (var item in ResearchGroups.Values)
          item?.BlockDefinitons?.Clear();
      }

      if (ResearchGroupList != null)
      {
        foreach (var item in ResearchGroupList)
          item?.BlockDefinitons?.Clear();
      }

      ResearchGroups?.Clear();
      ResearchGroupList?.Clear();
      ResearchGroups = null;
      ResearchGroupList = null;
    }
  }
}
