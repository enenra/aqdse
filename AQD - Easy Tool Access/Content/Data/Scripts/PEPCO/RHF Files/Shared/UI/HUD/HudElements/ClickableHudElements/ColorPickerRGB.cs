using System;
using System.Text;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    using UI;

    /// <summary>
    /// Named color picker using sliders designed to mimic the appearance of the color picker in the SE terminal.
    /// RGB only. Alpha not supported.
    /// </summary>
    public class ColorPickerRGB : HudElementBase
    {
        /// <summary>
        /// Text rendered by the label
        /// </summary>
        public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        /// <summary>
        /// Text builder backing the label
        /// </summary>
        public ITextBuilder NameBuilder => name.TextBoard;

        /// <summary>
        /// Formatting used by the label
        /// </summary>
        public GlyphFormat NameFormat { get { return name.TextBoard.Format; } set { name.TextBoard.SetFormatting(value); } }

        /// <summary>
        /// Formatting used by the color value labels
        /// </summary>
        public GlyphFormat ValueFormat
        { 
            get { return sliderText[0].Format; } 
            set 
            {
                foreach (Label label in sliderText)
                    label.TextBoard.SetFormatting(value);
            } 
        }

        public override float Width
        {
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                _size.X = (value);
                display.Width = value - name.Width;
                colorSliderColumn.Width = display.Width;
            }
        }

        public override float Height
        {
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                _size.Y = (value);
                value = (value - headerChain.Height - 15f) / 3f;
                colorNameColumn.MemberMaxSize = new Vector2(colorNameColumn.MemberMaxSize.X, value);
                colorSliderColumn.MemberMaxSize = new Vector2(colorSliderColumn.MemberMaxSize.X, value);
            }
        }

        /// <summary>
        /// Color currently specified by the color picker
        /// </summary>
        public Color Color 
        { 
            get { return _color; }
            set 
            {
                sliders[0].Current = value.R;
                sliders[1].Current = value.G;
                sliders[2].Current = value.B;
                _color = value;
            }
        }

        // Header
        private readonly Label name;
        private readonly TexturedBox display;
        private readonly HudChain headerChain;
        // Slider text
        private readonly Label[] sliderText;
        private readonly HudChain<HudElementContainer<Label>, Label> colorNameColumn;
        // Sliders
        public readonly SliderBox[] sliders;
        private readonly HudChain<HudElementContainer<SliderBox>, SliderBox> colorSliderColumn;

        private readonly HudChain mainChain, colorChain;
        private readonly StringBuilder valueBuilder;
        private Color _color;
        private int focusedChannel;

        public ColorPickerRGB(HudParentBase parent) : base(parent)
        {
            // Header
            name = new Label()
            {
                Format = GlyphFormat.Blueish.WithSize(1.08f),
                Text = "NewColorPicker",
                AutoResize = false,
                Size = new Vector2(88f, 22f)
            };

            display = new TexturedBox()
            {
                Width = 231f,
                Color = Color.Black
            };

            var dispBorder = new BorderBox(display)
            {
                Color = Color.White,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            headerChain = new HudChain(false)
            {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                Height = 22f,
                Spacing = 0f,
                CollectionContainer = { name, display }
            };

            // Color picker
            sliderText = new Label[]
            {
                new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f },
                new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f },
                new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f }
            };

            colorNameColumn = new HudChain<HudElementContainer<Label>, Label>(true)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 87f,
                Spacing = 5f,
                CollectionContainer = { sliderText[0], sliderText[1], sliderText[2] }
            };

            sliders = new SliderBox[] 
            {
                new SliderBox() { Min = 0f, Max = 255f, Height = 47f },
                new SliderBox() { Min = 0f, Max = 255f, Height = 47f },
                new SliderBox() { Min = 0f, Max = 255f, Height = 47f }
            };

            colorSliderColumn = new HudChain<HudElementContainer<SliderBox>, SliderBox>(true)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 231f,
                Spacing = 5f,
                CollectionContainer = { sliders[0], sliders[1], sliders[2] }
            };

            colorChain = new HudChain(false)
            {
                SizingMode = HudChainSizingModes.FitChainBoth,
                CollectionContainer =
                {
                    colorNameColumn,
                    colorSliderColumn,
                }
            };

            mainChain = new HudChain(true, this)
            {
                SizingMode = HudChainSizingModes.FitChainBoth,
                Spacing = 5f,
                CollectionContainer =
                {
                    headerChain,
                    colorChain,
                }
            };

            Size = new Vector2(318f, 163f);
            valueBuilder = new StringBuilder();

            UseCursor = true;
            ShareCursor = true;
            focusedChannel = -1;
        }

        public ColorPickerRGB() : this(null)
        { }

        /// <summary>
        /// Set focus for slider corresponding to the given color channel index [0, 2].
        /// </summary>
        public void SetChannelFocused(int channel)
        {
            channel = MathHelper.Clamp(channel, 0, 2);

            if (!sliders[channel].MouseInput.HasFocus)
                focusedChannel = channel;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (focusedChannel != -1)
            {
                sliders[focusedChannel].MouseInput.GetInputFocus();
                focusedChannel = -1;
            }

            for (int i = 0; i < sliders.Length; i++)
            {
                if (sliders[i].MouseInput.HasFocus)
                {
                    if (SharedBinds.UpArrow.IsNewPressed)
                    {
                        i = MathHelper.Clamp(i - 1, 0, sliders.Length - 1);
                        sliders[i].MouseInput.GetInputFocus();
                    }
                    else if (SharedBinds.DownArrow.IsNewPressed)
                    {
                        i = MathHelper.Clamp(i + 1, 0, sliders.Length - 1);
                        sliders[i].MouseInput.GetInputFocus();
                    }

                    break;
                }
            }

            _color = new Color()
            {
                R = (byte)Math.Round(sliders[0].Current),
                G = (byte)Math.Round(sliders[1].Current),
                B = (byte)Math.Round(sliders[2].Current),
                A = 255
            };

            valueBuilder.Clear();
            valueBuilder.Append("R: ");
            valueBuilder.Append(_color.R);
            sliderText[0].TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("G: ");
            valueBuilder.Append(_color.G);
            sliderText[1].TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("B: ");
            valueBuilder.Append(_color.B);
            sliderText[2].TextBoard.SetText(valueBuilder);

            display.Color = _color;
        }
    }
}