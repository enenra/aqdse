using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A pair of horizontally aligned on and off bordered buttons used to indicate a boolean value. Made to
    /// resemble on/off button used in the SE terminal, sans name tag.
    /// </summary>
    public class OnOffButton : HudElementBase, IClickableElement
    {   
        /// <summary>
        /// Distance between the on and off buttons
        /// </summary>
        public float ButtonSpacing { get { return buttonChain.Spacing; } set { buttonChain.Spacing = value; } }

        /// <summary>
        /// Color of the border surrounding the on and off buttons
        /// </summary>
        public Color BorderColor 
        { 
            get { return onBorder.Color; } 
            set 
            {
                onBorder.Color = value;
                offBorder.Color = value;
                bgBorder.Color = value;
            } 
        }

        /// <summary>
        /// Padding between background and button pair
        /// </summary>
        public Vector2 BackgroundPadding { get { return buttonChain.Padding; } set { buttonChain.Padding = value; } }

        /// <summary>
        /// Color used for the background behind the button pair
        /// </summary>
        public Color BackgroundColor { get { return _backgroundColor; } set { background.Color = value; _backgroundColor = value; } }

        /// <summary>
        /// Focus color used for the background behind the button pair
        /// </summary>
        public Color FocusColor { get; set; }

        /// <summary>
        /// Highlight color used for the background behind the button pair
        /// </summary>
        public Color HighlightColor { get; set; }

        /// <summary>
        /// Color used for the background of the unselected button
        /// </summary>
        public Color UnselectedColor { get; set; }

        /// <summary>
        /// Background color used to indicate the current selection
        /// </summary>
        public Color SelectionColor { get; set; }

        /// <summary>
        /// On button text
        /// </summary>
        public RichText OnText { get { return on.Text; } set { on.Text = value; } }

        /// <summary>
        /// Off button text
        /// </summary>
        public RichText OffText { get { return off.Text; } set { off.Text = value; } }

        /// <summary>
        /// Default glyph format used by the on and off buttons
        /// </summary>
        public GlyphFormat Format { get { return on.Format; } set { on.Format = value; off.Format = value; } }

        /// <summary>
        /// Current value of the on/off button
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// If true, then the button will change formatting when it takes focus.
        /// </summary>
        public bool UseFocusFormatting { get; set; }

        /// <summary>
        /// Determines whether or not the button will highlight when moused over.
        /// </summary>
        public virtual bool HighlightEnabled { get; set; }

        /// <summary>
        /// Mouse input element for the button
        /// </summary>
        public IMouseInput MouseInput => mouseInput;

        protected readonly LabelBox on, off;
        protected readonly BorderBox onBorder, offBorder;
        protected readonly HudChain buttonChain;

        protected readonly TexturedBox background;
        protected readonly BorderBox bgBorder;

        protected readonly MouseInputElement mouseInput;
        protected Color _backgroundColor;

        public OnOffButton(HudParentBase parent) : base(parent)
        {
            mouseInput = new MouseInputElement(this);

            background = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            bgBorder = new BorderBox(background)
            {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            on = new LabelBox()
            {
                AutoResize = false,
                Size = new Vector2(71f, 49f),
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center),
                Text = "On"
            };

            onBorder = new BorderBox(on)
            {
                Thickness = 2f,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            off = new LabelBox()
            {
                AutoResize = false,
                Size = new Vector2(71f, 49f),
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center),
                Text = "Off"
            };

            offBorder = new BorderBox(off)
            {
                Thickness = 2f,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            buttonChain = new HudChain(false, bgBorder)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Padding = new Vector2(12f, 10f),
                Spacing = 9f,
                CollectionContainer = { on, off }
            };

            Size = new Vector2(166f, 58f);

            BackgroundColor = TerminalFormatting.Cinder.SetAlphaPct(0.8f);
            HighlightColor = TerminalFormatting.Atomic;
            FocusColor = TerminalFormatting.Mint;
            BorderColor = TerminalFormatting.LimedSpruce;

            UnselectedColor = TerminalFormatting.OuterSpace;
            SelectionColor = TerminalFormatting.DullMint;

            HighlightEnabled = true;
            UseFocusFormatting = true;

            mouseInput.LeftClicked += LeftClick;
        }

        public OnOffButton() : this(null)
        { }

        protected virtual void LeftClick(object sender, EventArgs args)
        {
            Value = !Value;
        }

        protected override void Layout()
        {
            Vector2 buttonSize = cachedSize - cachedPadding - buttonChain.Padding;
            buttonSize.X = buttonSize.X * .5f - buttonChain.Spacing;
            buttonChain.MemberMaxSize = buttonSize;

            if (Value)
            {
                on.Color = SelectionColor;
                off.Color = UnselectedColor;
            }
            else
            {
                off.Color = SelectionColor;
                on.Color = UnselectedColor;
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (mouseInput.HasFocus && SharedBinds.Space.IsNewPressed)
            {
                mouseInput.OnLeftClick();
            }

            if (HighlightEnabled && mouseInput.IsMousedOver)
            {
                background.Color = HighlightColor;
            }
            else if (UseFocusFormatting && mouseInput.HasFocus)
            {
                background.Color = FocusColor;
            }
            else
            {
                background.Color = BackgroundColor;
            }
        }
    }
}