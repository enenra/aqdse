using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable button with a textured background.
    /// </summary>
    public class Button : TexturedBox, IClickableElement
    {
        /// <summary>
        /// Indicates whether or not the cursor is currently positioned over the button.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

        /// <summary>
        /// Handles mouse input for the button.
        /// </summary>
        public IMouseInput MouseInput => _mouseInput;

        /// <summary>
        /// Determines whether or not the button will highlight when moused over.
        /// </summary>
        public bool HighlightEnabled { get; set; }

        /// <summary>
        /// Color of the background when moused over.
        /// </summary>
        public Color HighlightColor { get; set; }

        protected readonly MouseInputElement _mouseInput;
        protected Color lastBackgroundColor;

        public Button(HudParentBase parent) : base(parent)
        {
            _mouseInput = new MouseInputElement(this);
            HighlightColor = new Color(125, 125, 125, 255);
            HighlightEnabled = true;

            _mouseInput.CursorEntered += CursorEnter;
            _mouseInput.CursorExited += CursorExit;
        }

        public Button() : this(null)
        { }

        protected virtual void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                lastBackgroundColor = Color;
                Color = HighlightColor;
            }
        }

        protected virtual void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                Color = lastBackgroundColor;
            }
        }
    }
}