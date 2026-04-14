namespace RichHudFramework.UI
{
    /// <summary>
    /// Basic container class used to associate a <see cref="HudChain"/> element with an arbitrary object
    /// of type TData.
    /// </summary>
    public class HudElementTuple<TElement, TData> : HudElementContainer<TElement> where TElement : HudElementBase
    {
        public virtual TData AssocData { get; set; }

        public HudElementTuple()
        { }
    }

    /// <summary>
    /// Basic container class used to associate a <see cref="HudChain"/> element with an arbitrary object
    /// of type TData.
    /// </summary>
    public class HudElementTuple<TData> : HudElementTuple<HudElementBase, TData> 
    { }
}
