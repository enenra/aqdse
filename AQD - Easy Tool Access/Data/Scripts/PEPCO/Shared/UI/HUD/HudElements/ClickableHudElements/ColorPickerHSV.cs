using RichHudFramework.UI.Rendering;
using System.Text;
using System;
using VRageMath;
using VRage.Game;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Named color picker using sliders designed to mimic the appearance of the color picker in the SE terminal.
    /// RGB only. Alpha not supported.
    /// </summary>
    public class ColorPickerHSV : HudElementBase
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
        /// Color currently specified by the color picker. Formatted as non-normalized, offset HSV.
        /// Max: [360, 100, 100]
        /// </summary>
        public Vector3 Color
        {
            get { return _color; }
            set
            {
                sliders[0].Current = value.X;
                sliders[1].Current = value.Y;
                sliders[2].Current = value.Z;
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
        private Vector3 _color;
        private int focusedChannel;

        public ColorPickerHSV(HudParentBase parent) : base(parent)
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
                Color = VRageMath.Color.Black
            };

            var dispBorder = new BorderBox(display)
            {
                Color = VRageMath.Color.White,
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
                new SliderBox() { Min = 0f, Max = 360f, Height = 47f },
                new SliderBox() { Min = 0f, Max = 100f, Height = 47f },
                new SliderBox() { Min = 0f, Max = 100f, Height = 47f }
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

        public ColorPickerHSV() : this(null)
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

        protected override void Layout()
        {
            _color = new Vector3()
            {
                X = sliders[0].Current,
                Y = sliders[1].Current,
                Z = sliders[2].Current,
            };

            valueBuilder.Clear();
            valueBuilder.Append("H: ");
            valueBuilder.Append(Math.Round(_color.X, 1));
            sliderText[0].TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("S: ");
            valueBuilder.Append(Math.Round(_color.Y, 1));
            sliderText[1].TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("V: ");
            valueBuilder.Append(Math.Round(_color.Z, 1));
            sliderText[2].TextBoard.SetText(valueBuilder);

            display.Color = (_color / new Vector3(360f, 100f, 100f)).HSVtoColor();
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
        }
    }
}