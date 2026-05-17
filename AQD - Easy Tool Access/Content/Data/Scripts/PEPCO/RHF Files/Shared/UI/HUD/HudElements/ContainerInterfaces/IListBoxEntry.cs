using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Interface implemented by objects that function as list box entries.
    /// </summary>
    public interface IListBoxEntry<TElement, TValue>
        : ISelectionBoxEntryTuple<TElement, TValue>
        where TElement : HudElementBase, IMinLabelElement
    {
        object GetOrSetMember(object data, int memberEnum);
    }
}