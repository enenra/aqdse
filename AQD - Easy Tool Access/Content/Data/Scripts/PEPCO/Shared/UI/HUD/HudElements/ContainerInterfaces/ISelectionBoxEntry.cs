using System;
using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    /// <summary>
    /// HUD element container that functions as a list entry in Selection Box types.
    /// </summary>
    public interface ISelectionBoxEntry<TElement> : IScrollBoxEntry<TElement>
         where TElement : HudElementBase
    {
        bool AllowHighlighting { get; set; }

        void Reset();
    }

    /// <summary>
    /// HUD element container that functions as a list entry in Selection Box types with data tuples.
    /// </summary>
    public interface ISelectionBoxEntryTuple<TElement, TValue>
        : ISelectionBoxEntry<TElement>, IScrollBoxEntryTuple<TElement, TValue>
        where TElement : HudElementBase
    { }

}
