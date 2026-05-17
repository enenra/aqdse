using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI.Client
    {
        using TextBuilderMembers = MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<IList<RichStringMembers>>, // SetText
            Action // Clear
        >;

        /// <summary>
        /// Scrollable text page used in the terminal.
        /// </summary>
        public class TextPage : TerminalPageBase, ITextPage
        {
            /// <summary>
            /// Gets/sets header text
            /// </summary>
            public RichText HeaderText
            {
                get { return new RichText(GetOrSetMemberFunc(null, (int)TextPageAccessors.GetOrSetHeader) as List<RichStringMembers>); }
                set { GetOrSetMemberFunc(value.apiData, (int)TextPageAccessors.GetOrSetHeader); }
            }

            /// <summary>
            /// Gets/sets subheader text
            /// </summary>
            public RichText SubHeaderText
            {
                get { return new RichText(GetOrSetMemberFunc(null, (int)TextPageAccessors.GetOrSetSubheader) as List<RichStringMembers>); }
                set { GetOrSetMemberFunc(value.apiData, (int)TextPageAccessors.GetOrSetSubheader); }
            }

            /// <summary>
            /// Contents of the text box.
            /// </summary>
            public RichText Text
            {
                get { return new RichText(GetOrSetMemberFunc(null, (int)TextPageAccessors.GetOrSetText) as List<RichStringMembers>); }
                set { GetOrSetMemberFunc(value.apiData, (int)TextPageAccessors.GetOrSetText); }
            }

            /// <summary>
            /// Text builder used to control the contents of the page
            /// </summary>
            public ITextBuilder TextBuilder { get; }

            public TextPage() : base(ModPages.TextPage)
            {
                TextBuilder = new BasicTextBuilder((TextBuilderMembers)GetOrSetMemberFunc(null, (int)TextPageAccessors.GetTextBuilder));
            }

            private class BasicTextBuilder : TextBuilder
            {
                public BasicTextBuilder(TextBuilderMembers members) : base(members)
                { }
            }
        }
    }
}