using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Utils;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Material data for rendering individual triangles.
            /// </summary>
            public struct TriMaterial
            {
                public static readonly TriMaterial Default = new TriMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = Vector4.One,
                    texCoords = new Triangle(
                        new Vector2(0f, 0f),
                        new Vector2(0f, 1f),
                        new Vector2(1f, 0f)
                    )
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Normalized texture coordinates
                /// </summary>
                public Triangle texCoords;
            }

            /// <summary>
            /// Material data for rendering quads.
            /// </summary>
            public struct QuadMaterial
            {
                public static readonly QuadMaterial Default = new QuadMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = Vector4.One,
                    texCoords = new FlatQuad(
                        new Vector2(0f, 0f),
                        new Vector2(0f, 1f),
                        new Vector2(1f, 0f),
                        new Vector2(1f, 1f)
                    )
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Normalized texture coordinates
                /// </summary>
                public FlatQuad texCoords;
            }

            /// <summary>
            /// Material data for rendering quads with texture coordinates defined by a bounding box.
            /// </summary>
            public struct BoundedQuadMaterial
            {
                public static readonly BoundedQuadMaterial Default = new BoundedQuadMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = Vector4.One,
                    texBounds = new BoundingBox2(Vector2.Zero, Vector2.One)
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Determines the scale and aspect ratio of the texture as rendered
                /// </summary>
                public BoundingBox2 texBounds;
            }

            /// <summary>
            /// Material data for rendering polygons.
            /// </summary>
            public struct PolyMaterial
            {
                public static readonly PolyMaterial Default = new PolyMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = Vector4.One,
                    texCoords = null
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Min/max texcoords
                /// </summary>
                public BoundingBox2 texBounds;

                /// <summary>
                /// Normalized texture coordinates
                /// </summary>
                public List<Vector2> texCoords;
            }

            /// <summary>
            /// Defines a quad comprised of four <see cref="Vector2"/>s.
            /// </summary>
            public struct FlatQuad
            {
                public Vector2 Point0, Point1, Point2, Point3;

                public FlatQuad(Vector2 Point0, Vector2 Point1, Vector2 Point2, Vector2 Point3)
                {
                    this.Point0 = Point0;
                    this.Point1 = Point1;
                    this.Point2 = Point2;
                    this.Point3 = Point3;
                }
            }

            /// <summary>
            /// A set of three vectors defining a triangle
            /// </summary>
            public struct Triangle
            {
                public Vector2 Point0, Point1, Point2;

                public Triangle(Vector2 Point0, Vector2 Point1, Vector2 Point2)
                {
                    this.Point0 = Point0;
                    this.Point1 = Point1;
                    this.Point2 = Point2;
                }
            }

            /// <summary>
            /// A set of three vectors defining a triangle
            /// </summary>
            public struct TriangleD
            {
                public Vector3D Point0, Point1, Point2;

                public TriangleD(Vector3D Point0, Vector3D Point1, Vector3D Point2)
                {
                    this.Point0 = Point0;
                    this.Point1 = Point1;
                    this.Point2 = Point2;
                }
            }
        }
    }
}