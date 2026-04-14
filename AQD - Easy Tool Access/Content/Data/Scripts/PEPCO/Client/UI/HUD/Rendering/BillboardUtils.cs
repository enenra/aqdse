using System.Collections.Generic;
using System;
using System.Threading;
using Sandbox.ModAPI;
using VRage.Game;
using VRage;
using VRage.Utils;
using VRageMath;
using VRageRender;
using RichHudFramework.UI.Rendering;
using RichHudFramework.Client;
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
            using BbUtilData = MyTuple<
                ApiMemberAccessor, // GetOrSetMember
                List<TriangleBillboardData>, // triangleList
                List<FlatTriangleBillboardData>, // flatTriangleList
                List<MatrixD>, // matrixBuf
                Dictionary<MatrixD[], int> // matrixTable
            >;

            public sealed partial class BillBoardUtils : RichHudClient.ApiModule<BbUtilData>
            {
                private static BillBoardUtils instance;

                private readonly List<MyTriangleBillboard> bbBuf;
                private List<MyTriangleBillboard> bbPoolBack;

                private readonly ApiMemberAccessor GetOrSetMember;
                private readonly List<TriangleBillboardData> triangleList;
                private readonly List<FlatTriangleBillboardData> flatTriangleList;
                private readonly List<MatrixD> matrixBuf;
                private readonly Dictionary<MatrixD[], int> matrixTable;

                private BillBoardUtils() : base(ApiModuleTypes.BillBoardUtils, false, true)
                {
                    if (instance != null)
                        throw new Exception($"Only one instance of {GetType().Name} can exist at once.");

                    bbBuf = new List<MyTriangleBillboard>(1000);

                    var data = GetApiData();
                    GetOrSetMember = data.Item1;
                    triangleList = data.Item2;
                    flatTriangleList = data.Item3;
                    matrixBuf = data.Item4;
                    matrixTable = data.Item5;
                }

                public static void Init()
                {
                    if (instance == null)
                    {
                        instance = new BillBoardUtils();
                    }
                }

                public override void Close()
                {
                    if (ExceptionHandler.Unloading)
                    {
                        instance = null;
                    }
                }

                public static void BeginDraw()
                {
                    if (instance != null)
                    {
                        instance.bbPoolBack = instance.GetOrSetMember(null, (int)BillBoardUtilAccessors.GetPoolBack) as List<MyTriangleBillboard>;
                    }
                }

                public static void FinishDraw()
                { }
            }
        }
    }
}