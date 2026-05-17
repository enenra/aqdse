using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A textured frame. The default texture is just a plain color.
    /// </summary>
    public class BorderBox : HudElementBase
    {
        /// <summary>
        /// Material applied to the box.
        /// </summary>
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }

        /// <summary>
        /// Determines how the material reacts to changes in element size/aspect ratio.
        /// </summary>
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }

        /// <summary>
        /// Coloring applied to the material.
        /// </summary>
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }

        /// <summary>
        /// Size of the border on all four sides in pixels.
        /// </summary>
        public float Thickness { get { return _thickness; } set { _thickness = value; } }

        private float _thickness;
        protected readonly MatBoard hudBoard;

        public BorderBox(HudParentBase parent) : base(parent)
        {
            hudBoard = new MatBoard();
            Thickness = 1f;
        }

        public BorderBox() : this(null)
        { }

        protected override void Draw()
        {
            if (Color.A > 0)
            {
                CroppedBox box = default(CroppedBox);
                box.mask = maskingBox;

                float thickness = _thickness, 
                    height = cachedSize.Y - cachedPadding.Y, 
                    width = cachedSize.X - cachedPadding.X;
                Vector2 halfSize, pos;

                // Left
                halfSize = new Vector2(thickness, height) * .5f;
                pos = cachedPosition + new Vector2((-width + thickness) * .5f, 0f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                // Top
                halfSize = new Vector2(width, thickness) * .5f;
                pos = cachedPosition + new Vector2(0f, (height - thickness) * .5f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                // Right
                halfSize = new Vector2(thickness, height) * .5f;
                pos = cachedPosition + new Vector2((width - thickness) * .5f, 0f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                // Bottom
                halfSize = new Vector2(width, thickness) * .5f;
                pos = cachedPosition + new Vector2(0f, (-height + thickness) * .5f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);
            }
        }
    }
}
