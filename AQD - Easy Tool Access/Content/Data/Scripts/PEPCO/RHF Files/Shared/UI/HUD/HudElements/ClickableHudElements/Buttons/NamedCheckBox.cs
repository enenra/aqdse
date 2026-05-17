using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Named checkbox designed to mimic the appearance of checkboxes used in the SE terminal.
    /// </summary>
    public class NamedCheckBox : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Text rendered by the label.
        /// </summary>
        public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return name.TextBoard.Format; } set { name.TextBoard.Format = value; } }

        /// <summary>
        /// Size of the text element sans padding.
        /// </summary>
        public Vector2 TextSize { get { return name.Size; } set { name.Size = value; } }

        /// <summary>
        /// Padding applied to the text element.
        /// </summary>
        public Vector2 TextPadding { get { return name.Padding; } set { name.Padding = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public bool AutoResize { get { return name.AutoResize; } set { name.AutoResize = value; } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return name.BuilderMode; } set { name.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return name.VertCenterText; } set { name.VertCenterText = value; } }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public ITextBuilder NameBuilder => name.TextBoard;

        /// <summary>
        /// Checkbox mouse input
        /// </summary>
        public IMouseInput MouseInput => checkbox.MouseInput;

        /// <summary>
        /// Indicates whether or not the box is checked.
        /// </summary>
        public bool IsBoxChecked { get { return checkbox.IsBoxChecked; } set { checkbox.IsBoxChecked = value; } }

        private readonly Label name;
        private readonly BorderedCheckBox checkbox;
        private readonly HudChain layout;

        public NamedCheckBox(HudParentBase parent) : base(parent)
        {
            name = new Label()
            {
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Right),
                Text = "NewCheckbox",
                AutoResize = false
            };

            checkbox = new BorderedCheckBox();

            layout = new HudChain(false, this)
            {
                Spacing = 17f,
                CollectionContainer = { name, checkbox }
            };

            AutoResize = true;
            Size = new Vector2(250f, 37f);
        }

        protected override void Layout()
        {
            Vector2 size = cachedSize - cachedPadding;
            checkbox.Size = new Vector2(size.Y);
            name.Width = size.X - checkbox.Width - layout.Spacing;
        }

        public NamedCheckBox() : this(null)
        { }
    }
}