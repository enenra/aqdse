using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI.Rendering
{
    using Client;
    using Server;

    /// <summary>
    /// Renders a 2D polygon using billboards
    /// </summary>
    public class PolyBoard
    {
        /// <summary>
        /// Tinting applied to the material
        /// </summary>
        public virtual Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                    polyMat.bbColor = BillBoardUtils.GetBillBoardBoardColor(value);

                color = value;
            }
        }

        /// <summary>
        /// Texture applied to the billboard.
        /// </summary>
        public virtual Material Material
        {
            get { return matFrame.Material; }
            set
            {
                if (value != matFrame.Material)
                {
                    updateMatFit = true;
                    matFrame.Material = value;
                    polyMat.textureID = value.TextureID;
                }
            }
        }

        /// <summary>
        /// Determines how the texture scales with the MatBoard's dimensions.
        /// </summary>
        public MaterialAlignment MatAlignment
        {
            get { return matFrame.Alignment; }
            set
            {
                if (value != matFrame.Alignment)
                {
                    updateMatFit = true;
                    matFrame.Alignment = value;
                }
            }
        }

        /// <summary>
        /// Get/set number of sides on the polygon
        /// </summary>
        public virtual int Sides
        {
            get { return _sides; }
            set
            {
                if (value != _sides)
                    updateVertices = true;

                _sides = value;
            }
        }

        protected int _sides;

        protected Color color;
        protected bool updateVertices, updateMatFit;

        protected PolyMaterial polyMat;
        protected readonly MaterialFrame matFrame;
        protected readonly List<int> triangles;
        protected readonly List<Vector2> vertices;
        protected readonly List<Vector2> drawVertices;

        public PolyBoard()
        {
            triangles = new List<int>();
            vertices = new List<Vector2>();
            drawVertices = new List<Vector2>();

            matFrame = new MaterialFrame();
            polyMat = PolyMaterial.Default;
            polyMat.texCoords = new List<Vector2>();

            _sides = 16;
            updateVertices = true;
        }

        /// <summary>
        /// Draws a polygon using billboards
        /// </summary>
        public virtual void Draw(Vector2 size, Vector2 origin, MatrixD[] matrixRef)
        {
            if (_sides > 2 && drawVertices.Count > 2)
            {
                if (updateVertices)
                    GeneratePolygon();

                if (updateMatFit)
                {
                    polyMat.texBounds = matFrame.GetMaterialAlignment(size.X / size.Y);
                    GenerateTextureCoordinates();
                    updateMatFit = false;
                }

                // Generate final vertices for drawing from unscaled vertices
                for (int i = 0; i < drawVertices.Count; i++)
                {
                    drawVertices[i] = origin + size * vertices[i];
                }

                BillBoardUtils.AddTriangles(triangles, drawVertices, ref polyMat, matrixRef);
            }
        }

        /// <summary>
        /// Draws the given range of faces
        /// </summary>
        public virtual void Draw(Vector2 size, Vector2 origin, Vector2I faceRange, MatrixD[] matrixRef)
        {
            if (_sides > 2 && drawVertices.Count > 2)
            {
                if (updateVertices)
                    GeneratePolygon();

                if (updateMatFit)
                {
                    polyMat.texBounds = matFrame.GetMaterialAlignment(size.X / size.Y);
                    GenerateTextureCoordinates();
                    updateMatFit = false;
                }

                // Generate final vertices for drawing from unscaled vertices
                int max = drawVertices.Count - 1;
                drawVertices[max] = origin + size * vertices[max];

                for (int i = 0; i < drawVertices.Count; i++)
                {
                    drawVertices[i] = origin + size * vertices[i];
                }

                faceRange *= 3;
                BillBoardUtils.AddTriangleRange(faceRange, triangles, drawVertices, ref polyMat, matrixRef);
            }
        }

        /// <summary>
        /// Returns the center position of the given slice relative to the center of the billboard
        /// </summary>
        public virtual Vector2 GetSliceOffset(Vector2 bbSize, Vector2I range)
        {
            if (updateVertices)
                GeneratePolygon();

            int max = vertices.Count;
            Vector2 start = vertices[range.X], 
                end = vertices[(range.Y + 1) % max], 
                center = Vector2.Zero;

            return bbSize * (start + end + center) / 3f;
        }

        protected virtual void GeneratePolygon()
        {
            GenerateVertices();
            GenerateTriangles();
            drawVertices.Clear();

            for (int i = 0; i < vertices.Count; i++)
                drawVertices.Add(Vector2.Zero);

            updateMatFit = true;
        }

        protected virtual void GenerateTriangles()
        {
            int max = vertices.Count - 1;
            triangles.Clear();
            triangles.EnsureCapacity(_sides * 3);

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                triangles.Add(max);
                triangles.Add(i);
                triangles.Add((i + 1) % max);
            }
        }

        protected virtual void GenerateTextureCoordinates()
        {
            Vector2 texScale = polyMat.texBounds.Size,
                texCenter = polyMat.texBounds.Center;

            polyMat.texCoords.Clear();
            polyMat.texCoords.EnsureCapacity(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 uv = vertices[i] * texScale;
                uv.Y *= -1f;

                polyMat.texCoords.Add(uv + texCenter);
            }
        }

        protected virtual void GenerateVertices()
        {
            float rotStep = (float)(Math.PI * 2f / _sides),
                rotPos = -.5f * rotStep;

            vertices.Clear();
            vertices.EnsureCapacity(_sides + 1);

            for (int i = 0; i < _sides; i++)
            {
                Vector2 point = Vector2.Zero;
                point.X = (float)Math.Cos(rotPos);
                point.Y = (float)Math.Sin(rotPos);

                vertices.Add(.5f * point);
                rotPos += rotStep;
            }

            vertices.Add(Vector2.Zero);
        }
    }
}