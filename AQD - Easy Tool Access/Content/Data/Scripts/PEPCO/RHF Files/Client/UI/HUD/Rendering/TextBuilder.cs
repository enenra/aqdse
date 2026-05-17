using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using BoolProp = MyTuple<Func<bool>, Action<bool>>;
    using FloatProp = MyTuple<Func<float>, Action<float>>;
    using RangeData = MyTuple<Vector2I, Vector2I>;
    using RangeFormatData = MyTuple<Vector2I, Vector2I, GlyphFormatMembers>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using Vec2Prop = MyTuple<Func<Vector2>, Action<Vector2>>;

    namespace UI
    {
        using System.Collections;
        using TextBuilderMembers = MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<IList<RichStringMembers>>, // SetText
            Action // Clear
        >;

        namespace Rendering.Client
        {
            public abstract class TextBuilder : ITextBuilder
            {
                /// <summary>
                /// Returns the character at the index specified.
                /// </summary>
                public IRichChar this[Vector2I index] => lines[index.X][index.Y];

                /// <summary>
                /// Returns the line at the index given.
                /// </summary>
                public ILine this[int index] => lines[index];

                /// <summary>
                /// Returns the current number of lines.
                /// </summary>
                public int Count => GetLineCountFunc();

                /// <summary>
                /// Default text format. Applied to strings added without any other formatting specified.
                /// </summary>
                public GlyphFormat Format
                {
                    get { return new GlyphFormat((GlyphFormatMembers)GetOrSetMemberFunc(null, (int)TextBuilderAccessors.Format)); }
                    set { GetOrSetMemberFunc(value.Data, (int)TextBuilderAccessors.Format); }
                }

                /// <summary>
                /// Gets or sets the maximum line width before text will wrap to the next line. Word wrapping must be enabled for
                /// this to apply.
                /// </summary>
                public float LineWrapWidth
                {
                    get { return (float)GetOrSetMemberFunc(null, (int)TextBuilderAccessors.LineWrapWidth); }
                    set { GetOrSetMemberFunc(value, (int)TextBuilderAccessors.LineWrapWidth); }
                }

                /// <summary>
                /// Determines the formatting mode of the text.
                /// </summary>
                public TextBuilderModes BuilderMode
                {
                    get { return (TextBuilderModes)GetOrSetMemberFunc(null, (int)TextBuilderAccessors.BuilderMode); }
                    set { GetOrSetMemberFunc(value, (int)TextBuilderAccessors.BuilderMode); }
                }

                protected readonly Func<object, int, object> GetOrSetMemberFunc;
                private readonly Func<int, int, object> GetLineMemberFunc;
                private readonly Func<int> GetLineCountFunc;
                private readonly Func<Vector2I, int, object> GetCharMemberFunc;
                private readonly Action<IList<RichStringMembers>, Vector2I> InsertTextAction;
                private readonly Action<IList<RichStringMembers>> SetTextAction;
                private readonly Action ClearAction;

                private readonly ReadOnlyApiCollection<ILine> lines;
                private RichText lastText;

                public TextBuilder(TextBuilderMembers data)
                {
                    GetLineMemberFunc = data.Item1.Item1;
                    GetLineCountFunc = data.Item1.Item2;

                    GetCharMemberFunc = data.Item2;
                    GetOrSetMemberFunc = data.Item3;
                    InsertTextAction = data.Item4;
                    SetTextAction = data.Item5;
                    ClearAction = data.Item6;

                    lines = new ReadOnlyApiCollection<ILine>(x => new LineData(this, x), GetLineCountFunc);
                }

                /// <summary>
                /// Replaces the current text with the <see cref="RichText"/> given
                /// </summary>
                public void SetText(RichText text)
                {
                    SetTextAction(text.apiData);
                    lastText = text;
                }

                /// <summary>
                /// Clears current text and appends a copy of the <see cref="StringBuilder"/> given.
                /// </summary>
                public void SetText(StringBuilder text, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(text, format ?? Format);
                    SetTextAction(lastText.apiData);
                }

                /// <summary>
                /// Clears current text and appends a copy of the <see cref="string"/> given.
                /// </summary>
                public void SetText(string text, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(text, format ?? Format);
                    SetTextAction(lastText.apiData);
                }

                /// <summary>
                /// Appends the given <see cref="RichText"/>
                /// </summary>
                public void Append(RichText text)
                {
                    InsertTextAction(text.apiData, GetLastIndex());
                }

                /// <summary>
                /// Appends a copy of the text in the <see cref="StringBuilder"/>
                /// </summary>
                public void Append(StringBuilder text, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(text, format ?? Format);
                    InsertTextAction(lastText.apiData, GetLastIndex());
                }

                /// <summary>
                /// Appends a copy of the <see cref="string"/>
                /// </summary>
                public void Append(string text, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(text, format ?? Format);
                    InsertTextAction(lastText.apiData, GetLastIndex());
                }

                /// <summary>
                /// Appends the given <see cref="char"/>
                /// </summary>
                public void Append(char ch, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(ch, format ?? Format);
                    InsertTextAction(lastText.apiData, GetLastIndex());
                }

                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                public void Insert(RichText text, Vector2I start)
                {
                    InsertTextAction(text.apiData, start);
                }

                /// <summary>
                /// Inserts a copy of the given <see cref="StringBuilder"/> starting at the specified starting index
                /// </summary>
                public void Insert(StringBuilder text, Vector2I start, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(text, format ?? Format);
                    InsertTextAction(lastText.apiData, start);
                }

                /// <summary>
                /// Inserts a copy of the given <see cref="string"/> starting at the specified starting index
                /// </summary>
                public void Insert(string text, Vector2I start, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(text, format ?? Format);
                    InsertTextAction(lastText.apiData, start);
                }

                /// <summary>
                /// Inserts the given <see cref="char"/> starting at the specified starting index
                /// </summary>
                public void Insert(char ch, Vector2I start, GlyphFormat? format = null)
                {
                    if (lastText == null)
                        lastText = new RichText();

                    lastText.Clear();
                    lastText.Add(ch, format ?? Format);
                    InsertTextAction(lastText.apiData, start);
                }

                /// <summary>
                /// Returns the contents of the text as <see cref="RichText"/>.
                /// </summary>
                public RichText GetText() =>
                    GetTextRange(Vector2I.Zero, GetLastIndex() - new Vector2I(0, 1));

                /// <summary>
                /// Returns the specified range of characters from the text as <see cref="RichText"/>.
                /// </summary>
                public RichText GetTextRange(Vector2I start, Vector2I end)
                {
                    var textData = GetOrSetMemberFunc(new RangeData(start, end), (int)TextBuilderAccessors.GetRange) as List<RichStringMembers>;

                    if (lastText == null || lastText.apiData != textData)
                        lastText = new RichText(textData);

                    return lastText;
                }

                /// <summary>
                /// Changes the formatting for the whole text to the given format.
                /// </summary>
                public void SetFormatting(GlyphFormat format)
                {
                    GetOrSetMemberFunc(format.Data, (int)TextBuilderAccessors.Format);
                    GetOrSetMemberFunc(new RangeFormatData(Vector2I.Zero, GetLastIndex() - new Vector2I(0, 1), format.Data), (int)TextBuilderAccessors.SetFormatting);
                }

                /// <summary>
                /// Changes the formatting for the text within the given range to the given format.
                /// </summary>
                public void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format) =>
                    GetOrSetMemberFunc(new RangeFormatData(start, end, format.Data), (int)TextBuilderAccessors.SetFormatting);

                /// <summary>
                /// Removes the character at the specified index.
                /// </summary>
                public void RemoveAt(Vector2I index) =>
                    GetOrSetMemberFunc(new RangeData(index, index), (int)TextBuilderAccessors.RemoveRange);

                /// <summary>
                /// Removes all text within the specified range.
                /// </summary>
                public void RemoveRange(Vector2I start, Vector2I end) =>
                    GetOrSetMemberFunc(new RangeData(start, end), (int)TextBuilderAccessors.RemoveRange);

                /// <summary>
                /// Clears all existing text.
                /// </summary>
                public void Clear() =>
                    ClearAction();

                /// <summary>
                /// Returns the contents of the <see cref="ITextBuilder"/> as an unformatted string.
                /// </summary>
                public override string ToString() =>
                    GetOrSetMemberFunc(null, (int)TextBuilderAccessors.ToString) as string;

                protected Vector2I GetLastIndex()
                {
                    int lineCount = GetLineCountFunc();
                    Vector2I start = new Vector2I(Math.Max(0, lineCount - 1), 0);

                    if (lineCount > 0)
                        start.Y = Math.Max(0, lines[start.X].Count);

                    return start;
                }

                protected class LineData : ILine
                {
                    public IRichChar this[int ch] => characters[ch];
                    public int Count => (int)parent.GetLineMemberFunc(index, (int)LineAccessors.Count);
                    public Vector2 Size => (Vector2)parent.GetLineMemberFunc(index, (int)LineAccessors.Size);
                    public float VerticalOffset => (float)parent.GetLineMemberFunc(index, (int)LineAccessors.VerticalOffset);

                    private readonly TextBuilder parent;
                    private readonly int index;
                    private readonly ReadOnlyApiCollection<IRichChar> characters;

                    public LineData(TextBuilder parent, int index)
                    {
                        this.parent = parent;
                        this.index = index;

                        characters = new ReadOnlyApiCollection<IRichChar>
                        (
                            x => new RichCharData(parent, new Vector2I(index, x)),
                            () => (int)parent.GetLineMemberFunc(index, (int)LineAccessors.Count)
                        );
                    }
                }

                protected class RichCharData : IRichChar
                {
                    public char Ch => (char)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Ch);
                    public GlyphFormat Format => new GlyphFormat((GlyphFormatMembers)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Format));
                    public Vector2 Size => (Vector2)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Size);
                    public Vector2 Offset => (Vector2)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Offset);

                    private readonly TextBuilder parent;
                    private readonly Vector2I index;

                    public RichCharData(TextBuilder parent, Vector2I index)
                    {
                        this.parent = parent;
                        this.index = index;
                    }
                }
            }
        }
    }
}