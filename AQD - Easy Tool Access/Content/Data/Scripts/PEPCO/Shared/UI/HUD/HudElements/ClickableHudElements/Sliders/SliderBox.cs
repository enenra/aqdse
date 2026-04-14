using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Horizontal slider designed to mimic the appearance of the slider in the SE terminal.
    /// </summary>
    public class SliderBox : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Lower limit.
        /// </summary>
        public float Min { get { return slide.Min; } set { slide.Min = value; } }

        /// <summary>
        /// Upper limit.
        /// </summary>
        public float Max { get { return slide.Max; } set { slide.Max = value; } }

        /// <summary>
        /// Current value. Clamped between min and max.
        /// </summary>
        public float Current { get { return slide.Current; } set { slide.Current = value; } }

        /// <summary>
        /// Current value expressed as a percentage over the range between the min and max values.
        /// </summary>
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        /// <summary>
        /// Border size. Included in total element size.
        /// </summary>
        public override Vector2 Padding { get { return slide.Padding; } set { slide.Padding = value; } }

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return slide.BarColor; } set { slide.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return slide.BarHighlight; } set { slide.BarHighlight = value; } }

        /// <summary>
        /// Color of the bar when it has input focus
        /// </summary>
        public Color BarFocusColor { get; set; }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return slide.SliderColor; } set { slide.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return slide.SliderHighlight; } set { slide.SliderHighlight = value; } }

        /// <summary>
        /// Color of the slider when it has input focus
        /// </summary>
        public Color SliderFocusColor { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        public Color BackgroundColor { get { return background.Color; } set { background.Color = value; } }

        /// <summary>
        /// Background color when the slider box is moused over
        /// </summary>
        public Color BackgroundHighlight { get; set; }

        /// <summary>
        /// Background color when the slider box has input focus
        /// </summary>
        public Color BackgroundFocusColor { get; set; }

        /// <summary>
        /// Border color
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// If true then the slider box will change color when moused over
        /// </summary>
        public bool HighlightEnabled { get; set; }

        /// <summary>
        /// If true, then the slider box will change formatting when it takes focus.
        /// </summary>
        public bool UseFocusFormatting { get; set; }

        public IMouseInput MouseInput => slide.MouseInput;

        public override bool IsMousedOver => slide.IsMousedOver;

        protected readonly TexturedBox background;
        protected readonly BorderBox border;
        protected readonly SliderBar slide;

        protected Color lastBarColor, lastSliderColor, lastBackgroundColor;

        public SliderBox(HudParentBase parent) : base(parent)
        {
            background = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.Both
            };

            border = new BorderBox(background)
            {
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            slide = new SliderBar(this)
            {
                DimAlignment = DimAlignments.Both,
                SliderSize = new Vector2(14f, 28f),
                BarHeight = 5f
            };

            BackgroundColor = TerminalFormatting.OuterSpace;
            BorderColor = TerminalFormatting.LimedSpruce;
            BackgroundHighlight = TerminalFormatting.Atomic;
            BackgroundFocusColor = TerminalFormatting.Mint;

            SliderColor = TerminalFormatting.MistBlue;
            SliderHighlight = Color.White;
            SliderFocusColor = TerminalFormatting.Cinder;

            BarColor = TerminalFormatting.MidGrey;
            BarHighlight = Color.White;
            BarFocusColor = TerminalFormatting.BlackPerl;

            UseFocusFormatting = true;
            HighlightEnabled = true;

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);

            slide.MouseInput.CursorEntered += CursorEnter;
            slide.MouseInput.CursorExited += CursorExit;
            slide.MouseInput.GainedInputFocus += GainFocus;
            slide.MouseInput.LostInputFocus += LoseFocus;
        }

        public SliderBox() : this(null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (MouseInput.HasFocus)
            {
                if (SharedBinds.LeftArrow.IsNewPressed || SharedBinds.LeftArrow.IsPressedAndHeld)
                {
                    Percent -= 0.01f;
                }
                else if (SharedBinds.RightArrow.IsNewPressed || SharedBinds.RightArrow.IsPressedAndHeld)
                {
                    Percent += 0.01f;
                }
            }
        }

        protected virtual void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (!(UseFocusFormatting && slide.MouseInput.HasFocus))
                {
                    lastBarColor = BarColor;
                    lastSliderColor = SliderColor;
                    lastBackgroundColor = BackgroundColor;
                }

                SliderColor = SliderHighlight;
                BarColor = BarHighlight;
                BackgroundColor = BackgroundHighlight;
            }
        }

        protected virtual void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (UseFocusFormatting && slide.MouseInput.HasFocus)
                {
                    SliderColor = SliderFocusColor;
                    BarColor = BarFocusColor;
                    BackgroundColor = BackgroundFocusColor;
                }
                else
                {
                    SliderColor = lastSliderColor;
                    BarColor = lastBarColor;
                    BackgroundColor = lastBackgroundColor;
                }
            }
        }

        protected virtual void GainFocus(object sender, EventArgs args)
        {
            if (UseFocusFormatting && !MouseInput.IsMousedOver)
            {
                lastBarColor = BarColor;
                lastSliderColor = SliderColor;
                lastBackgroundColor = BackgroundColor;

                SliderColor = SliderFocusColor;
                BarColor = BarFocusColor;
                BackgroundColor = BackgroundFocusColor;
            }
        }

        protected virtual void LoseFocus(object sender, EventArgs args)
        {
            if (UseFocusFormatting)
            {
                SliderColor = lastSliderColor;
                BarColor = lastBarColor;
                BackgroundColor = lastBackgroundColor;
            }
        }
    }
}