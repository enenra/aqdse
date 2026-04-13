using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A text element with a textured background.
    /// </summary>
    public class LabelBox : LabelBoxBase, ILabelElement
    {
        /// <summary>
        /// Text rendered by the label.
        /// </summary>
        public RichText Text { get { return textElement.TextBoard.GetText(); } set { textElement.TextBoard.SetText(value); } }

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return textElement.TextBoard.Format; } set { textElement.TextBoard.Format = value; } }

        /// <summary>
        /// Padding applied to the text element.
        /// </summary>
        public override Vector2 TextPadding { get { return textElement.Padding; } set { textElement.Padding = value; } }

        /// <summary>
        /// Size of the text element including TextPadding.
        /// </summary>
        public override Vector2 TextSize { get { return textElement.Size; } set { textElement.Size = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public override bool AutoResize { get { return textElement.AutoResize; } set { textElement.AutoResize = value; } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return TextBoard.BuilderMode; } set { TextBoard.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return textElement.VertCenterText; } set { textElement.VertCenterText = value; } }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public ITextBoard TextBoard => textElement.TextBoard;

        /// <summary>
        /// Text element contained by the label box.
        /// </summary>
        public readonly Label textElement;

        public LabelBox(HudParentBase parent) : base(parent)
        {
            textElement = new Label(this);
        }

        public LabelBox() : this(null)
        { }
    }
}
