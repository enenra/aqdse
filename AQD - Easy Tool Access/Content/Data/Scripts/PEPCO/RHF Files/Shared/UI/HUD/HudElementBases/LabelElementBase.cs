using RichHudFramework.UI.Rendering;
using System;
using VRage;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        public abstract class LabelElementBase : HudElementBase, IMinLabelElement
        {
            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public abstract ITextBoard TextBoard { get; }

            public LabelElementBase(HudParentBase parent = null) : base(parent)
            { }
        }
    }
}
