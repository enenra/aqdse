using System.Xml.Serialization;
using VRage;
using BindDefinitionData = VRage.MyTuple<string, string[]>;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Stores data for serializing individual key binds to XML.
        /// </summary>
        [XmlType(TypeName = "Bind")]
        public struct BindDefinition
        {
            [XmlAttribute]
            public string name;

            [XmlArray("Controls")]
            public string[] controlNames;

            public BindDefinition(string name, string[] controlNames)
            {
                this.name = name;
                this.controlNames = controlNames;
            }

            public static implicit operator BindDefinition(BindDefinitionData value)
            {
                return new BindDefinition(value.Item1, value.Item2);
            }

            public static implicit operator MyTuple<string, string[]>(BindDefinition value)
            {
                return new BindDefinitionData(value.name, value.controlNames);
            }
        }
    }
}