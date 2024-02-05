using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
namespace SoulWater
{
    internal static class Block
    {
        public static List<BlockReference> GetNameBlock(this List<BlockReference> blockReferences, string name)
        {
            List<BlockReference> blocks = [];
            foreach (BlockReference block in blockReferences)
            {
                if (block.Name == "图名编号")
                {
                    blocks.Add(block);
                }
            }
            return blocks;
        }
    }
    internal static class DBlock
    {



        /// <summary>
        /// 动态块真实块名获取
        /// </summary>
        /// <param name="brRec">块参照</param>
        /// <returns>成功返回:块的真实名称,失败返回:null</returns>
        // 1.块的Z比例是0就会令动态块变成普通块,那么导致判断动态块失效
        // 2.brRec.IsDynamicBlock 如果是动态块这句会报错:eInvalidObjectId
        //   重复空格执行上次报这个错误,应该在所有GetObject位置写.Dispose();
        public static string GetBlockName(this BlockReference brRec)
        {
            string blockName = string.Empty;
            if (brRec.DynamicBlockTableRecord.IsOk())
            {
                // 动态块表记录可以获取 动态块名 也可以获取 普通块名
                var btRec = brRec.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                blockName = btRec.Name;
                btRec.Dispose();
            }
            return blockName;
        }
        /// <summary>
        /// id有效,未被删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsOk(this ObjectId id)
        {
            return !id.IsNull && id.IsValid && !id.IsErased && !id.IsEffectivelyErased && id.IsResident;
        }



        /// <summary>
        /// 更改动态块拉伸参数
        /// </summary>
        /// <param name="ent">动态块参照的图元</param>
        /// <param name="name">动作参数名</param>
        /// <param name="value">动作参数值</param>
        private static void ChangDynamicBlockValue(Entity ent, string name, string valueStr)
        {
            if (ent is BlockReference br && br.IsDynamicBlock)//如果是动态块
            {
                br.UpgradeOpen();
                var props = br.DynamicBlockReferencePropertyCollection; //动态块属性集
                foreach (DynamicBlockReferenceProperty item in props)
                {
                    if (item.PropertyName.Contains(name))//参数名:可见性/距离之类的
                        item.Value = valueStr;//参数值
                }
                br.DowngradeOpen();
            }
        }





        ///https://blog.csdn.net/yxp_xa/article/details/72229202?tdsourcetag=s_pcqq_aiomsg
        /// <summary>
        /// 转义通配符,加入过滤器必须转义
        /// </summary>
        /// <param name="blockName">块名</param>
        /// <returns></returns>
        static string BlockNameEscape(string blockName)
        {
            return blockName.Replace("#", "`#").Replace("@", "`@");
        }

