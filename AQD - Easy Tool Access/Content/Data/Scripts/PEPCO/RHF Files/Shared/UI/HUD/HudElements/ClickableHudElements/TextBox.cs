using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VRageMath;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Clickable text box. Supports text highlighting and has its own text caret. Text only, no background.
    /// </summary>
    public class TextBox : Label, IClickableElement
    {
        /// <summary>
        /// Determines whether or not the textbox will allow the user to edit its contents
        /// </summary>
        public bool EnableEditing { get { return caret.ShowCaret; } set { caret.ShowCaret = value; } }

        /// <summary>
        /// Determines whether the user will be allowed to highlight text
        /// </summary>
        public bool EnableHighlighting { get; set; }

        /// <summary>
        /// Indicates whether or not the textbox will accept input
        /// </summary>
        public bool InputOpen { get; private set; }

        /// <summary>
        /// Used to restrict the range of characters allowed for input.
        /// </summary>
        public Func<char, bool> CharFilterFunc { get; set; }

        /// <summary>
        /// Index of the first character in the selected range.
        /// </summary>
        public Vector2I SelectionStart => selectionBox.Start;

        /// <summary>
        /// Index of the last character in the selected range.
        /// </summary>
        public Vector2I SelectionEnd => selectionBox.End;

        /// <summary>
        /// If true, then text box currently has a range of characters selected.
        /// </summary>
        public bool SelectionEmpty => selectionBox.Empty;

        /// <summary>
        /// If true, the caret will move to the end of the text when it gains focus.
        /// </summary>
        public bool MoveToEndOnGainFocus { get; set; }

        /// <summary>
        /// If true, any text selections will be cleared when focus is lost.
        /// </summary>
        public bool ClearSelectionOnLoseFocus { get; set; }

        public IMouseInput MouseInput { get; }

        private readonly TextInput textInput;
        private readonly TextCaret caret;
        private readonly SelectionBox selectionBox;
        private readonly ToolTip warningToolTip;
        private bool canHighlight, allowInput;
        private Vector2 cursorStart;

        public TextBox(HudParentBase parent) : base(parent)
        {
            MouseInput = new MouseInputElement(this) { ShareCursor = true, ZOffset = 1 };
            textInput = new TextInput(AddChar, RemoveLastChar, TextInputFilter);

            caret = new TextCaret(this) { Visible = false };
            selectionBox = new SelectionBox(caret, this) { Color = new Color(255, 255, 255, 140) };

            warningToolTip = new ToolTip()
            {
                text = "Open Chat to Enable Text Editing",
                bgColor = ToolTip.orangeWarningBG
            };

            caret.CaretMoved += CaretMoved;
            MouseInput.GainedInputFocus += GainFocus;
            MouseInput.LostInputFocus += LoseFocus;

            ShareCursor = true;
            EnableEditing = true;
            EnableHighlighting = true;
            UseCursor = true;

            MoveToEndOnGainFocus = false;
            ClearSelectionOnLoseFocus = true;

            Size = new Vector2(60f, 200f);
        }

        public TextBox() : this(null)
        { }

        /// <summary>
        /// Opens the textbox for input and moves the caret to the end.
        /// </summary>
        public void OpenInput()
        {
            allowInput = true;
            UpdateInputOpen();
            caret.SetPosition(0);
            caret.SetPosition(short.MaxValue);
        }
        
        /// <summary>
        /// Closes textbox input and clears the text selection.
        /// </summary>
        public void CloseInput()
        {
            allowInput = false;
            UpdateInputOpen();
            selectionBox.ClearSelection();
        }

        /// <summary>
        /// Highlights the range of text specified.
        /// </summary>
        public void SetSelection(Vector2I start, Vector2I end) =>
            selectionBox.SetSelection(start, end);

        /// <summary>
        /// Clears selected text range.
        /// </summary>
        public void ClearSelection() =>
            selectionBox.ClearSelection();

        /// <summary>
        /// Determines whether or not the given character is within the accepted range for input.
        /// </summary>
        private bool TextInputFilter(char ch)
        {
            if (CharFilterFunc == null)
                return ch >= ' ' || ch == '\n' || ch == '\t';
            else
                return CharFilterFunc(ch) && (ch >= ' ' || ch == '\n');
        }

        private void CaretMoved()
        {
            if (canHighlight)
                selectionBox.UpdateSelection();
        }

        private void GainFocus(object sender, EventArgs args)
        {
            if (MoveToEndOnGainFocus)
                caret.SetPosition(short.MaxValue);
        }

        private void LoseFocus(object sender, EventArgs args)
        {
            if (ClearSelectionOnLoseFocus)
                ClearSelection();
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            bool useInput = allowInput || (MouseInput.HasFocus && HudMain.InputMode == HudInputMode.Full);

            if (EnableEditing && IsMousedOver && HudMain.InputMode == HudInputMode.CursorOnly)
                HudMain.Cursor.RegisterToolTip(warningToolTip);

            if (useInput && EnableEditing)
            {
                textInput.HandleInput();

                if (SharedBinds.Cut.IsNewPressed && !selectionBox.Empty && EnableHighlighting)
                {
                    RichText text = TextBoard.GetTextRange(selectionBox.Start, selectionBox.End);
                    DeleteSelection();
                    caret.Move(new Vector2I(0, -GetRichTextMinLength(text)));
                    HudMain.ClipBoard = text;
                }

                if (SharedBinds.Paste.IsNewPressed)
                {
                    if (!HudMain.ClipBoard.Equals(default(RichText)))
                    {
                        Vector2I insertIndex = caret.CaretIndex + new Vector2I(0, 1);
                        insertIndex.X = MathHelper.Clamp(insertIndex.X, 0, TextBoard.Count);

                        DeleteSelection();
                        TextBoard.Insert(HudMain.ClipBoard, insertIndex);
                        int length = GetRichTextMinLength(HudMain.ClipBoard);

                        if (caret.CaretIndex.Y == -1)
                            length++;

                        caret.Move(new Vector2I(0, length));
                    }
                }
            }

            UpdateInputOpen();
            caret.Visible = InputOpen;

            if (useInput && EnableHighlighting)
            {
                bool isCursorHighlighting = false;

                if (UseCursor)
                {
                    if (MouseInput.IsNewLeftClicked)
                    {
                        cursorStart = cursorPos;
                        selectionBox.ClearSelection();
                    }
                    // Require some movement before enabling highlighting
                    else if (!canHighlight && MouseInput.IsLeftClicked && (cursorPos - cursorStart).LengthSquared() > 16f)
                    {
                        canHighlight = true;
                        isCursorHighlighting = true;
                    }
                    else if (!MouseInput.IsLeftClicked)
                    {
                        canHighlight = false;
                    }
                }

                if (!isCursorHighlighting)
                    canHighlight = SharedBinds.Shift.IsPressed;

                if (SharedBinds.SelectAll.IsNewPressed)
                    selectionBox.SetSelection(Vector2I.Zero, new Vector2I(TextBoard.Count - 1, TextBoard[TextBoard.Count - 1].Count - 1));
                else if (SharedBinds.Escape.IsNewPressed)
                    selectionBox.ClearSelection();

                if (SharedBinds.Copy.IsNewPressed && !selectionBox.Empty)
                    HudMain.ClipBoard = TextBoard.GetTextRange(selectionBox.Start, selectionBox.End);
            }
            else
            {
                canHighlight = false;
            }
        }

        private void UpdateInputOpen()
        {
            bool useInput = allowInput || (MouseInput.HasFocus && HudMain.InputMode == HudInputMode.Full);
            InputOpen = useInput && (EnableHighlighting || EnableEditing);
        }

        /// <summary>
        /// Inserts the given character to the right of the caret.
        /// </summary>
        private void AddChar(char ch)
        {
            DeleteSelection();
            TextBoard.Insert(ch, caret.CaretIndex + new Vector2I(0, 1));
            caret.Move(new Vector2I(0, 1));
        }

        /// <summary>
        /// Removes the character immediately preceeding the caret.
        /// </summary>
        private void RemoveLastChar()
        {
            if (TextBoard.Count > 0 && TextBoard[caret.CaretIndex.X].Count > 0)
            {
                DeleteSelection();

                if (caret.CaretIndex.Y >= 0)
                    TextBoard.RemoveAt(ClampIndex(caret.CaretIndex));

                caret.Move(new Vector2I(0, -1));
            }
        }

        /// <summary>
        /// Removes the text currently highlighted from the textbox.
        /// </summary>
        private void DeleteSelection()
        {
            if (!selectionBox.Empty)
            {
                TextBoard.RemoveRange(selectionBox.Start, selectionBox.End);
                selectionBox.ClearSelection();
            }
        }

        /// <summary>
        /// Clamps the given index within the range of existing characters.
        /// </summary>
        private Vector2I ClampIndex(Vector2I index)
        {
            if (TextBoard.Count > 0)
            {
                index.X = MathHelper.Clamp(index.X, 0, TextBoard.Count - 1);
                index.Y = MathHelper.Clamp(index.Y, 0, TextBoard[index.X].Count - 1);

                return index;
            }
            else
                return Vector2I.Zero;
        }

        private static int GetRichTextMinLength(RichText text)
        {
            int length = 0;

            for (int n = 0; n < text.apiData.Count; n++)
                length += text.apiData[n].Item1.Length;

            return length;
        }

        private class TextCaret : TexturedBox
        {
            /// <summary>
            /// Index of the character currently selected by the caret. When Y == -1, that means
            /// the caret is positioned to the left of the first character in the line.
            /// </summary>
            public Vector2I CaretIndex { get; private set; }

            /// <summary>
            /// Determines whether the caret will be visible
            /// </summary>
            public bool ShowCaret { get; set; }

            public event Action CaretMoved;

            private readonly TextBox textElement;
            private readonly ITextBoard text;
            private readonly Stopwatch blinkTimer;
            private bool blink, caretMoved;
            private int caretOffset;
            private Vector2 lastCursorPos;

            public TextCaret(TextBox textElement) : base(textElement)
            {
                this.textElement = textElement;
                text = textElement.TextBoard;
                Size = new Vector2(1f, 16f);
                Color = new Color(240, 240, 230);

                blinkTimer = new Stopwatch();
                blinkTimer.Start();
            }

            /// <summary>
            /// Moves the caret in the direction indicated by the vector. The caret will automatically
            /// wrap to the last/next line if movement in the Y direction would result in the index
            /// going out of range.
            /// </summary>
            /// <param name="dir">Index direction vector</param>
            public void Move(Vector2I dir, bool navigate = false)
            {
                Vector2I newIndex, min = new Vector2I(0, -1);

                if (dir.Y < 0 && CaretIndex == min)
                    dir.Y = 0;

                bool moveLeft = dir.Y < 0, moveRight = dir.Y > 0,
                    prepending = CaretIndex.Y == -1,
                    startPrepend = moveLeft && CaretIndex.Y == 0;

                if (startPrepend || (dir.Y == 0 && prepending))
                {
                    newIndex = CaretIndex + new Vector2I(dir.X, 0);
                    newIndex.Y = -1;

                    newIndex = ClampIndex(newIndex);
                    caretOffset = GetOffsetFromIndex(new Vector2I(newIndex.X, 0));
                }
                else
                {
                    int newOffset = Math.Max(caretOffset + dir.Y, 0);

                    // Stop prepending
                    if ((prepending && moveRight) && (CaretIndex.X > 0 || text[0].Count > 1))
                        newOffset -= 1;

                    newIndex = GetIndexFromOffset(newOffset) + new Vector2I(dir.X, 0);
                    newIndex = ClampIndex(newIndex);
                    caretOffset = GetOffsetFromIndex(newIndex);

                    if (navigate && moveRight && newIndex.X > CaretIndex.X)
                        newIndex.Y = -1;
                }

                CaretIndex = ClampIndex(newIndex);
                caretMoved = true;

                if (CaretIndex.Y >= 0)
                    text.MoveToChar(CaretIndex);
                else
                    text.MoveToChar(CaretIndex + new Vector2I(0, 1));

                blink = true;
                blinkTimer.Restart();
            }

            public void SetPosition(Vector2I index)
            {
                index = ClampIndex(index);
                
                if (CaretIndex != index)
                    caretMoved = true;

                CaretIndex = index;
                caretOffset = Math.Max(GetOffsetFromIndex(CaretIndex), 0);
                text.MoveToChar(CaretIndex);
            }

            public void SetPosition(short offset)
            {
                Vector2I index = GetIndexFromOffset(offset);

                if (CaretIndex != index)
                    caretMoved = true;

                CaretIndex = index;
                caretOffset = Math.Max(GetOffsetFromIndex(CaretIndex), 0);
                text.MoveToChar(index);
            }

            protected override void Layout()
            {
                base.Layout();

                if (caretMoved)
                {
                    CaretMoved?.Invoke();
                    caretMoved = false;
                }
            }

            protected override void Draw()
            {
                if (ShowCaret)
                {
                    bool isCharVisible = text.Count == 0 || text[0].Count == 0;
                    CaretIndex = ClampIndex(CaretIndex);

                    // If line visible
                    if ((text.Count > 0 && text[0].Count > 0) && 
                        (CaretIndex.X >= text.VisibleLineRange.X && CaretIndex.X <= text.VisibleLineRange.Y) )
                    {
                        // Damned special cases
                        Vector2I index = Vector2I.Max(CaretIndex, Vector2I.Zero);

                        // Calculate visibilty on line
                        IRichChar ch = text[index];
                        Vector2 size = ch.Size,
                            pos = ch.Offset + text.TextOffset;
                        BoundingBox2 textBounds = BoundingBox2.CreateFromHalfExtent(Vector2.Zero, .5f * text.Size),
                            charBounds = BoundingBox2.CreateFromHalfExtent(pos, .5f * Vector2.Max(size, new Vector2(8f)));

                        if (textBounds.Contains(charBounds) != ContainmentType.Disjoint)
                            isCharVisible = true;
                    }

                    if (blink & isCharVisible)
                    {
                        UpdateOffset();
                        base.Draw();
                    }

                    if (blinkTimer.ElapsedMilliseconds > 500)
                    {
                        blink = !blink;
                        blinkTimer.Restart();
                    }
                }
            }

            /// <summary>
            /// Updates the position of the caret to match that of the currently selected character.
            /// </summary>
            private void UpdateOffset()
            {
                Vector2 offset = new Vector2();
                Vector2I index = Vector2I.Max(CaretIndex, Vector2I.Zero);

                if (text.Count > 0 && text[index.X].Count > 0)
                {
                    IRichChar ch;
                    Height = text[index.X].Size.Y - 2f;
                    ch = text[index];

                    if (CaretIndex.Y == -1)
                    {
                       
                        offset = ch.Offset + text.TextOffset;
                        offset.X -= ch.Size.X * .5f + 1f;
                    }
                    else
                    {
                        offset = ch.Offset + text.TextOffset;
                        offset.X += ch.Size.X * .5f + 1f;
                    }
                }
                else
                {
                    if (text.Format.Alignment == TextAlignment.Left)
                        offset.X = -textElement.Size.X * .5f + 2f;
                    else if (text.Format.Alignment == TextAlignment.Right)
                        offset.X = textElement.Size.X * .5f - 2f;

                    offset += _parentFull.Padding * .5f;

                    if (!text.VertCenterText)
                        offset.Y = (text.Size.Y - Height) * .5f - 4f;
                }

                Offset = offset;
            }

            /// <summary>
            /// Handles input for moving the caret.
            /// </summary>
            protected override void HandleInput(Vector2 cursorPos)
            {
                if (SharedBinds.DownArrow.IsPressedAndHeld || SharedBinds.DownArrow.IsNewPressed)
                    Move(new Vector2I(1, 0), true);

                if (SharedBinds.UpArrow.IsPressedAndHeld || SharedBinds.UpArrow.IsNewPressed)
                    Move(new Vector2I(-1, 0), true);

                if (SharedBinds.RightArrow.IsPressedAndHeld || SharedBinds.RightArrow.IsNewPressed)
                    Move(new Vector2I(0, 1), true);

                if (SharedBinds.LeftArrow.IsPressedAndHeld || SharedBinds.LeftArrow.IsNewPressed)
                    Move(new Vector2I(0, -1), true);

                if (textElement.UseCursor)
                {
                    if (textElement.MouseInput.IsLeftClicked)
                        GetClickedChar(cursorPos);
                }
            }

            /// <summary>
            /// Sets the index of the caret to that of the character closest to the cursor.
            /// </summary>
            private void GetClickedChar(Vector2 cursorPos)
            {
                if ((cursorPos - lastCursorPos).LengthSquared() > 4f)
                {
                    CaretIndex = ClampIndex(CaretIndex);

                    Vector2 offset = cursorPos - textElement.Position;
                    Vector2I index = Vector2I.Max(CaretIndex, Vector2I.Zero),
                        newIndex = text.GetCharAtOffset(offset);

                    // If clicking left of center on the char, move one char back.
                    if ((text.Count > 0 && text[index.X].Count > 0 && text[index].Ch != '\n') && (offset.X < text[index].Offset.X))
                        CaretIndex -= new Vector2I(0, 1);

                    CaretIndex = ClampIndex(newIndex);
                    caretOffset = GetOffsetFromIndex(CaretIndex);
                    lastCursorPos = cursorPos;

                    blink = true;
                    blinkTimer.Restart();
                    caretMoved = true;
                }
            }

            /// <summary>
            /// Clamps the given index within the range of existing characters.
            /// </summary>
            private Vector2I ClampIndex(Vector2I index)
            {
                if (text.Count > 0)
                {
                    index.X = MathHelper.Clamp(index.X, 0, text.Count - 1);
                    index.Y = MathHelper.Clamp(index.Y, -1, text[index.X].Count - 1);

                    return index;
                }
                else
                    return Vector2I.Zero;
            }

            /// <summary>
            /// Returns the total number of characters between the start of the text and the current index.
            /// </summary>
            private int GetOffsetFromIndex(Vector2I index)
            {
                int offset = 0;

                for (int line = 0; line < index.X; line++)
                {
                    offset += text[line].Count;
                }

                offset += index.Y;
                return offset;
            }

            /// <summary>
            /// Calculates the index with given the number of characters between it and the beginning of the
            /// text.
            /// </summary>
            private Vector2I GetIndexFromOffset(int offset)
            {
                Vector2I index = Vector2I.Zero;
                int charCount = 0;

                for (int line = 0; line < text.Count; line++)
                    charCount += text[line].Count;

                offset = Math.Min(offset, charCount - 1);

                for (int line = 0; line < text.Count; line++)
                {
                    int lineLength = text[line].Count;

                    if (offset < lineLength)
                    {
                        index.Y = offset;
                        break;
                    }
                    else
                    {
                        offset -= lineLength;
                        index.X++;
                    }
                }

                return index;
            }
        }

        private class SelectionBox : HudElementBase
        {
            /// <summary>
            /// Color of the selection box
            /// </summary>
            public Color Color { get { return highlightBoard.Color; } set { highlightBoard.Color = value; } }

            /// <summary>
            /// Index of the first character in the selection.
            /// </summary>
            public Vector2I Start { get; private set; }

            /// <summary>
            /// Index of the last character in the selection.
            /// </summary>
            public Vector2I End { get; private set; }

            /// <summary>
            /// If true, then the current selection is empty.
            /// </summary>
            public bool Empty => (Start == -Vector2I.One || End == -Vector2I.One);

            private readonly TextCaret caret;
            private readonly ITextBoard text;
            private readonly MatBoard highlightBoard;
            private readonly List<HighlightBox> highlightList;
            private Vector2 lastTextSize;
            private Vector2I lastVisRange;
            private bool highlightStale;

            public SelectionBox(TextCaret caret, Label parent) : base(parent)
            {
                text = parent.TextBoard;
                this.caret = caret;

                Start = -Vector2I.One;
                highlightBoard = new MatBoard();
                highlightList = new List<HighlightBox>();
            }

            public void SetSelection(Vector2I start, Vector2I end)
            {
                Start = start;
                End = end;

                highlightStale = true;
            }

            public void ClearSelection()
            {
                Start = -Vector2I.One;
                End = -Vector2I.One;
                highlightList.Clear();
            }

            public void UpdateSelection()
            {
                Vector2I caretIndex = caret.CaretIndex;

                if (text.Count > 0)
                {
                    if (Start == -Vector2I.One)
                    {
                        Start = caretIndex;
                        End = Start;

                        if (Start.Y < text[Start.X].Count - 1)
                            Start += new Vector2I(0, 1);
                    }
                    else
                    {
                        // If caret after start
                        if (caretIndex.X > Start.X || (caretIndex.X == Start.X && caretIndex.Y >= Start.Y))
                            End = caretIndex;
                        else
                        {
                            Start = caretIndex;

                            if (Start.Y < text[Start.X].Count - 1)
                                Start += new Vector2I(0, 1);
                        }
                    }

                    if (End.Y == -1)
                        End += new Vector2I(0, 1);

                    highlightStale = true;
                }
                else
                {
                    Start = -Vector2I.One;
                    End = -Vector2I.One;
                }
            }

            protected override void Draw()
            {
                if (lastTextSize != text.Size)
                {
                    lastTextSize = text.Size;
                    ClearSelection();
                }

                if (!Empty)
                {
                    if (highlightStale || text.VisibleLineRange != lastVisRange)
                    {
                        lastVisRange = text.VisibleLineRange;
                        UpdateHighlight();
                    }

                    Vector2 tbOffset = text.TextOffset, bounds = new Vector2(-text.Size.X * .5f, text.Size.X * .5f);

                    for (int n = 0; n < highlightList.Count; n++)
                        highlightList[n].Draw(highlightBoard, Origin, tbOffset, bounds, HudSpace.PlaneToWorldRef);
                }
            }

            /// <summary>
            /// Calculates the size and offsets for the boxes highlighting the selection.
            /// </summary>
            private void UpdateHighlight()
            {
                highlightStale = false;
                highlightList.Clear();

                // Clamp line range
                Vector2I lineRange = text.VisibleLineRange;
                Start = new Vector2I(MathHelper.Clamp(Start.X, 0, text.Count - 1), Start.Y);
                End = new Vector2I(MathHelper.Clamp(End.X, 0, text.Count - 1), End.Y);

                // Clamp char range
                if (text.Count > 0)
                {
                    Start = new Vector2I(Start.X, MathHelper.Clamp(Start.Y, 0, text[Start.X].Count - 1));
                    End = new Vector2I(End.X, MathHelper.Clamp(End.Y, 0, text[End.X].Count - 1));
                }

                int startLine = Math.Max(Start.X, lineRange.X),
                    endLine = Math.Min(End.X, lineRange.Y);

                // Add start and end
                if (Start.X == End.X && Start.X == startLine)
                    AddHighlightBox(Start.X, Start.Y, End.Y);
                else
                {
                    for (int line = startLine; line <= endLine; line++)
                    {
                        if (line == Start.X)
                            AddHighlightBox(Start.X, Start.Y, text[Start.X].Count - 1); // Top
                        else if (line == End.X)
                            AddHighlightBox(End.X, 0, End.Y); // Bottom
                        else
                            AddHighlightBox(line, 0, text[line].Count - 1); // Middle
                    }
                }

                if (highlightList.Count > 0 && highlightList.Capacity > 3 * highlightList.Count)
                    highlightList.TrimExcess();
            }

            /// <summary>
            /// Adds an appropriately sized highlight box for the range of characters on the given line.
            /// Does not take into account text clipping or text offset.
            /// </summary>
            private void AddHighlightBox(int line, int startCh, int endCh)
            {
                if (text[line].Count > 0)
                {
                    if (startCh < 0 || endCh < 0 || startCh >= text[line].Count || endCh >= text[line].Count)
                        throw new Exception($"Char out of range. Line: {line} StartCh: {startCh}, EndCh: {endCh}, Count: {text[line].Count}");

                    IRichChar left = text[line][startCh], right = text[line][endCh];
                    var highlightBox = new HighlightBox
                    {
                        size = new Vector2()
                        {
                            X = right.Offset.X - left.Offset.X + (left.Size.X + right.Size.X) * .5f,
                            Y = text[line].Size.Y
                        },
                        offset = new Vector2()
                        {
                            X = (right.Offset.X + left.Offset.X) * .5f - 2f,
                            Y = text[line].VerticalOffset - text[line].Size.Y * .5f
                        }
                    };

                    if (highlightBox.size.X > 1f)
                        highlightBox.size.X += 4f;

                    highlightList.Add(highlightBox);
                }
            }

            private struct HighlightBox
            {
                public Vector2 size, offset;

                public void Draw(MatBoard matBoard, Vector2 origin, Vector2 tbOffset, Vector2 xBounds, MatrixD[] matrixRef)
                {
                    CroppedBox box = default(CroppedBox);
                    Vector2 clipSize, clipPos;
                    clipSize = size;
                    clipPos = offset + tbOffset;

                    // Determine the visible extents of the highlight box within the bounds of the textboard
                    float leftBound = Math.Max(clipPos.X - clipSize.X * .5f, xBounds.X),
                        rightBound = Math.Min(clipPos.X + clipSize.X * .5f, xBounds.Y);

                    // Adjust highlight size and offset to compensate for textboard clipping and offset
                    clipSize.X = Math.Max(0, rightBound - leftBound);
                    clipPos.X = (rightBound + leftBound) * .5f;
                    clipPos += origin;

                    clipSize *= .5f;
                    box.bounds = new BoundingBox2(clipPos - clipSize, clipPos + clipSize);

                    matBoard.Draw(ref box, matrixRef);
                }
            }
        }
    }
}
