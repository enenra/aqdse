namespace RichHudFramework.UI
{
    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudElementContainer<TElement> : IHudElementContainer<TElement> where TElement : HudNodeBase
    {
        public virtual TElement Element { get; private set; }

        public HudElementContainer()
        { }

        public virtual void SetElement(TElement element)
        {
            if (Element == null)
                Element = element;
            else
                throw new System.Exception("Only one element can ever be associated with a container object.");
        }
    }

    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudElementContainer : HudElementContainer<HudElementBase>
    { }
}
