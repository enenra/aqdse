using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;
    using Rendering.Client;
    using Rendering.Server;

    /// <summary>
    /// HUD element used to render text.
    /// </summary>
    public class Label : LabelElementBase
    {
        /// <summary>
        /// Text rendered by the label.
        /// </summary>
        public RichText Text { get { return _textBoard.GetText(); } set { _textBoard.SetText(value); } }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public override ITextBoard TextBoard => _textBoard;

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return _textBoard.Format; } set { _textBoard.Format = value; } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return _textBoard.BuilderMode; } set { _textBoard.BuilderMode = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public bool AutoResize { get { return _textBoard.AutoResize; } set { _textBoard.AutoResize = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return _textBoard.VertCenterText; } set { _textBoard.VertCenterText = value; } }

        public override float Width
        {
            get { return _textBoard.Size.X + Padding.X; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                _textBoard.FixedSize = new Vector2(value, _textBoard.FixedSize.Y);
            }
        }

        public override float Height
        {
            get { return _textBoard.Size.Y + Padding.Y; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                _textBoard.FixedSize = new Vector2(_textBoard.FixedSize.X, value);
            }
        }

        protected readonly TextBoard _textBoard;

        public Label(HudParentBase parent) : base(parent)
        {
            _textBoard = new TextBoard();
            _textBoard.Format = GlyphFormat.White;
            _textBoard.SetText("NewLabel");
        }

        public Label() : this(null)
        { }

        protected override void Draw()
        {
            Vector2 halfSize = (cachedSize - cachedPadding) * .5f;
            BoundingBox2 box = new BoundingBox2(cachedPosition - halfSize, cachedPosition + halfSize);

            if (maskingBox != null)
                _textBoard.Draw(box, maskingBox.Value, HudSpace.PlaneToWorldRef);
            else
                _textBoard.Draw(box, CroppedBox.defaultMask, HudSpace.PlaneToWorldRef);
        }
    }
}
