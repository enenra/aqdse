using System;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Unlined clickable textbox with a background and border designed to look like text fields in the SE
    /// terminal.
    /// </summary>
    public class TextField : LabelBoxBase, IClickableElement, ILabelElement
    {
        /// <summary>
        /// Invoked whenever a change is made to the text. Invokes once every 500ms, at most.
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// Text rendered by the text field.
        /// </summary>
        public RichText Text { get { return textBox.TextBoard.GetText(); } set { textBox.TextBoard.SetText(value); } }

        /// <summary>
        /// TextBoard backing the text field.
        /// </summary>
        public ITextBoard TextBoard => textBox.TextBoard;

        /// <summary>
        /// Default formatting used by the text field.
        /// </summary>
        public GlyphFormat Format { get { return textBox.Format; } set { textBox.Format = value; } }

        /// <summary>
        /// Text formatting used when the control gains focus.
        /// </summary>
        public Color FocusTextColor { get; set; }

        /// <summary>
        /// Size of the text as rendered
        /// </summary>
        public override Vector2 TextSize { get { return textBox.Size; } set { textBox.Size = value; } }

        /// <summary>
        /// Padding around text element
        /// </summary>
        public override Vector2 TextPadding { get { return textBox.Padding; } set { textBox.Padding = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public override bool AutoResize { get { return textBox.AutoResize; } set { textBox.AutoResize = value; } }

        /// <summary>
        /// Determines whether or not the textbox will allow the user to edit its contents
        /// </summary>
        public bool EnableEditing { get { return textBox.EnableEditing; } set { textBox.EnableEditing = value; } }

        /// <summary>
        /// Determines whether the user will be allowed to highlight text
        /// </summary>
        public bool EnableTextHighlighting { get { return textBox.EnableHighlighting; } set { textBox.EnableHighlighting = value; } }

        /// <summary>
        /// Indicates whether or not the text field will accept input
        /// </summary>
        public bool InputOpen => textBox.InputOpen;

        /// <summary>
        /// Used to restrict the range of characters allowed for input.
        /// </summary>
        public Func<char, bool> CharFilterFunc { get { return textBox.CharFilterFunc; } set { textBox.CharFilterFunc = value; } }

        /// <summary>
        /// Index of the first character in the selected range.
        /// </summary>
        public Vector2I SelectionStart => textBox.SelectionStart;

        /// <summary>
        /// Index of the last character in the selected range.
        /// </summary>
        public Vector2I SelectionEnd => textBox.SelectionEnd;

        /// <summary>
        /// If true, then text box currently has a range of characters selected.
        /// </summary>
        public bool SelectionEmpty => textBox.SelectionEmpty;

        /// <summary>
        /// Background color when the text field is moused over
        /// </summary>
        public Color HighlightColor { get; set; }

        /// <summary>
        /// Background color when the text field has input focus
        /// </summary>
        public Color FocusColor { get; set; }

        /// <summary>
        /// Color of the thin border surrounding the text field
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// Thickness of the border around the text field
        /// </summary>
        public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

        /// <summary>
        /// If true then the text field will change color when moused over
        /// </summary>
        public bool HighlightEnabled { get; set; }

        /// <summary>
        /// If true, then the text field will change formatting when it takes focus.
        /// </summary>
        public bool UseFocusFormatting { get; set; }

        public IMouseInput MouseInput => textBox.MouseInput;

        public override bool IsMousedOver => textBox.IsMousedOver;

        /// <summary>
        /// Line formatting mode used by the field
        /// </summary>
        public TextBuilderModes BuilderMode { get { return textBox.BuilderMode; } set { textBox.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return textBox.VertCenterText; } set { textBox.VertCenterText = value; } }

        protected readonly TextBox textBox;
        protected readonly BorderBox border;
        protected Color lastColor, lastTextColor;

        public TextField(HudParentBase parent) : base(parent)
        {
            border = new BorderBox(background)
            {
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            textBox = new TextBox(background)
            {
                AutoResize = false,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Padding = new Vector2(24f, 0f),
                MoveToEndOnGainFocus = true,
                ClearSelectionOnLoseFocus = true
            };

            Format = TerminalFormatting.ControlFormat;
            FocusTextColor = TerminalFormatting.Charcoal;
            Text = "NewTextField";

            Color = TerminalFormatting.OuterSpace;
            HighlightColor = TerminalFormatting.Atomic;
            FocusColor = TerminalFormatting.Mint;
            BorderColor = TerminalFormatting.LimedSpruce;

            UseFocusFormatting = true;
            HighlightEnabled = true;

            Size = new Vector2(250f, 40);

            textBox.TextBoard.TextChanged += OnTextChanged;
            MouseInput.CursorEntered += CursorEnter;
            MouseInput.CursorExited += CursorExit;
            MouseInput.GainedInputFocus += GainFocus;
            MouseInput.LostInputFocus += LoseFocus;
        }

        public TextField() : this(null)
        { }

        public void OpenInput() =>
            textBox.OpenInput();

        public void CloseInput() =>
            textBox.CloseInput();

        private void OnTextChanged()
        {
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (!(UseFocusFormatting && MouseInput.HasFocus))
                {
                    lastColor = Color;
                    lastTextColor = Format.Color;
                }

                TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
                Color = HighlightColor;
            }
        }

        protected virtual void CursorExit(object sender, EventArgs args)
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