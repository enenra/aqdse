using VRageMath;
using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Draws a rectangular prism using billboards in world space.
            /// </summary>
            public class BlockBoard
            {
                /// <summary>
                /// Controls the dimensions of the block.
                /// </summary>
                public Vector3D Size { get; set; }

                /// <summary>
                /// Determines the distance of the block from the center of its transform.
                /// </summary>
                public Vector3D Offset { get; set; }

                /// <summary>
                /// Material board for the front face (-Z).
                /// </summary>
                public MatBoard Front => faces[0];

                /// <summary>
                /// Material board for the back face (+Z).
                /// </summary>
                public MatBoard Back => faces[1];

                /// <summary>
                /// Material board for the top face (+Y).
                /// </summary>
                public MatBoard Top => faces[2];

                /// <summary>
                /// Material board for the bottom face (-Y).
                /// </summary>
                public MatBoard Bottom => faces[3];

                /// <summary>
                /// Material board for the left face (-X).
                /// </summary>
                public MatBoard Left => faces[4];

                /// <summary>
                /// Material board for the right face (+X).
                /// </summary>
                public MatBoard Right => faces[5];

                /// <summary>
                /// Gets all six faces of the block as a read only list.
                /// </summary>
                public IReadOnlyList<MatBoard> Faces => faces;

                private readonly MatBoard[] faces;
                private readonly Vector3D[] octant;

                public BlockBoard()
                {
                    faces = new MatBoard[6];
                    octant = new Vector3D[8];

                    for (int n = 0; n < 6; n++)
                        faces[n] = new MatBoard();
                }

                /// <summary>
                /// Sets the color for every face.
                /// </summary>
                public void SetColor(Color color)
                {
                    for (int n = 0; n < 6; n++)
                        faces[n].Color = color;
                }

                /// <summary>
                /// Sets every face to the given material.
                /// </summary>
                public void SetMaterial(Material material)
                {
                    for (int n = 0; n < 6; n++)
                        faces[n].Material = material;
                }

                /// <summary>
                /// Sets every face's material alignment.
                /// </summary>
                public void SetMaterialAlignment(MaterialAlignment materialAlignment)
                {
                    for (int n = 0; n < 6; n++)
                        faces[n].MatAlignment = materialAlignment;
                }

                /// <summary>
                /// Draws a block made of billboards in world space using the given matrix transform.
                /// </summary>
                public void Draw(ref MatrixD matrix)
                {
                    MyQuadD faceQuad;
                    UpdateOctant(ref matrix);

                    // -Z/+Z
                    faceQuad.Point0 = octant[3];
                    faceQuad.Point1 = octant[2];
                    faceQuad.Point2 = octant[1];
                    faceQuad.Point3 = octant[0];

                    faces[0].Draw(ref faceQuad);

                    faceQuad.Point0 = octant[4];
                    faceQuad.Point1 = octant[5];
                    faceQuad.Point2 = octant[6];
                    faceQuad.Point3 = octant[7];

                    faces[1].Draw(ref faceQuad);

                    // -Y/+Y
                    faceQuad.Point0 = octant[7];
                    faceQuad.Point1 = octant[6];
                    faceQuad.Point2 = octant[2];
                    faceQuad.Point3 = octant[3];

                    faces[2].Draw(ref faceQuad);

                    faceQuad.Point0 = octant[0];
                    faceQuad.Point1 = octant[1];
                    faceQuad.Point2 = octant[5];
                    faceQuad.Point3 = octant[4];

                    faces[3].Draw(ref faceQuad);

                    // -X/+X
                    faceQuad.Point0 = octant[0];
                    faceQuad.Point1 = octant[4];
                    faceQuad.Point2 = octant[7];
                    faceQuad.Point3 = octant[3];

                    faces[4].Draw(ref faceQuad);

                    faceQuad.Point0 = octant[5];
                    faceQuad.Point1 = octant[1];
                    faceQuad.Point2 = octant[2];
                    faceQuad.Point3 = octant[6];

                    faces[5].Draw(ref faceQuad);
                }

                private void UpdateOctant(ref MatrixD matrix)
                {
                    Vector3D size = Size * 0.5d;

                    octant[0] = new Vector3D(-size.X, size.Y, -size.Z);
                    octant[1] = new Vector3D(size.X, size.Y, -size.Z);
                    octant[2] = new Vector3D(size.X, -size.Y, -size.Z);
                    octant[3] = new Vector3D(-size.X, -size.Y, -size.Z);

                    octant[4] = new Vector3D(-size.X, size.Y, size.Z);
                    octant[5] = new Vector3D(size.X, size.Y, size.Z);
                    octant[6] = new Vector3D(size.X, -size.Y, size.Z);
                    octant[7] = new Vector3D(-size.X, -size.Y, size.Z);

                    for (int n = 0; n < 8; n++)
                        octant[n] = Vector3D.Transform(octant[n], ref matrix) + Offset;
                }
            }
        }
    }
}