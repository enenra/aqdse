using System;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// LabelBoxButton modified to roughly match the appearance of buttons in the SE terminal.
    /// </summary>
    public class BorderedButton : LabelBoxButton
    {
        /// <summary>
        /// Color of the border surrounding the button
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// Thickness of the border surrounding the button
        /// </summary>
        public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

        /// <summary>
        /// Background highlight color
        /// </summary>
        public override Color HighlightColor { get; set; }

        /// <summary>
        /// Text color used when the control gains focus.
        /// </summary>
        public Color FocusTextColor { get; set; }

        /// <summary>
        /// Background color used when the control gains focus.
        /// </summary>
        public Color FocusColor { get; set; } 

        /// <summary>
        /// If true, then the button will change formatting when it takes focus.
        /// </summary>
        public bool UseFocusFormatting { get; set; }

        protected readonly BorderBox border;
        protected Color lastColor, lastTextColor;

        public BorderedButton(HudParentBase parent) : base(parent)
        {
            border = new BorderBox(this)
            {
                Thickness = 1f,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            AutoResize = false;
            Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center);
            FocusTextColor = TerminalFormatting.Charcoal;
            Text = "NewBorderedButton";

            TextPadding = new Vector2(32f, 0f);
            Padding = new Vector2(37f, 0f);
            Size = new Vector2(253f, 50f);
            HighlightEnabled = true;

            Color = TerminalFormatting.OuterSpace;
            HighlightColor = TerminalFormatting.Atomic;
            BorderColor = TerminalFormatting.LimedSpruce;
            FocusColor = TerminalFormatting.Mint;
            UseFocusFormatting = true;

            _mouseInput.GainedInputFocus += GainFocus;
            _mouseInput.LostInputFocus += LoseFocus;
        }

        public BorderedButton() : this(null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (MouseInput.HasFocus)
            {
                if (SharedBinds.Space.IsNewPressed)
                {
                    _mouseInput.OnLeftClick();
                }
            }
        }

        protected override void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (!(UseFocusFormatting && MouseInput.HasFocus))
                {
                    lastColor = Color;
                    lastTextColor = TextBoard.Format.Color;
                }

                TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
                Color = HighlightColor;
            }
        }

        protected override void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (UseFocusFormatting && MouseInput.HasFocus)
                {
                    Color = FocusColor;
                    TextBoard.SetFormatting(TextBoard.Format.WithColor(FocusTextColor));
                }
                else
                {
                    Color = lastColor;
                    TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
                }
            }
        }

        protected virtual void GainFocus(object sender, EventArgs args)
        {
            if (UseFocusFormatting)
            {
                if (!MouseInput.IsMousedOver)
                {
                    lastColor = Color;
                    lastTextColor = TextBoard.Format.Color;
                }

                Color = FocusColor;
                TextBoard.SetFormatting(TextBoard.Format.WithColor(FocusTextColor));
            }
        }

        protected virtual void LoseFocus(object sender, EventArgs args)
        {
            if (UseFocusFormatting)
            {
                Color = lastColor;
                TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
            }
        }
    }
}