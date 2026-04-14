using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
    /// <summary>
    /// An interface for clickable UI elements that represent of ListBoxEntry elements.
    /// </summary>
    public interface IEntryBox<TContainer, TElement> : IEnumerable<TContainer>, IReadOnlyHudElement
        where TContainer : IScrollBoxEntry<TElement>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
        /// <summary>
        /// Invoked when a member of the list is selected.
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        IReadOnlyList<TContainer> EntryList { get; }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        TContainer Selection { get; }
    }

    public interface IEntryBox<TValue> : IEntryBox<ListBoxEntry<TValue>, Label>
    { }
}