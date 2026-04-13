using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            public interface IReadOnlyMaterialFrame
            {
                /// <summary>
                /// Texture associated with the frame
                /// </summary>
                MaterialAlignment Alignment { get; }

                /// <summary>
                /// Determines how or if the material is scaled w/respect to its aspect ratio.
                /// </summary>
                Material Material { get;}

                /// <summary>
                /// Texture coordinate offset
                /// </summary>
                Vector2 UvOffset { get; }

                BoundingBox2 GetMaterialAlignment(float bbAspectRatio);
            }

            /// <summary>
            /// Defines the positioning and alignment of a Material on a QuadBoard.
            /// </summary>
            public class MaterialFrame : IReadOnlyMaterialFrame
            {
                /// <summary>
                /// Texture associated with the frame
                /// </summary>
                public Material Material { get; set; }

                /// <summary>
                /// Determines how or if the material is scaled w/respect to its aspect ratio.
                /// </summary>
                public MaterialAlignment Alignment { get; set; }

                /// <summary>
                /// Texture coordinate offset
                /// </summary>
                public Vector2 UvOffset { get; set; }

                public MaterialFrame()
                {
                    Material = Material.Default;
                    Alignment = MaterialAlignment.StretchToFit;
                    UvOffset = Vector2.Zero;
                }

                /// <summary>
                /// Calculates the texture coordinates needed to fit the material to the billboard. 
                /// Aspect ratio = Width/Height
                /// </summary>
                public BoundingBox2 GetMaterialAlignment(float bbAspectRatio)
                {
                    Vector2 matOrigin = Material.uvOffset + UvOffset,
                        matStep = Material.uvSize * .5f;

                    if (Alignment != MaterialAlignment.StretchToFit)
                    {
                        float matAspectRatio = Material.size.X / Material.size.Y;
                        Vector2 localUV = new Vector2(1f);

                        if (Alignment == MaterialAlignment.FitAuto)
                        {
                            if (matAspectRatio > bbAspectRatio) // If material is too wide, make it shorter
                                localUV = new Vector2(1f, matAspectRatio / bbAspectRatio);
                            else // If the material is too tall, make it narrower
                                localUV = new Vector2(bbAspectRatio / matAspectRatio, 1f);
                        }
                        else if (Alignment == MaterialAlignment.FitVertical)
                        {
                            localUV = new Vector2(bbAspectRatio / matAspectRatio, 1f);
                        }
                        else if (Alignment == MaterialAlignment.FitHorizontal)
                        {
                            localUV = new Vector2(1f, matAspectRatio / bbAspectRatio);
                        }

                        matStep *= localUV;
                    }

                    return new BoundingBox2
                    (
                        matOrigin - matStep, // Bottom left
                        matOrigin + matStep // Upper right
                    );
                }
            }
        }
    }
}