using VRage.Utils;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Used to determine how a given <see cref="Material"/> is scaled on a given Billboard.
            /// Note: texture colors are clamped to their edges.
            /// </summary>
            public enum MaterialAlignment : int
            {
                /// <summary>
                /// Stretches/compresses the material to cover the whole billboard. Default behavior.
                /// </summary>
                StretchToFit = 0,

                /// <summary>
                ///  Rescales the material so that it matches the height of the Billboard while maintaining its aspect ratio.
                ///  Material will be clipped as needed.
                /// </summary>
                FitVertical = 1,

                /// <summary>
                /// Rescales the material so that it matches the width of the Billboard while maintaining its aspect ratio.
                /// Material will be clipped as needed.
                /// </summary>
                FitHorizontal = 2,

                /// <summary>
                /// Rescales the material such that it maintains it's aspect ratio while filling as much of the billboard
                /// as possible
                /// </summary>
                FitAuto = 3,
            }

            /// <summary>
            /// Defines a texture used by <see cref="MatBoard"/>s. Supports sprite sheets.
            /// </summary>
            public class Material
            {
                public static readonly Material Default = new Material("RichHudDefault", new Vector2(4f, 4f)),
                    CircleMat = new Material("RhfCircle", new Vector2(1024f)),
                    AnnulusMat = new Material("RhfAnnulus", new Vector2(1024f));

                /// <summary>
                /// ID of the Texture the <see cref="Material"/> is based on.
                /// </summary>
                public readonly MyStringId TextureID;

                /// <summary>
                /// The dimensions, in pixels, of the <see cref="Material"/>.
                /// </summary>
                public readonly Vector2 size;

                /// <summary>
                /// The dimensions of the <see cref="Material"/> in normalized texture coordinates.
                /// </summary>
                public readonly Vector2 uvSize;

                /// <summary>
                /// Center of the <see cref="Material"/> in normalized texture coordinates.
                /// </summary>
                public readonly Vector2 uvOffset;

                /// <summary>
                /// Creates a <see cref="Material"/> using the name of the Texture's ID and its size in pixels.
                /// </summary>
                /// <param name="TextureName">Name of the texture ID</param>
                /// <param name="size">Size of the material in pixels</param>
                public Material(string TextureName, Vector2 size) : this(MyStringId.GetOrCompute(TextureName), size)
                { }

                /// <summary>
                /// Creates a <see cref="Material"/> based on a Texture Atlas/Sprite with a given offset and size.
                /// </summary>
                /// <param name="TextureName">Name of the texture ID</param>
                /// <param name="texSize">Size of the texture associated with the texture ID in pixels</param>
                /// <param name="texCoords">UV offset starting from the upper left hand corner in pixels</param>
                /// <param name="size">Size of the material starting from the given offset</param>
                public Material(string TextureName, Vector2 texSize, Vector2 texCoords, Vector2 size)
                    : this(MyStringId.GetOrCompute(TextureName), texSize, texCoords, size)
                { }

                /// <summary>
                /// Creates a <see cref="Material"/> using the name of the Texture's ID and its size in pixels.
                /// </summary>
                /// <param name="TextureID">MyStringID associated with the texture</param>
                /// <param name="size">Size of the material in pixels</param>
                public Material(MyStringId TextureID, Vector2 size)
                {
                    this.TextureID = TextureID;
                    this.size = size;

                    uvSize = Vector2.One;
                    uvOffset = uvSize * .5f;
                }

                /// <summary>
                /// Creates a <see cref="Material"/> based on a Texture Atlas/Sprite with a given offset and size.
                /// </summary>
                /// <param name="TextureID">MyStringID associated with the texture</param>
                /// <param name="texSize">Size of the texture associated with the texture ID in pixels</param>
                /// <param name="texCoords">UV offset starting from the upper left hand corner in pixels</param>
                /// <param name="size">Size of the material starting from the given offset</param>
                public Material(MyStringId TextureID, Vector2 textureSize, Vector2 offset, Vector2 size)
                {
                    this.TextureID = TextureID;
                    this.size = size;

                    size.X /= textureSize.X;
                    size.Y /= textureSize.Y;

                    uvSize = size;

                    offset.X /= textureSize.X;
                    offset.Y /= textureSize.Y;

                    uvOffset = offset + (uvSize * .5f);
                }
            }

        }
    }
}