        //https://www.cnblogs.com/edata/p/6797362.html
        /// <summary>
        /// 通过块信息,设置动态块过滤器或普通块过滤器
        /// </summary>
        /// <param name="baseId">块的图元id</param>
        /// <returns>过滤器</returns>
        public static SelectionFilter GetBlockSelectionFilter(this Transaction tr, ObjectId baseId)
        {
            //选择集过滤器
            var list = new List<TypedValue>();
            var brRec = tr.GetObject(baseId, OpenMode.ForRead) as BlockReference;
            string blockName = BlockNameEscape(brRec.GetBlockName());

            list.Add(new TypedValue((int)DxfCode.Start, "INSERT"));  //是块

            //当前布局名称(视口内为模型)
            string layoutName = GetMouseSpace();
            list.Add(new TypedValue((int)DxfCode.LayoutName, layoutName));//是同一个空间 "*Model_Space"

            //动态块过滤器{动态块名,真实名称(匿名块不提供)}
            list.Add(new TypedValue((int)DxfCode.BlockName, $"`*U*,{blockName}"));
            return new SelectionFilter(list.ToArray());
        }
        /// <summary>
        /// 当前布局名称(视口内为模型)
        /// </summary>
        /// <returns>模型或布局名称</returns>
        public static string GetMouseSpace()
        {
            //获得当前布局名称
            string layoutName = LayoutManager.Current.CurrentLayout;//切换当前布局要锁文档
            //if (SpatialPoint() == CadSystem.SpatialPosition.Viewport)
            //    layoutName = "Model";
            return layoutName;
        }
    }
    internal static class Ety
    {

    }
    public  class RoAlDimension
    {
        public Point3d XLine1Point { get; }
        public Point3d XLine2Point { get; }
        public Point3d DimLinePoint { get; }
        public Xline Xl { get; }
        public string XSText { get; }
        public double RText { get; }
        public double Rotation { get; }
        public RoAlDimension(RotatedDimension rotated)
        {
            this.XLine1Point = rotated.XLine1Point;
            this.XLine2Point = rotated.XLine2Point;
            this.DimLinePoint = rotated.DimLinePoint;
            this.Xl = new Xline
            {
                BasePoint = Point.CenterPoint([this.XLine1Point, this.XLine2Point]).Z坐标归零(),
                SecondPoint = Point.CenterPoint([this.XLine1Point, this.DimLinePoint]).Z坐标归零()
            };
            
            this.Xl.SecondPoint = new Point3d(this.Xl.SecondPoint.X, this.Xl.SecondPoint.Y, 0);
            if (!(rotated.DimensionText == ""))
            {
                this.XSText = rotated.DimensionText;
            }
            else
            {
                this.XSText = rotated.Measurement.ToString("0");
            }
            this.RText = Math.Round(rotated.Measurement, 4);
            this.Rotation = this.Xl.UnitDir.GetAngleTo(-Vector3d.XAxis);
        }
        public RoAlDimension(AlignedDimension rotated)
        {
            this.XLine1Point = rotated.XLine1Point;
            this.XLine2Point = rotated.XLine2Point;
            this.DimLinePoint = rotated.DimLinePoint;
            this.Xl = new Xline
            {
                BasePoint = Point.CenterPoint([this.XLine1Point, this.XLine2Point]).Z坐标归零(),
                SecondPoint = Point.CenterPoint([this.XLine1Point, this.DimLinePoint]).Z坐标归零()
            };
            
            if (!(rotated.DimensionText == ""))
            {
                this.XSText = rotated.DimensionText;
            }
            else
            {
                this.XSText = rotated.Measurement.ToString("0");
            }
            this.RText = Math.Round(rotated.Measurement, 4);
            this.Rotation = this.Xl.UnitDir.GetAngleTo(-Vector3d.XAxis);
        }

    }
    internal  class Layer
    {
        public static void AddLayer(string name, Autodesk.AutoCAD.Colors.Color color)
        //添加图层
        {
            {
                Database db = CAD.Db;

                using (Transaction trans = CAD.Tr)
                {
                    LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForWrite);
                    //foreach (ObjectId item in lt)
                    //{
                    //    LayerTableRecord ltr = (LayerTableRecord)item.GetObject(OpenMode.ForRead);
                    //    //遍历查看所有图层
                    //}
                    if (!lt.Has(name))
                    {
                        LayerTableRecord layerTable = new()
                        {
                            Name = name,
                            Color = color,
                            IsFrozen = false,
                            IsOff = false
                        };
                        lt.Add(layerTable);
                        trans.AddNewlyCreatedDBObject(layerTable, true);
                        trans.Commit();
                    }
                }
            }
        }
        public static void SetLayerCurrent(string name)
        {
            // 获取当前文档和数据库
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // 启动事务
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // 以读模式打开图层表
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(name) == true)
                {
                    // 设置图层 Center 为当前图层
                    acCurDb.Clayer = acLyrTbl[name];
                    // 保存修改
                    acTrans.Commit();
                }
                // 关闭事务
            }
        }
        /// <summary>
        /// 判断图层是否存在
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns></returns>
        public bool IsExistLayer(String layerName)
        {
            bool retval = false;

            Transaction trs = CAD.Tr;
            using (trs)
            {
                LayerTable lt = trs.GetObject(CAD.Db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    retval = true;
                }
                trs.Commit();
            }
            return retval;
        }
        /// <summary>
        /// 锁住图层
        /// </summary>
        /// <param name="layerName"></param>
        public void LockLayer(String layerName)
        {
            if (!IsExistLayer(layerName))
            {
                return;
            }

            Transaction trs = CAD.Tr;
            using (trs)
            {
                LayerTable lt = trs.GetObject(CAD.Db.LayerTableId, OpenMode.ForRead) as LayerTable;
                LayerTableRecord ltr = trs.GetObject(lt[layerName], OpenMode.ForWrite) as LayerTableRecord;
                if (!ltr.IsLocked)
                {
                    ltr.IsLocked = true;
                }
                trs.Commit();
            }
        }
        /// <summary>
        /// 删除图层
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public bool EraseLayer(String layerName)
        {
            bool retval = false;
            if (!IsExistLayer(layerName))
            {
                return retval;
            }
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Transaction trs = db.TransactionManager.StartTransaction();
            using (trs)
            {
                LayerTable lt = trs.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;
                LayerTableRecord ltr = trs.GetObject(lt[layerName], OpenMode.ForWrite) as LayerTableRecord;
                lt.GenerateUsageData();
                if ((db.Clayer == lt[layerName]) || ltr.IsUsed
                    || layerName == "0" || layerName == "Defpoints")
                {
                    trs.Commit();
                    return retval;
                }
                ltr.Erase(true);
                retval = true;
                trs.Commit();
            }

            return retval;
        }
        /// <summary>
        /// 获取所有图层名称
        /// </summary>
        /// <returns></returns>
        public List<String> GetAllLayersName()
        {
            List<String> layers = [];
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Transaction trs = db.TransactionManager.StartTransaction();
            using (trs)
            {
                LayerTable lt = trs.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId id in lt)
                {
                    LayerTableRecord ltr = trs.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                    layers.Add(ltr.Name);
                }
                trs.Commit();
            }
            return layers;
        }
    }
    internal static class BOX
    {
        
        public static bool GetBoxs(List<Entity> entitie ,out Tuple<Point3d, Point3d> tuple,out Extents3d extents)
        {
            List<Entity> entities = [.. entitie];
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is BlockReference entity)
                {
                    DBObjectCollection dBObjectCollection = [];
                    entity.Explode(dBObjectCollection);

                    foreach (Entity entity2 in dBObjectCollection)
                    {
                        entities.Add(entity2);
                    }
                }
            }

            double minx = entities.Min(x => x.Bounds.Value.MinPoint.X);
            double miny = entities.Min(x => x.Bounds.Value.MinPoint.Y);
            double maxx = entities.Max(x => x.Bounds.Value.MaxPoint.X);
            double maxy = entities.Max(x => x.Bounds.Value.MaxPoint.Y);

            tuple = new Tuple<Point3d, Point3d>(new Point3d(minx, miny, 0), new Point3d(maxx, maxy, 0));
            extents = new Extents3d(new Point3d(minx, miny, 0), new Point3d(maxx, maxy, 0));
            if (tuple.Item2 == null && tuple.Item1 == null && (extents.MaxPoint == extents.MinPoint)) return false;
            return true;
        }
    }
   
    internal static class Point
    {
        /// <summary>
        /// 判断点是否在box内
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="extents">BOX</param>
        /// <returns></returns>
        public static bool PointInBox(this Point3d point, Extents3d extents)
        {
            double minX = extents.MinPoint.X;
            double maxX = extents.MaxPoint.X;
            double minY = extents.MinPoint.Y;
            double maxY = extents.MaxPoint.Y;

            bool isInBox = minX <= point.X && point.X <= maxX && minY <= point.Y && point.Y <= maxY;
            return isInBox;
        }
        /// <summary>
        /// 通过角度和距离计算点（极坐标计算）
        /// </summary>
        /// <param name="point">起点</param>
        /// <param name="angle">角度</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static Point3d PolarPoint(this Point3d point, double angle, double distance)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            double newX = point.X + distance * cosAngle;
            double newY = point.Y + distance * sinAngle;
            double newZ = point.Z;

            Point3d newPoint = new(newX, newY, newZ);
            return newPoint;
        }
        /// <summary>
        /// 计算点集的中心
        /// </summary>
        /// <param name="point3Ds">点集</param>
        /// <returns>中心点</returns>
        public static Point3d CenterPoint(List<Point3d> point3Ds)
        {
            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;
            int count = point3Ds.Count;

            foreach (Point3d p in point3Ds)
            {
                sumX += p.X;
                sumY += p.Y;
                sumZ += p.Z;
            }

            Point3d point = new(sumX / count, sumY / count, sumZ / count);
            return point;
        }
        /// <summary>
        /// 沿直线计算点
        /// </summary>
        /// <param name="stratpoint">起点</param>
        /// <param name="endpoint">方向点</param>
        /// <param name="lengt">距离</param>
        /// <returns></returns>
        public static Point3d GetPointAlonePolyline(Point3d startPoint, Point3d endPoint, double length)
        {
            Vector3d vector = endPoint - startPoint;
            double vectorLength = vector.Length;

            double ratio = length / vectorLength;
            double newX = startPoint.X + (vector.X * ratio);
            double newY = startPoint.Y + (vector.Y * ratio);
            double newZ = startPoint.Z + (vector.Z * ratio);

            Point3d newPoint = new(newX, newY, newZ);
            return newPoint;
        }
        public static Point3d Z坐标归零(this Point3d point3D)
        {
            return new Point3d(point3D.X, point3D.Y, 0);
        }

    }

    internal static class 曲线
    {
        /// <summary>
        /// 分隔曲线
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point3Ds"></param>
        /// <returns></returns>
        public static List<Curve> 分隔曲线(this Curve curve, List<Point3d> point3Ds)
        {
            List<Curve> curves = [];
            List<Point3d> tmps = [];

            point3Ds.ForEach(p => tmps.Add(curve.GetClosestPointTo(p, false)));
            point3Ds = [.. tmps.OrderBy(p => curve.GetParameterAtPoint(p))];
            Point3dCollection pos = [];
            point3Ds.ForEach(p => pos.Add(curve.GetClosestPointTo(p, false)));
            DBObjectCollection dbs = curve.GetSplitCurves(pos);
            foreach (Curve item in dbs)
            {
                curves.Add(item);
            }
            return curves;
        }
        /// <summary>
        /// 求两个曲线交点
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <param name="intersect">延长关系</param>
        /// <returns></returns>
        public static List<Point3d> 求两个曲线交点(this Curve curve1, Curve curve2, Intersect intersect = Intersect.OnBothOperands)
        {
            // Intersect.OnBothOperands：表示要求在两个实体对象上同时进行相交检测。
            //Intersect.ExtendBoth：表示要求将两个实体对象进行延伸，并检测延伸后的部分是否相交。
            //Intersect.ExtendThis：表示要求将当前实体对象进行延伸，并检测延伸后的部分与另一个实体对象是否相交。
            //Intersect.ExtendOther：表示要求将另一个实体对象进行延伸，并检测延伸后的部分与当前实体对象是否相交。
            Point3dCollection pos = [];
            curve1.IntersectWith(curve2, intersect, pos, IntPtr.Zero, IntPtr.Zero);
            List<Point3d> points = new();
            foreach (Point3d item in pos)
            {
                points.Add(item);
            }
            return points;
        }
        /// <summary>
        /// 获得曲线集合中沿向量方向离点最近的曲线
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="vector">方向向量</param>
        /// <param name="curves">曲线集合</param>
        /// <returns>最近的曲线</returns>
        public static Curve 获得点最近曲线(Point3d point, Vector3d vector, List<Curve> curves)
        {
            Ray ray = new()
            {
                BasePoint = point,
                UnitDir = vector
            };
            //Point3d closetPoint = new Point3d();
            double closestDis = double.MaxValue;
            Curve closestCur = null;
            foreach (Curve curve in curves)
            {
                Point3dCollection pos = [];
                ray.IntersectWith(curve, Intersect.OnBothOperands, pos, IntPtr.Zero, IntPtr.Zero);
                foreach (Point3d po in pos)
                {
                    double dis = po.DistanceTo(point);
                    if (dis < closestDis)
                    {
                        //closetPoint= po;
                        closestDis = dis;
                        closestCur = curve;
                    }
                }
            }
            return closestCur;
        }
    }
    internal class PL
    {
        /// <summary>
        /// 创建多段线通过2d点集
        /// </summary>
        /// <param name="point2Ds">2d点集</param>
        /// <param name="isClosed">是否闭合</param>
        /// <returns>多段线</returns>
        public static Polyline CreatePolyline(List<Point2d> point2Ds, bool isClosed=false)
        {
            Polyline polyline = new Polyline();
            for (int i = 0; i < point2Ds.Count; i++)
            {
                polyline.AddVertexAt(i, point2Ds[i], 0, 0, 0);
            }
            polyline.Closed = isClosed;
            return polyline;
        }
        /// <summary>
        /// 创建多段线通过2d点集
        /// </summary>
        /// <param name="point2Ds">3d点集</param>
        /// <param name="isClosed">是否闭合</param>
        /// <returns></returns>
        public static Polyline CreatePolyline(List<Point3d> point2Ds, bool isClosed = false)
        {
            Polyline polyline = new();
            for (int i = 0; i < point2Ds.Count; i++)
            {
                polyline.AddVertexAt(i, point2Ds[i].Convert2d(new Plane()), 0, 0, 0);
            }
            polyline.Closed = isClosed;
            return polyline;
        }
        /// <summary>
        /// 通过box绘制多段线，多用于绘制box
        /// </summary>
        /// <param name="extents">box集合</param>
        /// <returns></returns>
        public static Polyline CreatePolyline(Extents3d extents)
        {
            Polyline pl = new();
            pl.AddVertexAt(0, new Point2d(extents.MinPoint.X, extents.MinPoint.Y), 0, 0, 0);
            pl.AddVertexAt(1, new Point2d(extents.MaxPoint.X, extents.MinPoint.Y), 0, 0, 0);
            pl.AddVertexAt(2, new Point2d(extents.MaxPoint.X, extents.MaxPoint.Y), 0, 0, 0);
            pl.AddVertexAt(3, new Point2d(extents.MinPoint.X, extents.MaxPoint.Y), 0, 0, 0);
            pl.Closed = true;
            return pl;
        }
        /// <summary>
        /// 获得多段线的所有点
        /// </summary>
        /// <param name="polyline">多段线</param>
        /// <returns>3d点集</returns>
        public static List<Point3d> GetPoint3DsFromPolyLine(Polyline polyline)
        {
            List<Point3d> points = [];
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                points.Add(polyline.GetPointAtParameter(i));
            }
            return points;
        }
    }
}
