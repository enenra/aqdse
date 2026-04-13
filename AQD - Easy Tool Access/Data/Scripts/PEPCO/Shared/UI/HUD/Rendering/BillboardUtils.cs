using System.Collections.Generic;
using System;
using System.Threading;
using Sandbox.ModAPI;
using VRage.Game;
using VRage;
using VRage.Utils;
using VRageMath;
using VRageRender;
using RichHudFramework.Internal;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RichHudFramework
{
    namespace UI
    {
        using ApiMemberAccessor = System.Func<object, int, object>;
        using TriangleBillboardData = MyTuple<
            BlendTypeEnum, // blendType
            Vector2I, // bbID + matrixID
            MyStringId, // material
            Vector4, // color
            MyTuple<Vector2, Vector2, Vector2>, // texCoords
            MyTuple<Vector3D, Vector3D, Vector3D> // vertexPos
        >;
        using FlatTriangleBillboardData = MyTuple<
            BlendTypeEnum, // blendType
            Vector2I, // bbID + matrixID
            MyStringId, // material
            MyTuple<Vector4, BoundingBox2?>, // color + mask
            MyTuple<Vector2, Vector2, Vector2>, // texCoords
            MyTuple<Vector2, Vector2, Vector2> // flat pos
        >;

        namespace Rendering
        {
            public enum BillBoardUtilAccessors : int
            {
                /// <summary>
                /// out: List<MyTriangleBillboard>
                /// </summary>
                GetPoolBack = 1
            }

            public sealed partial class BillBoardUtils
            {
                #region 3D Billboards

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices and the tex coords are parallel to the vertex list.
                /// </summary>
                public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref PolyMaterial mat, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    int triangleCount = indices.Count / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var bb = new TriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = mat.bbColor,
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texCoords[indices[i]],
                                mat.texCoords[indices[i + 1]],
                                mat.texCoords[indices[i + 2]]
                            ),
                            Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                            (
                                vertices[indices[i]],
                                vertices[indices[i + 1]],
                                vertices[indices[i + 2]]
                            ),
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a triangles in the given starting index range
                /// </summary>
                public static void AddTriangleRange(Vector2I range, IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref PolyMaterial mat, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    int iMax = indices.Count,
                        triangleCount = (range.Y - range.X) / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = range.X; i <= range.Y; i += 3)
                    {
                        var bb = new TriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = mat.bbColor,
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texCoords[indices[i % iMax]],
                                mat.texCoords[indices[(i + 1) % iMax]],
                                mat.texCoords[indices[(i + 2) % iMax]]
                            ),
                            Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                            (
                                vertices[indices[i % iMax]],
                                vertices[indices[(i + 1) % iMax]],
                                vertices[indices[(i + 2) % iMax]]
                            ),
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices.
                /// </summary>
                public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref TriMaterial mat, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    int triangleCount = indices.Count / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var bb = new TriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = mat.bbColor,
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texCoords.Point0,
                                mat.texCoords.Point1,
                                mat.texCoords.Point2
                            ),
                            Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                            (
                                vertices[indices[i]],
                                vertices[indices[i + 1]],
                                vertices[indices[i + 2]]
                            ),
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a list of textured quads in one batch using QuadBoard data
                /// </summary>
                public static void AddQuads(IReadOnlyList<QuadBoardData> quads, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    int triangleCount = quads.Count * 2,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < quads.Count; i++)
                    {
                        QuadBoardData quadBoard = quads[i];
                        MyQuadD quad = quadBoard.positions;
                        BoundedQuadMaterial mat = quadBoard.material;

                        var bbL = new TriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = mat.bbColor,
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texBounds.Min,
                                mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y),
                                mat.texBounds.Max
                            ),
                            Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                            (
                                quad.Point0,
                                quad.Point1,
                                quad.Point2
                            ),

                        };
                        var bbR = new TriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count + 1, matrixID),
                            Item3 = mat.textureID,
                            Item4 = mat.bbColor,
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texBounds.Min,
                                mat.texBounds.Max,
                                mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f)
                            ),
                            Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                            (
                                quad.Point0,
                                quad.Point2,
                                quad.Point3
                            ),
                        };

                        bbDataBack.Add(bbL);
                        bbDataBack.Add(bbR);
                    }
                }

                /// <summary>
                /// Adds a triangle starting at the given index.
                /// </summary>
                public static void AddTriangle(int start, IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref TriMaterial mat, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int index = bbDataBack.Count;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    var bb = new TriangleBillboardData
                    {
                        Item1 = BlendTypeEnum.PostPP,
                        Item2 = new Vector2I(index, matrixID),
                        Item3 = mat.textureID,
                        Item4 = mat.bbColor,
                        Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texCoords.Point0,
                                mat.texCoords.Point1,
                                mat.texCoords.Point2
                            ),
                        Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                            (
                                vertices[indices[start]],
                                vertices[indices[start + 1]],
                                vertices[indices[start + 2]]
                            ),
                    };
                    bbDataBack.Add(bb);

                    if (index >= bbPool.Count)
                        instance.AddNewBB(index - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[index], false);
                }

                /// <summary>
                /// Queues a single triangle billboard for rendering
                /// </summary>
                public static void AddTriangle(ref TriMaterial mat, ref TriangleD triangle, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int index = bbDataBack.Count;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    var bb = new TriangleBillboardData
                    {
                        Item1 = BlendTypeEnum.PostPP,
                        Item2 = new Vector2I(index, matrixID),
                        Item3 = mat.textureID,
                        Item4 = mat.bbColor,
                        Item5 = new MyTuple<Vector2, Vector2, Vector2>
                        (
                            mat.texCoords.Point0,
                            mat.texCoords.Point1,
                            mat.texCoords.Point2
                        ),
                        Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                        (
                            triangle.Point0,
                            triangle.Point1,
                            triangle.Point2
                        ),
                    };
                    bbDataBack.Add(bb);

                    if (index >= bbPool.Count)
                        instance.AddNewBB(index - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[index], false);
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref QuadMaterial mat, ref MyQuadD quad, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int indexL = bbDataBack.Count,
                        indexR = bbDataBack.Count + 1;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    var bbL = new TriangleBillboardData
                    {
                        Item1 = BlendTypeEnum.PostPP,
                        Item2 = new Vector2I(indexL, matrixID),
                        Item3 = mat.textureID,
                        Item4 = mat.bbColor,
                        Item5 = new MyTuple<Vector2, Vector2, Vector2>
                        (
                            mat.texCoords.Point0,
                            mat.texCoords.Point1,
                            mat.texCoords.Point2
                        ),
                        Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                        (
                            quad.Point0,
                            quad.Point1,
                            quad.Point2
                        ),
                    };
                    var bbR = new TriangleBillboardData
                    {
                        Item1 = BlendTypeEnum.PostPP,
                        Item2 = new Vector2I(indexR, matrixID),
                        Item3 = mat.textureID,
                        Item4 = mat.bbColor,
                        Item5 = new MyTuple<Vector2, Vector2, Vector2>
                        (
                            mat.texCoords.Point0,
                            mat.texCoords.Point2,
                            mat.texCoords.Point3
                        ),
                        Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                        (
                            quad.Point0,
                            quad.Point2,
                            quad.Point3
                        ),
                    };

                    bbDataBack.Add(bbL);
                    bbDataBack.Add(bbR);

                    if (indexR >= bbPool.Count)
                        instance.AddNewBB(indexR - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                    MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref BoundedQuadMaterial mat, ref MyQuadD quad, MatrixD[] matrixRef = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int indexL = bbDataBack.Count,
                        indexR = bbDataBack.Count + 1;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID = -1;

                    if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    var bbL = new TriangleBillboardData
                    {
                        Item1 = BlendTypeEnum.PostPP,
                        Item2 = new Vector2I(indexL, matrixID),
                        Item3 = mat.textureID,
                        Item4 = mat.bbColor,
                        Item5 = new MyTuple<Vector2, Vector2, Vector2>
                        (
                            mat.texBounds.Min,
                            mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y),
                            mat.texBounds.Max
                        ),
                        Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                        (
                            quad.Point0,
                            quad.Point1,
                            quad.Point2
                        ),
                    };
                    var bbR = new TriangleBillboardData
                    {
                        Item1 = BlendTypeEnum.PostPP,
                        Item2 = new Vector2I(indexR, matrixID),
                        Item3 = mat.textureID,
                        Item4 = mat.bbColor,
                        Item5 = new MyTuple<Vector2, Vector2, Vector2>
                        (
                            mat.texBounds.Min,
                            mat.texBounds.Max,
                            mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f)
                        ),
                        Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
                        (
                            quad.Point0,
                            quad.Point2,
                            quad.Point3
                        ),
                    };

                    bbDataBack.Add(bbL);
                    bbDataBack.Add(bbR);

                    if (indexR >= bbPool.Count)
                        instance.AddNewBB(indexR - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                    MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                }

                #endregion

                #region 2D Billboards

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices and the tex coords are parallel to the vertex list.
                /// </summary>
                public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector2> vertices, ref PolyMaterial mat, MatrixD[] matrixRef)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.flatTriangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID;

                    if (!matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    // Get triangle count, ensure enough billboards are in the pool and add them to the
                    // render queue before writing QB data to buffer
                    int triangleCount = indices.Count / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var bb = new FlatTriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, null),
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texCoords[indices[i]],
                                mat.texCoords[indices[i + 1]],
                                mat.texCoords[indices[i + 2]]
                            ),
                            Item6 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                vertices[indices[i]],
                                vertices[indices[i + 1]],
                                vertices[indices[i + 2]]
                            ),
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a triangles in the given starting index range
                /// </summary>
                public static void AddTriangleRange(Vector2I range, IReadOnlyList<int> indices, IReadOnlyList<Vector2> vertices, ref PolyMaterial mat, MatrixD[] matrixRef)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.flatTriangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID;

                    if (!matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    // Get triangle count, ensure enough billboards are in the pool and add them to the
                    // render queue before writing QB data to buffer
                    int iMax = indices.Count,
                        triangleCount = (range.Y - range.X) / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = range.X; i <= range.Y; i += 3)
                    {
                        var bb = new FlatTriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, null),
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                mat.texCoords[indices[i % iMax]],
                                mat.texCoords[indices[(i + 1) % iMax]],
                                mat.texCoords[indices[(i + 2) % iMax]]
                            ),
                            Item6 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                vertices[indices[i % iMax]],
                                vertices[indices[(i + 1) % iMax]],
                                vertices[indices[(i + 2) % iMax]]
                            ),
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a list of textured quads in one batch using QuadBoard data
                /// </summary>
                public static void AddQuads(IReadOnlyList<BoundedQuadBoard> quads, MatrixD[] matrixRef, BoundingBox2? mask = null,
                    Vector2 offset = default(Vector2), float scale = 1f)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.flatTriangleList;
                    var bbBuf = instance.bbBuf;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID;

                    if (!matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    int triangleCount = quads.Count * 2,
                        bbCountStart = bbDataBack.Count;

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < quads.Count; i++)
                    {
                        BoundedQuadBoard boundedQB = quads[i];
                        BoundedQuadMaterial mat = boundedQB.quadBoard.materialData;
                        Vector2 size = boundedQB.bounds.Size * scale,
                            center = offset + boundedQB.bounds.Center * scale;

                        BoundingBox2 bounds = BoundingBox2.CreateFromHalfExtent(center, .5f * size);
                        BoundingBox2? maskBox = mask;
                        ContainmentType containment = ContainmentType.Contains;

                        if (maskBox != null)
                        {
                            maskBox.Value.Contains(ref bounds, out containment);

                            if (containment == ContainmentType.Contains)
                                maskBox = null;
                        }

                        if (containment != ContainmentType.Disjoint)
                        {
                            var bbL = new FlatTriangleBillboardData
                            {
                                Item1 = BlendTypeEnum.PostPP,
                                Item2 = new Vector2I(bbDataBack.Count, matrixID),
                                Item3 = mat.textureID,
                                Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, maskBox),
                                Item5 = new MyTuple<Vector2, Vector2, Vector2>
                                (
                                    new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                    mat.texBounds.Max, // 0
                                    new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y) // 3
                                ),
                                Item6 = new MyTuple<Vector2, Vector2, Vector2>
                                (
                                    bounds.Max,
                                    new Vector2(bounds.Max.X, bounds.Min.Y),
                                    bounds.Min
                                ),
                            };
                            var bbR = new FlatTriangleBillboardData
                            {
                                Item1 = BlendTypeEnum.PostPP,
                                Item2 = new Vector2I(bbDataBack.Count + 1, matrixID),
                                Item3 = mat.textureID,
                                Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, maskBox),
                                Item5 = new MyTuple<Vector2, Vector2, Vector2>
                                (
                                    new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                    new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                    mat.texBounds.Min // 2
                                ),
                                Item6 = new MyTuple<Vector2, Vector2, Vector2>
                                (
                                    bounds.Max,
                                    bounds.Min,
                                    new Vector2(bounds.Min.X, bounds.Max.Y)
                                ),
                            };

                            bbDataBack.Add(bbL);
                            bbDataBack.Add(bbR);
                        }
                    }

                    // Add more billboards to pool as needed then queue them for rendering
                    int bbToAdd = Math.Max(bbDataBack.Count - bbPool.Count, 0);
                    instance.AddNewBB(bbToAdd);

                    for (int i = bbCountStart; i < bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref BoundedQuadBoard boundedQB, MatrixD[] matrixRef, BoundingBox2? mask = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.flatTriangleList;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID;

                    if (!matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    // Mask bounding check. Null mask if not intersecting.
                    BoundingBox2? maskBox = mask;
                    ContainmentType containment = ContainmentType.Contains;

                    if (maskBox != null)
                    {
                        maskBox.Value.Contains(ref boundedQB.bounds, out containment);

                        if (containment == ContainmentType.Contains)
                            maskBox = null;
                    }

                    if (containment != ContainmentType.Disjoint)
                    {
                        int indexL = bbDataBack.Count,
                            indexR = bbDataBack.Count + 1;
                        BoundedQuadMaterial mat = boundedQB.quadBoard.materialData;

                        var bbL = new FlatTriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, maskBox),
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                mat.texBounds.Max, // 0
                                new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y) // 3
                            ),
                            Item6 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                boundedQB.bounds.Max,
                                new Vector2(boundedQB.bounds.Max.X, boundedQB.bounds.Min.Y),
                                boundedQB.bounds.Min
                            ),
                        };
                        var bbR = new FlatTriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count + 1, matrixID),
                            Item3 = mat.textureID,
                            Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, maskBox),
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                mat.texBounds.Min // 2
                            ),
                            Item6 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                boundedQB.bounds.Max,
                                boundedQB.bounds.Min,
                                new Vector2(boundedQB.bounds.Min.X, boundedQB.bounds.Max.Y)
                            ),
                        };

                        bbDataBack.Add(bbL);
                        bbDataBack.Add(bbR);

                        if (indexR >= bbPool.Count)
                            instance.AddNewBB(indexR - (bbPool.Count - 1));

                        MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                        MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                    }
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref FlatQuad quad, ref BoundedQuadMaterial mat, MatrixD[] matrixRef, BoundingBox2? mask = null)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.flatTriangleList;
                    var matList = instance.matrixBuf;
                    var matTable = instance.matrixTable;

                    // Find matrix index in table or add it
                    int matrixID;

                    if (!matTable.TryGetValue(matrixRef, out matrixID))
                    {
                        matrixID = matList.Count;
                        matList.Add(matrixRef[0]);
                        matTable.Add(matrixRef, matrixID);
                    }

                    // Mask bounding check. Null mask if not intersecting.
                    BoundingBox2? maskBox = mask;
                    ContainmentType containment = ContainmentType.Contains;

                    if (maskBox != null)
                    {
                        BoundingBox2 bounds = new BoundingBox2(quad.Point2, quad.Point0);
                        maskBox.Value.Contains(ref bounds, out containment);

                        if (containment == ContainmentType.Contains)
                            maskBox = null;
                    }

                    if (containment != ContainmentType.Disjoint)
                    {
                        int indexL = bbDataBack.Count,
                            indexR = bbDataBack.Count + 1;

                        var bbL = new FlatTriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count, matrixID),
                            Item3 = mat.textureID,
                            Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, maskBox),
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                mat.texBounds.Max, // 0
                                new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y) // 3
                            ),
                            Item6 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                quad.Point0,
                                quad.Point1,
                                quad.Point2
                            ),
                        };
                        var bbR = new FlatTriangleBillboardData
                        {
                            Item1 = BlendTypeEnum.PostPP,
                            Item2 = new Vector2I(bbDataBack.Count + 1, matrixID),
                            Item3 = mat.textureID,
                            Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, maskBox),
                            Item5 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                mat.texBounds.Min // 2
                            ),
                            Item6 = new MyTuple<Vector2, Vector2, Vector2>
                            (
                                quad.Point0,
                                quad.Point2,
                                quad.Point3
                            ),
                        };

                        bbDataBack.Add(bbL);
                        bbDataBack.Add(bbR);

                        if (indexR >= bbPool.Count)
                            instance.AddNewBB(indexR - (bbPool.Count - 1));

                        MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                        MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                    }
                }

                #endregion

                /// <summary>
                /// Adds the given number of <see cref="MyTriangleBillboard"/>s to the pool
                /// </summary>
                private void AddNewBB(int count)
                {
                    bbPoolBack.EnsureCapacity(bbPoolBack.Count + count);

                    for (int i = 0; i < count; i++)
                    {
                        bbPoolBack.Add(new MyTriangleBillboard
                        {
                            BlendType = BlendTypeEnum.PostPP,
                            Position0 = Vector3D.Zero,
                            Position1 = Vector3D.Zero,
                            Position2 = Vector3D.Zero,
                            UV0 = Vector2.Zero,
                            UV1 = Vector2.Zero,
                            UV2 = Vector2.Zero,
                            Material = Material.Default.TextureID,
                            Color = Vector4.One,
                            DistanceSquared = float.PositiveInfinity,
                            ColorIntensity = 1f,
                            CustomViewProjection = -1
                        });
                    }
                }

                /// <summary>
                /// Converts a color to its normalized linear RGB equivalent. Assumes additive blending
                /// with premultiplied alpha.
                /// </summary>
                public static Vector4 GetBillBoardBoardColor(Color color)
                {
                    float opacity = color.A / 255f;

                    color.R = (byte)(color.R * opacity);
                    color.G = (byte)(color.G * opacity);
                    color.B = (byte)(color.B * opacity);

                    return ((Vector4)color).ToLinearRGB();
                }
            }
        }
    }
}