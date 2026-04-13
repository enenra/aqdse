using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            public enum LineAccessors : int
            {
                /// <summary>
                /// out: int
                /// </summary>
                Count = 1,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Size = 2,

                /// <summary>
                /// out: float
                /// </summary>
                VerticalOffset = 3,
            }

            public interface ILine : IIndexedCollection<IRichChar>
            {
                /// <summary>
                /// Size of the line as rendered
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// Starting vertical position of the line starting from the center of the text element, sans text offset.
                /// </summary>
                float VerticalOffset { get; }
            }
        }
    }
}