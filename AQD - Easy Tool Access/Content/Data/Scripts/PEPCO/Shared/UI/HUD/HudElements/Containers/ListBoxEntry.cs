using System;
using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    /// <summary>
    /// Label assocated with an object of type T. Used in conjunction with list boxes.
    /// </summary>
    public class ListBoxEntry<TValue> : ListBoxEntry<Label, TValue>
    { }

    public class ListBoxEntry<TElement, TValue>
        : SelectionBoxEntryTuple<TElement, TValue>, IListBoxEntry<TElement, TValue>
        where TElement : HudElementBase, IMinLabelElement, new()
    {
        public ListBoxEntry()
        {
            SetElement(new TElement());
            Element.TextBoard.AutoResize = false;
        }

        public override void Reset()
        {
            Enabled = true;
            AllowHighlighting = true;
            AssocMember = default(TValue);
            Element.TextBoard.Clear();
        }

        public object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxEntryAccessors)memberEnum;

            switch (member)
            {
                case ListBoxEntryAccessors.Name:
                    {
                        if (data != null)
                            Element.TextBoard.SetText(data as List<RichStringMembers>);
                        else
                            return Element.TextBoard.GetText().apiData;

                        break;
                    }
                case ListBoxEntryAccessors.Enabled:
                    {
                        if (data != null)
                            Enabled = (bool)data;
                        else
                            return Enabled;

                        break;
                    }
                case ListBoxEntryAccessors.AssocObject:
                    {
                        if (data != null)
                            AssocMember = (TValue)data;
                        else
                            return AssocMember;

                        break;
                    }
                case ListBoxEntryAccessors.ID:
                        return this;
            }

            return null;
        }
    }
}