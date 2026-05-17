namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable text element. Text only, no background.
    /// </summary>
    public class LabelButton : Label, IClickableElement
    {
        /// <summary>
        /// Handles mouse input for the button.
        /// </summary>
        public IMouseInput MouseInput => _mouseInput;

        /// <summary>
        /// Indicates whether or not the cursor is currently positioned over the button.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

        protected MouseInputElement _mouseInput;

        public LabelButton(HudParentBase parent) : base(parent)
        {
            _mouseInput = new MouseInputElement(this);
        }

        public LabelButton() : this(null)
        { }
    }
}