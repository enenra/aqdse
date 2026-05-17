namespace RichHudFramework.UI
{
    /// <summary>
    /// Interface for objects acting as containers of UI elements
    /// </summary>
    public interface IHudElementContainer<TElement> where TElement : HudNodeBase
    {
        /// <summary>
        /// HUD Element associated with the container
        /// </summary>
        TElement Element { get; }

        /// <summary>
        /// Sets the element associated with the container. Should only
        /// allow one assignment.
        /// </summary>
        void SetElement(TElement Element);
    }
}
