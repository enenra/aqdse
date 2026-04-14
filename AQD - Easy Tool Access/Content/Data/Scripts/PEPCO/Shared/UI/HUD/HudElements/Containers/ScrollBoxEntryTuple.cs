namespace RichHudFramework.UI
{
    /// <summary>
    /// Basic container class used to associate a scrollbox element with an arbitrary object
    /// of type TData.
    /// </summary>
    public class ScrollBoxEntryTuple<TElement, TData> 
        : ScrollBoxEntry<TElement>, IScrollBoxEntryTuple<TElement, TData> 
        where TElement : HudElementBase
    {
        /// <summary>
        /// Object associated with the entry
        /// </summary>
        public virtual TData AssocMember { get; set; }

        public ScrollBoxEntryTuple()
        { }
    }

    /// <summary>
    /// Basic container class used to associate a scrollbox element with an arbitrary object
    /// of type TData.
    /// </summary>
    public class ScrollBoxEntryTuple<TData> : ScrollBoxEntryTuple<HudElementBase, TData> 
    { }
}