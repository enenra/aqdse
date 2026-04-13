namespace RichHudFramework.UI
{
    /// <summary>
    /// Interface for scrollbox entry containers.
    /// </summary>
    public interface IScrollBoxEntry<TElement> : IHudElementContainer<TElement> 
        where TElement : HudElementBase
    {
        bool Enabled { get; set; }
    }

    public interface IScrollBoxEntryTuple<TElement, TData> : IScrollBoxEntry<TElement> 
        where TElement : HudElementBase
    {
        TData AssocMember { get; set; }
    }
}