using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI.Rendering
{
    using Client;
    using Server;

    /// <summary>
    /// Renders a 2D polygon using billboards with the center punched out.
    /// </summary>
    public class PuncturedPolyBoard : PolyBoard
    {
        /// <summary>
        /// Scaled inner radius as fractional offset from exterior edge.
        /// </summary>
        public float InnerRadius
        {
            get { return _innerRadius; }
            set
            {
                if (value != _innerRadius)
                    updateVertices = true;

                _innerRadius = value;
            }
        }

        private float _innerRadius;

        public PuncturedPolyBoard()
        {
            _innerRadius = 0.6f;
        }

        /// <summary>
        /// Draws the given range of faces
        /// </summary>
        public override void Draw(Vector2 size, Vector2 origin, Vector2I faceRange, MatrixD[] matrixRef)
        {
            if (_sides > 2 && drawVertices.Count > 5)
            {
                if (updateVertices)
                    GeneratePolygon();

                if (updateMatFit)
                {
                    polyMat.texBounds = matFrame.GetMaterialAlignment(size.X / size.Y);
                    GenerateTextureCoordinates();
                    updateMatFit = false;
                }

                faceRange.Y++;
                faceRange *= 2;
                // Outer vertex indices are even
                faceRange.X -= faceRange.X % 2;
                faceRange.Y -= faceRange.Y % 2;

                // Generate final vertices for drawing from unscaled vertices
                for (int i = faceRange.X; i <= faceRange.Y + 1; i++)
                {
                    drawVertices[i % drawVertices.Count] = origin + size * vertices[i % drawVertices.Count];
                }

                faceRange *= 3;
                BillBoardUtils.AddTriangleRange(faceRange, triangles, drawVertices, ref polyMat, matrixRef);
            }
        }

        /// <summary>
        /// Returns the center position of the given slice relative to the center of the billboard
        /// </summary>
        public override Vector2 GetSliceOffset(Vector2 bbSize, Vector2I range)
        {
            if (updateVertices)
                GeneratePolygon();

            range.Y++;
            range *= 2;
            // Outer vertex indices are even
            range.X -= range.X % 2;
            range.Y -= range.Y % 2;

            int max = vertices.Count;
            Vector2 sum = 
                vertices[range.X] +
                vertices[range.X + 1] +
                vertices[(range.Y) % max] +
                vertices[(range.Y + 1) % max];

            return bbSize * sum * .25f;
        }

        protected override void GenerateTriangles()
        {
            int max = vertices.Count;
            triangles.Clear();
            triangles.EnsureCapacity(_sides * 3 * 2);

            for (int i = 0; i < vertices.Count; i += 2)
            {
                int outerStart = i,
                    innerStart = (i + 1) % max,
                    outerEnd = (i + 2) % max,
                    innerEnd = (i + 3) % max;

                // Left Upper
                triangles.Add(outerStart);
                triangles.Add(outerEnd);
                triangles.Add(innerStart);

                // Right Lower
                triangles.Add(outerEnd);
                triangles.Add(innerEnd);
                triangles.Add(innerStart);
            }
        }

        protected override void GenerateVertices()
        {
            float rotStep = (float)(Math.PI * 2f / _sides),
                rotPos = -.5f * rotStep;

            _innerRadius = Math.Min(1f - 0.01f, _innerRadius);
            vertices.Clear();
            vertices.EnsureCapacity(_sides * 2);

            for (int i = 0; i < _sides; i++)
            {
                Vector2 outerStart = Vector2.Zero;
                outerStart.X = (float)Math.Cos(rotPos);
                outerStart.Y = (float)Math.Sin(rotPos);

                Vector2 innerStart = outerStart * _innerRadius;

                vertices.Add(.5f * outerStart);
                vertices.Add(.5f * innerStart);
                rotPos += rotStep;
            }
        }
    }
}