using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable button with text and a textured background. 
    /// </summary>
    public class LabelBoxButton : LabelBox, IClickableElement
    {
        /// <summary>
        /// Color of the background when moused over.
        /// </summary>
        public virtual Color HighlightColor { get; set; }

        /// <summary>
        /// Determines whether or not the button will highlight when moused over.
        /// </summary>
        public virtual bool HighlightEnabled { get; set; }

        /// <summary>
        /// Indicates whether or not the cursor is currently over the element.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

        /// <summary>
        /// Mouse input for the button.
        /// </summary>
        public IMouseInput MouseInput => _mouseInput;

        protected MouseInputElement _mouseInput;
        protected Color oldColor;

        public LabelBoxButton(HudParentBase parent) : base(parent)
        {
            _mouseInput = new MouseInputElement(this);
            Color = Color.DarkGray;
            HighlightColor = Color.Gray;
            HighlightEnabled = true;

            _mouseInput.CursorEntered += CursorEnter;
            _mouseInput.CursorExited += CursorExit;
        }

        public LabelBoxButton() : this(null)
        { }

        protected virtual void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                oldColor = Color;
                Color = HighlightColor;
            }
        }

        protected virtual void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                Color = oldColor;
            }
        }
    }
}