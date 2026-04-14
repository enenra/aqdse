namespace RichHudFramework.UI
{
    /// <summary>
    /// A bare HUD element that renders nothing, no graphics, no text.
    /// </summary>
    public class EmptyHudElement : HudElementBase
    {
        public EmptyHudElement(HudParentBase parent) : base(parent)
        { }

        public EmptyHudElement() : this(null)
        { }
    }
}