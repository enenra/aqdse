using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
    /// <summary>
    /// An On/Off button with a label over it
    /// </summary>
    public class NamedOnOffButton : HudElementBase, IClickableElement
    {
        public override Vector2 Padding { get { return layout.Padding; } set { layout.Padding = value; } }

        /// <summary>
        /// The name of the control as it appears in the terminal.
        /// </summary>
        public RichText Name { get { return name.Text; } set { name.Text = value; } }

        /// <summary>
        /// Distance between the on and off buttons
        /// </summary>
        public float ButtonSpacing { get { return onOffButton.ButtonSpacing; } set { onOffButton.ButtonSpacing = value; } }

        /// <summary>
        /// Padding around on/off button block
        /// </summary>
        public Vector2 ButtonPadding { get { return onOffButton.Padding; } set { onOffButton.Padding = value; } }

        /// <summary>
        /// Color of the border surrounding the on and off buttons
        /// </summary>
        public Color BorderColor { get { return onOffButton.BorderColor; } set { onOffButton.BorderColor = value; } }

        /// <summary>
        /// On button text
        /// </summary>
        public RichText OnText { get { return onOffButton.OnText; } set { onOffButton.OnText = value; } }

        /// <summary>
        /// Off button text
        /// </summary>
        public RichText OffText { get { return onOffButton.OnText; } set { onOffButton.OnText = value; } }

        /// <summary>
        /// Default glyph format used by the on and off buttons
        /// </summary>
        public GlyphFormat Format { get { return onOffButton.Format; } set { onOffButton.Format = value; } }

        /// <summary>
        /// Current value of the on/off button
        /// </summary>
        public bool Value { get { return onOffButton.Value; } set { onOffButton.Value = value; } }

        /// <summary>
        /// Mouse input element for the button
        /// </summary>
        public IMouseInput MouseInput => onOffButton.MouseInput;

        protected readonly Label name;
        protected readonly OnOffButton onOffButton;
        protected readonly HudChain layout;

        public NamedOnOffButton(HudParentBase parent) : base(parent)
        {
            name = new Label()
            {
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center),
                Text = "NewOnOffButton",
                AutoResize = false,
                Height = 22f,
            };

            onOffButton = new OnOffButton() 
            { 
                Padding = new Vector2(78f, 0f),
            };

            layout = new HudChain(true, this)
            {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                Spacing = 2f,
                CollectionContainer = { name, onOffButton }
            };

            Padding = new Vector2(20f, 0f);
            Size = new Vector2(300f, 84f);
        }

        public NamedOnOffButton() : this(null)
        { }

        protected override void Layout()
        {
            onOffButton.Height = Height - name.Height - Padding.Y - layout.Spacing;
        }
    }
}