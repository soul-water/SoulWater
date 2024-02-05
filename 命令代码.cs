using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using System.Reflection.Emit;
using System.Security.Principal;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.EditorInput;
using System.Windows;
using System.Windows.Forms;



namespace SoulWater
{
    public class 王博
    {
        internal class CADcommand
        {
            [CommandMethod("CA")]
            public void CA()
            {
                CAD.Ed.Command("LAYMCUR");
            }
            [CommandMethod("XH")]
            public void XH()
            {
                CAD.Ed.Command("XLINE", "H");
            }
            [CommandMethod("XV")]
            public void XV()
            {
                CAD.Ed.Command("XLINE", "V");
            }
            [CommandMethod("XX")]
            public void XX()
            {
                CAD.Ed.Command("XLINE");
            }
            [CommandMethod("FF")]
            public void FF()
            {
                CAD.Ed.Command("PLINE");
            }
            [CommandMethod("CC")]
            public void CC()
            {
                CAD.Ed.Command("CIRCLE");
            }
        }
        [CommandMethod("SWI", CommandFlags.UsePickSet)]
        public void SWI()
        {
            //Wipeout;
            if (!选择.OnlySelectEntities(new Wipeout(), out List<Wipeout> ws)) return;
            CAD.Ed.WriteMessage("选中");
        }
        [CommandMethod("DDW", CommandFlags.UsePickSet)]
        public void DDW()
        {
            Autodesk.AutoCAD.DatabaseServices.Dimension dimension = new DiametricDimension();
            if(!选择.OnlySelectEntities(dimension ,out List<Autodesk.AutoCAD.DatabaseServices.Dimension> dimensions))return;
            if (dimensions.Count == 0) return;
            if (!获取.GetDouble(out Double value, "输入旋转角度", 60)) return;
            using (Transaction tr = CAD.Tr)
            {
                foreach (Autodesk.AutoCAD.DatabaseServices.Dimension dimension1 in dimensions)
                {
                    if(dimension1 is RotatedDimension dimension2)
                    {
                        dimension2.ObjectId.GetObject(OpenMode.ForWrite);
                        dimension2.Oblique = (Math.PI / 180) * value;
                    }
                    if (dimension1 is AlignedDimension dimension3)
                    {
                        dimension3.ObjectId.GetObject(OpenMode.ForWrite);
                        dimension3.Oblique = (Math.PI / 180) * value;
                    }
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 根据距离和点获取曲面两边的点
        /// </summary>
        [CommandMethod("OD",CommandFlags.UsePickSet)]
        public void OD()
        {
            if (!获取.GetEntity("选择曲线", out Curve entity)) return;
            if (!获取.GetPoint(out Point3d point)) return;
            Point3d pt1 = entity.GetClosestPointTo(point, false);
            if (pt1.DistanceTo(point) >= 1)
            {
                CAD.Ed.WriteMessage("点不在曲线上");
                return;
            }
            if (!获取.GetDouble(out double d,"输入偏移距离")) return;
            
            
            double leng = entity.GetDistAtPoint(entity.EndPoint);
            double length = entity.GetDistAtPoint(pt1);
            List<Point3d> points = [];
            if ((length + d) < leng && (length - d) > 0)
            {
                Point3d point3D = entity.GetPointAtDist(length + d);
                Point3d point3D2 = entity.GetPointAtDist(length - d);
                points.Add(point3D);
                points.Add(point3D2);
            }else if ((length + d) >= leng && (length - d) > 0)
            {
                
                Point3d point3D2 = entity.GetPointAtDist(length - d);
                
                points.Add(point3D2);
            }
            else if ((length + d) < leng && (length - d) <= 0)
            {
                Point3d point3D = entity.GetPointAtDist(length + d);
                
                points.Add(point3D);
            }
            else
            {
                CAD.Ed.WriteMessage("超出曲线");
            }
            List<Circle> circles = [];
            foreach (Point3d point3d in points)
            {
                double 比例 = 系统变量.当标注比例;
                if (比例 == 0) 比例 = 1;
                Circle circle = new()
                {
                    Center = point3d,
                    Radius = 比例 * 3
                };
                circles.Add(circle);
            }
            circles.AddEntityToModeSpace();
        }
        /// <summary>
        /// 将多段线的闭合属性改为是
        /// </summary>
        [CommandMethod("ASA")]
        public void ASA()
        {
            if (!选择.SelectEntities(out List<Polyline> polyline)) return;

            for (int i = 0; i < polyline.Count; i++)
            {
                Curve line1 = polyline[i];
                using (Transaction trans = CAD.Tr)
                {
                    Polyline polyline1 = (Polyline)line1.ObjectId.GetObject(OpenMode.ForWrite);
                    polyline1.Closed = true;
                    trans.Commit();
                }
            }
        }
        /// <summary>
        /// 框选线的顶点进行连接
        /// </summary>
        [CommandMethod("ASD")]
        public void ASD()
        {

            if (!选择.GetEntityAndBounds<Curve>("请框选线", out List<Curve> curves, out List<Extents3d> extents)) return;

            List<Point2d> points = new List<Point2d>();
            if (curves.Count <= 1) return;
            for (int i = 0; i < extents.Count; i++)
            {
                Point3d startpoint = curves[i].StartPoint;
                Point3d endpoint = curves[i].EndPoint;
                if (extents[i].MinPoint.X < startpoint.X && startpoint.X < extents[i].MaxPoint.X && extents[i].MinPoint.Y < startpoint.Y && startpoint.Y < extents[i].MaxPoint.Y)
                {
                    points.Add(startpoint.Convert2d(new Plane()));
                }
                else points.Add(endpoint.Convert2d(new Plane()));
            }

            Polyline poly = PL.CreatePolyline(points, false);
            poly.AddEntityToModeSpace();
        }
        /// <summary>
        /// 多段线添加或删除点
        /// </summary>
        [CommandMethod("Cpt")]
        public void Cpt()
        {
            if (!获取.GetEntity("请选择一个多段线", out Polyline polyline)) return;
            CAD.Ed.SetImpliedSelection(new ObjectId[] { polyline.Id });
            CAD.Ed.Regen();
            //return;
            while (true)
            {
                if (获取.GetPoint(out Point3d point, "\n请选择点"))
                {
                    Point3d po = polyline.GetClosestPointTo(point, false);
                    bool online = false;
                    if (po.DistanceTo(point) < 0.00001)
                    {
                        online = true;
                    }
                    double p = polyline.GetParameterAtPoint(po);
                    using (Transaction tr = CAD.Tr)
                    {
                        polyline.ObjectId.GetObject(OpenMode.ForWrite);
                        if (Math.Floor(p) == p)
                        {
                            int num = (int)p;
                            if (online)
                            {
                                polyline.RemoveVertexAt(num);
                            }
                            else
                            {
                                polyline.AddVertexAt(num == 0 ? 0 : num + 1, point.Convert2d(new Plane()), 0, 0, 0);
                            }
                        }
                        else
                        {
                            polyline.AddVertexAt((int)Math.Ceiling(p), point.Convert2d(new Plane()), 0, 0, 0);
                        }
                        tr.Commit();
                    }
                }
                else { break; }
            }
        }
        /// <summary>
        /// 生成框
        /// </summary>
        [CommandMethod("DLL")]
        public void DLL()
        {
            Layer.AddLayer("打印矩形图框", Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0));
            if (!选择.SelectEntities(out List<Entity> entities, "选择图块")) return;
            Layer.SetLayerCurrent("打印矩形图框");
            foreach (Entity entity in entities)
            {
                 BOX.GetBoxs(new List<Entity> { entity },out Tuple<Point3d, Point3d> tuple,out _);
                Point2d minpo = new Point2d(tuple.Item1.X, tuple.Item1.Y);
                Point2d maxpo = new Point2d(tuple.Item2.X, tuple.Item2.Y);
                List<Point2d> points = new List<Point2d>
                {
                    minpo,
                    new Point2d(maxpo.X,minpo.Y),
                    maxpo,
                    new Point2d(minpo.X,maxpo.Y)
                };
                Polyline polyline = PL.CreatePolyline(points, true);
                polyline.Layer = "打印矩形图框";
                添加.AddEntityToModeSpace(polyline);
            }
        }
        /// <summary>
        /// 框选多段线中的点去倒角
        /// </summary>
        [CommandMethod("DJJ")]
        public void DJJ()
        {
            if (!选择.GetEntityAndBounds("框选多段线", out List<Polyline> pls, out List<Extents3d> extents3Ds))
            {
                return;
            }
            if (!获取.GetDouble(out double lengt)) return;
            for (int i = 0; i < pls.Count; i++)
            {
                for (int j = 0; j < pls[i].NumberOfVertices; j++)
                {
                    if (Point.PointInBox(pls[i].GetPoint3dAt(j), extents3Ds[i]))
                    {
                        Point3d point3D = Point.GetPointAlonePolyline(pls[i].GetPoint3dAt(j), pls[i].GetPoint3dAt(j - 1), lengt);
                        Point3d point3D1 = Point.GetPointAlonePolyline(pls[i].GetPoint3dAt(j), pls[i].GetPoint3dAt(j + 1), lengt);
                        using (Transaction transaction = CAD.Tr)
                        {
                            pls[i].ObjectId.GetObject(OpenMode.ForWrite);
                            pls[i].AddVertexAt(j, point3D.Convert2d(new Plane()), 0, 0, 0);
                            pls[i].AddVertexAt(j + 2, point3D1.Convert2d(new Plane()), 0, 0, 0);
                            pls[i].RemoveVertexAt(j + 1);
                            transaction.Commit();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 通过圆绘制五角星
        /// </summary>
        [CommandMethod("WJX")]
        public void WJX()
        {
            if (!获取.GetEntity("选择一个圆", out Circle circle)) return;
            Point3d center = circle.Center;
            double radius = circle.Radius;
            Point3d pt1 = Point.PolarPoint(center, 162 * (Math.PI / 180), radius);
            Point3d pt2 = Point.PolarPoint(center, 18 * (Math.PI / 180), radius);
            Line line = new Line(pt1, pt2);
            List<Line> lines = new List<Line>
            {
                line
            };
            for (int i = 1; i < 5; i++)
            {
                Matrix3d matrix3D1 = Matrix3d.Rotation(i * 72 * (Math.PI / 180), Vector3d.ZAxis, center);
                Line line1 = line.Clone() as Line;
                line1.TransformBy(matrix3D1);
                lines.Add(line1);
            }
            //BlockReference block=
            lines.AddEntityToModeSpace();
            //AddEntity.AddEntities(lines);
        }
        /// <summary>
        /// 修改标注比例
        /// </summary>
        [CommandMethod("DSC")]
        public void DSC()
        {
            //object oldFileDia = Application.GetSystemVariable("DIMSCALE");
            int olddscvalue = System.Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("DIMSCALE"));
            if (!获取.GetInt(out int newdscvalue, "请输入新的DIMSCALE值", olddscvalue)) return;
            Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("DIMSCALE", newdscvalue);
        }
        /// <summary>
        /// 生成单行文字
        /// </summary>
        [CommandMethod("BB")]
        public void BB()
        {
            int dscvalue = System.Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("DIMSCALE"));
            if (dscvalue == 0)
            {
                dscvalue = 1;
            }
            if (!获取.GetPoint(out Point3d point, "请输入一个点")) return;
            DBText dBText = new DBText
            {
                TextString = "单行文字",
                Height = dscvalue * 2,
                Position = point
            };
            dBText.AddEntityToModeSpace(-1);
        }
        /// <summary>
        /// 插入多行文字
        /// </summary>
        [CommandMethod("BBB")]
        public void BBB()
        {
            int dscvalue = 系统变量.当标注比例;
            if (dscvalue == 0)
            {
                dscvalue = 1;
            }
            if (!获取.GetPoint(out Point3d point, "请输入一个点")) return;
            MText mText = new MText()
            {
                Contents = "多行文字\n多行文字",
                Location = point,
                TextHeight = dscvalue,
                Width = dscvalue * 6
            };
            mText.AddEntityToModeSpace(-1);
        }
        /// <summary>
        /// 将第一条线复制到两条线的中点
        /// </summary>
        [CommandMethod("XXM")]
        public void XXM()
        {
            if (!获取.GetEntity("选择第一条线", out Curve curve1)) return;
            if (!获取.GetEntity("选择第二条线", out Curve curve2)) return;
            Point3d point1 = Point.CenterPoint(new List<Point3d>() { curve1.StartPoint, curve1.EndPoint });
            Point3d point2 = Point.CenterPoint(new List<Point3d>() { curve2.StartPoint, curve2.EndPoint });
            Point3d point3 = Point.CenterPoint(new List<Point3d>() { point1, point2 });
            Vector3d vector3D = point3 - point1;
            Matrix3d matrix3D = Matrix3d.Displacement(vector3D);
            Curve curve3 = curve1.Clone() as Curve;
            curve3.TransformBy(matrix3D);
            curve3.AddEntityToModeSpace();
        }
        /// <summary>
        /// 添加文字下划线
        /// </summary>
        [CommandMethod("WX")]
        public void WX()
        {
            if (!选择.SelectEntities(out List<DBText> lists, "请选择文字")) return;
            Layer.AddLayer("M_Dim", Autodesk.AutoCAD.Colors.Color.FromRgb(0, 255, 0));
            List<Curve> curves = new List<Curve>();
            foreach (DBText text in lists)
            {
               BOX.GetBoxs(new List<Entity> { text },out Tuple<Point3d, Point3d> box1,out _ );
                double h = text.Height / 2.0;
                Point3d point1 = new Point3d(box1.Item1.X - 0.2 * h, box1.Item1.Y - h * (2.0 / 3.0), 0);
                Point3d point2 = new Point3d(box1.Item2.X + 0.2 * h, box1.Item1.Y - h * (2.0 / 3.0), 0);
                Line line1 = new Line(point1, point2)
                {
                    Layer = "M_Dim"
                };
                Point3d point3 = new Point3d(box1.Item1.X - 0.2 * h, box1.Item1.Y - h * (1.0 / 3.0), 0);
                Point3d point4 = new Point3d(box1.Item2.X + 0.2 * h, box1.Item1.Y - h * (1.0 / 3.0), 0);
                Polyline polyline = PL.CreatePolyline(new List<Point2d>() { point3.Convert2d(new Plane()), point4.Convert2d(new Plane()) }, false);
                polyline.ConstantWidth = h * (1.0 / 6.0);
                polyline.Layer = "M_Dim";
                curves.Add(polyline);
                curves.Add(line1);
            }
            curves.AddEntityToModeSpace();
        }
        /// <summary>
        /// 添加文字一条下划线
        /// </summary>
        [CommandMethod("WWX")]
        public void WWX()
        {
            if (!选择.SelectEntities(out List<DBText> lists, "请选择文字")) return;
            Layer.AddLayer("M_Dim", Autodesk.AutoCAD.Colors.Color.FromRgb(0, 255, 0));
            List<Curve> curves = new List<Curve>();
            foreach (DBText text in lists)
            {
                 BOX.GetBoxs(new List<Entity> { text },out Tuple<Point3d, Point3d> box1,out _ );
                double h = text.Height / 2.0;
                Point3d point1 = new Point3d(box1.Item1.X - 0.2 * h, box1.Item1.Y - h * (2.0 / 3.0), 0);
                Point3d point2 = new Point3d(box1.Item2.X + 0.2 * h, box1.Item1.Y - h * (2.0 / 3.0), 0);
                Line line1 = new Line(point1, point2)
                {
                    Layer = "M_Dim"
                };

                curves.Add(line1);
            }
            curves.AddEntityToModeSpace();
        }
        /// <summary>
        /// 修改单行文字文字对齐方式为左对齐
        /// </summary>
        [CommandMethod("DZ")]
        public void DZ()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> text);
            if (text.Count == 0) return;
            foreach (DBText text2 in text)
            {
                using (Transaction transaction = CAD.Tr)
                {
                    text2.ObjectId.GetObject(OpenMode.ForWrite);
                    Point3d point3D = text2.Position;
                    text2.Justify = AttachmentPoint.BaseLeft;
                    //text2.AlignmentPoint = point3D;
                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 修改文字对齐方式为右对齐
        /// </summary>
        [CommandMethod("DZR")]
        public void DZR()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> text);
            if (text.Count == 0) return;
            foreach (DBText text2 in text)
            {
                using (Transaction transaction = CAD.Tr)
                {
                    text2.ObjectId.GetObject(OpenMode.ForWrite);
                    BOX.GetBoxs(new List<Entity>() { text2 }, out Tuple<Point3d, Point3d> tumpl3, out _);
                    Point3d point3D = Point.CenterPoint(new List<Point3d> { tumpl3.Item1, tumpl3.Item2 });
                    text2.Justify = AttachmentPoint.BaseRight;
                    text2.AlignmentPoint = point3D;
                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 修改文字对齐方式为居中对齐
        /// </summary>
        [CommandMethod("DZZ")]
        public void DZZ()
        {
            DBText dBText = new DBText();
           选择.OnlySelectEntities(dBText,out List<DBText> text);
            if (text.Count == 0) return;
            foreach (DBText text2 in text)
            {
                using (Transaction transaction = CAD.Tr)
                {
                    text2.ObjectId.GetObject(OpenMode.ForWrite);
                    BOX.GetBoxs(new List<Entity>() { text2 }, out Tuple<Point3d, Point3d> tumpl3, out _);
                    Point3d point3D = Point.CenterPoint(new List<Point3d> { tumpl3.Item1, tumpl3.Item2 });
                    
                    text2.Justify = AttachmentPoint.MiddleCenter;
                    text2.AlignmentPoint = point3D;
                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 单行文求和
        /// </summary>
        [CommandMethod("QH")]
        public void QH()
        {
            DBText dBText = new DBText();
             选择.OnlySelectEntities(dBText,out List<DBText> texts);
            if (texts.Count == 0) return;
            double he = 0;
            foreach (DBText text2 in texts)
            {
                double result = 0;
                try
                {
                    result = double.Parse(text2.TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());
                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{text2.TextString}'不是数字");
                }
                he += result;
            }
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;
            DBText dBText1 = new DBText()
            {
                TextString = he.ToString(),
                Height = texts[0].Height,
                Position = point,
            };
            dBText1.AddEntityToModeSpace();
        }
        /// <summary>
        /// 单行文求和(询问保留位数)
        /// </summary>
        [CommandMethod("QHH")]
        public void QHH()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> texts);
            if (texts.Count == 0) return;
            double he = 0;
            foreach (DBText text2 in texts)
            {
                double result = 0;
                try
                {
                    result = double.Parse(text2.TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());
                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{text2.TextString}'不是数字");
                }
                he += result;
            }
            if (!获取.GetInt(out int loor, "请输入要保留的位数")) return;
            if (!获取.GetPoint(out Point3d point, "请输入一个点")) return;
            he = Math.Round(he, loor);
            DBText dBText1 = new DBText()
            {
                TextString = he.ToString(),
                Height = texts[0].Height,
                Position = point,
            };
            dBText1.AddEntityToModeSpace();
        }
        /// <summary>
        /// 单行文字所有数字求积
        /// </summary>
        [CommandMethod("QJ")]
        public void QJ()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> texts);
            if (texts.Count == 0) return;
            double he = 1;
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;

            foreach (DBText text2 in texts)
            {
                double result = 1;
                try
                {
                    result = double.Parse(text2.TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());

                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{text2.TextString}'不是数字");
                }
                he *= result;
            }
            DBText dBText1 = new DBText()
            {
                TextString = he.ToString(),
                Height = texts[0].Height,
                Position = point,
            };
            dBText1.AddEntityToModeSpace();
        }
        /// <summary>
        /// 单行文字所有数字求积(询问保留位数)
        /// </summary>
        [CommandMethod("QJJ")]
        public void QJJ()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText ,out List<DBText> texts);
            if (texts.Count == 0) return;
            double he = 1;
            foreach (DBText text2 in texts)
            {
                double result = 1;
                try
                {
                    result = double.Parse(text2.TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());
                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{text2.TextString}'不是数字");
                }
                he *= result;
            }
            if (!获取.GetInt(out int loor, "请输入要保留的位数")) return;
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;
            he = Math.Round(he, loor);
            DBText dBText1 = new DBText()
            {
                TextString = he.ToString(),
                Height = texts[0].Height,
                Position = point,
            };
            dBText1.AddEntityToModeSpace();
        }
        /// <summary>
        /// 文字根据对齐点横向对齐
        /// </summary>
        [CommandMethod("DQQ")]
        public void DQQ()
        {
            DBText dBText = new DBText();
             选择.OnlySelectEntities(dBText,out List<DBText> dBTexts);
            if (dBTexts.Count == 0) return;
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;
            using (Transaction tr = CAD.Tr)
            {
                foreach (DBText dBText1 in dBTexts)
                {
                    dBText1.ObjectId.GetObject(OpenMode.ForWrite);
                    if (dBText1.Justify == AttachmentPoint.BaseLeft)
                    {
                        dBText1.Position = new Point3d(point.X, dBText1.Position.Y, dBText1.Position.Z);
                    }
                    else
                    {
                        dBText1.AlignmentPoint = new Point3d(point.X, dBText1.Position.Y, dBText1.Position.Z);
                    }
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 文字根据对齐点纵向对齐
        /// </summary>
        [CommandMethod("DFF")]
        public void DFF()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> dBTexts);
            if (dBTexts.Count == 0) return;
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点"))
                return;
            using (Transaction tr = CAD.Tr)
            {
                foreach (DBText dBText1 in dBTexts)
                {
                    dBText1.ObjectId.GetObject(OpenMode.ForWrite);
                    if (dBText1.Justify == AttachmentPoint.BaseLeft)
                    {
                        dBText1.Position = new Point3d(dBText1.Position.X, point.Y, dBText1.Position.Z);
                    }
                    else
                    {
                        dBText1.AlignmentPoint = new Point3d(dBText1.Position.X, point.Y, dBText1.Position.Z);
                    }
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 根据源对象刷新对象
        /// </summary>
        [CommandMethod("MAA")]
        public void MAA()
        {
            if (!获取.GetEntity("选择源对象", out Entity entity)) return;
            选择.OnlySelectEntities(entity,out List<Entity> entities);
            if (entities.Count == 0) return;
            foreach (Entity entity1 in entities)
            {
                using (Transaction tr = CAD.Tr)
                {
                    entity1.ObjectId.GetObject(OpenMode.ForWrite);
                    entity1.Layer = entity.Layer;
                    entity1.Color = entity.Color;
                    entity1.Linetype = entity.Linetype;
                    entity1.LinetypeScale = entity.LinetypeScale;
                    entity1.LineWeight = entity.LineWeight;
                    entity1.Transparency = entity.Transparency;

                    //entity1.EdgeStyleId
                    tr.Commit();
                }
            }
        }
        /// <summary>
        /// 根据标注比例两倍的文字高度修改文字
        /// </summary>
        [CommandMethod("WSS")]
        public void WSS()
        {
            int oldvalue = 系统变量.当标注比例;
            if (!获取.GetInt(out int height, "请输入标注比例", oldvalue)) return;
            if (height == 0) height = 1;
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> dBTexts);
            if (dBTexts.Count == 0) { return; }
            using (Transaction tr = CAD.Tr)
            {
                foreach (DBText text in dBTexts)
                {
                    text.ObjectId.GetObject(OpenMode.ForWrite);
                    text.Height = height * 2;
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 中心点求面积
        /// </summary>
        [CommandMethod("WEM")]
        public void WEM()
        {
            Polyline polyline = new Polyline();
            选择.OnlySelectEntities(polyline,out List<Polyline> entities);
            entities = entities.FindAll(x => x is Curve && (x as Curve).Closed);
            if (entities.Count == 0) return;
            foreach (var item in entities)
            {
                Curve curve = item as Curve;
                string area = curve.Area.ToString("0.00");
                Point3d minpo = curve.Bounds.Value.MinPoint;
                Point3d maxpo = curve.Bounds.Value.MaxPoint;
                double height = maxpo.Y - minpo.Y;
                Point3d midpo = new Point3d((minpo.X + maxpo.X) / 2, (minpo.Y + maxpo.Y) / 2, 0);
                DBText dBText = new DBText()
                {
                    TextString = area,
                    HorizontalMode = TextHorizontalMode.TextMid,
                    AlignmentPoint = midpo,
                    Height = height / 5,
                };
                添加.AddEntityToModeSpace(dBText);
            }

        }
        /// <summary>
        /// 矩形长乘宽
        /// </summary>
        [CommandMethod("WET")]
        public void WET()
        {
            Polyline polygon = new Polyline();
            选择.OnlySelectEntities(polygon,out List<Polyline> pls);
            pls = pls.FindAll(x => x.NumberOfVertices == 4);
            if (pls.Count == 0) return;
            List<DBText> texts = new List<DBText>();
            foreach (Polyline poly in pls)
            {
                Point3d point3D = poly.Bounds.Value.MinPoint;
                Point3d point3D1 = poly.Bounds.Value.MaxPoint;
                double l = point3D1.X - point3D.X;
                double l1 = point3D1.Y - point3D.Y;
                string text = l.ToString("0") + "*" + l1.ToString("0");
                Point3d pt3 = Point.CenterPoint(new List<Point3d> { point3D, point3D1 });
                DBText dBText = new DBText()
                {
                    TextString = text,
                    Height = (point3D1.Y - point3D.Y) / 5,
                    Justify = AttachmentPoint.MiddleCenter,
                    AlignmentPoint = pt3
                };
                texts.Add(dBText);
            }
            texts.AddEntityToModeSpace();
        }
        /// <summary>
        /// 批量求积
        /// </summary>
        [CommandMethod("PQJ")]
        public void PQJ()
        {

            if (!选择.SelectEntities(out List<DBText> texts0, "选择第一组数据")) return;
            if (!选择.SelectEntities(out List<DBText> texts1, "选择第二组数据")) return;
            if (!选择.SelectEntities(out List<DBText> texts2, "选择第三组数据")) return;
            if (!获取.GetInt(out int length, "输入第一精度", -1)) return;
            if (!获取.GetInt(out int length1, "输入第二精度", length)) return;
            texts0 = texts0.OrderBy(x => x.Position.Y).ThenBy(x => x).ToList();
            texts1 = texts1.OrderBy(x => x.Position.Y).ThenBy(x => x).ToList();
            texts2 = texts2.OrderBy(x => x.Position.Y).ThenBy(x => x).ToList();
            List<double> r0s = new List<double>();
            List<double> r1s = new List<double>();
            for (int i = 0; i < texts0.Count; i++)
            {
                try
                {
                    double result = 1;
                    double result1 = 1;
                    result = double.Parse(texts0[i].TextString) * double.Parse(texts1[i].TextString);
                    result1 = double.Parse(texts0[i].TextString) * double.Parse(texts1[i].TextString) * double.Parse(texts2[i].TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());
                    if (length != -1)
                    {
                        result = Math.Round(result, length);
                    }
                    if (length1 != -1)
                    {
                        result1 = Math.Round(result1, length1);
                    }
                    r0s.Add(result);
                    r1s.Add(result1);
                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{texts0[i].TextString}' or'{texts1[i].TextString}' 对应数据有错误");
                }
            }
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;
            if (!获取.GetPoint(out Point3d point1, "\n请输入一个点")) return;
            List<DBText> dBTexts = new List<DBText>();
            for (int i = 0; i < texts0.Count; i++)
            {
                Point3d point3D = new Point3d(point.X, texts0[i].Position.Y, texts0[i].Position.Z);
                Point3d point3D1 = new Point3d(point1.X, texts0[i].Position.Y, texts0[i].Position.Z);
                DBText dBText = new DBText()
                {
                    Position = point3D,
                    TextString = r0s[i].ToString(),
                    Height = texts0[i].Height
                };
                DBText dBText1 = new DBText()
                {
                    Position = point3D1,
                    TextString = r1s[i].ToString(),
                    Height = texts0[i].Height
                };
                dBTexts.Add(dBText);
                dBTexts.Add(dBText1);
            }
            dBTexts.AddEntityToModeSpace();
        }
        /// <summary>
        /// 多段线添加文字
        /// </summary>
        [CommandMethod("PLT")]
        public void PLT()
        {
            if (!选择.SelectEntities(out List<Polyline> polylines)) return;
            List<DBText> dBTexts = new List<DBText>();
            foreach (Polyline polyline in polylines)
            {
                Point3d point3D1 = polyline.StartPoint;
                Point3d point3D2 = polyline.EndPoint;
                Point3d point3D3 = Point.CenterPoint(new List<Point3d> { point3D1, point3D2 });
                Vector3d vector3D = polyline.EndPoint - polyline.StartPoint;
                Vector3d vector3D1 = polyline.Normal;
                Vector3d vector3D2 = 向量.CrossNormal(vector3D, vector3D1).GetNormal() * (polyline.ConstantWidth * 2 + 系统变量.当标注比例 * 2);
                Matrix3d matrix3D = Matrix3d.Displacement(vector3D2);
                Point3d point3D4 = point3D3.TransformBy(matrix3D);
                double angle = vector3D.AngleOnPlane(new Plane());
                DBText dBText = new DBText()
                {
                    //Position = point3D,
                    Justify = AttachmentPoint.BaseCenter,
                    AlignmentPoint = point3D4,
                    TextString = "250LG,L=" + polyline.Length.ToString("0"),
                    Height = 系统变量.当标注比例 * 2,
                    Rotation = angle,
                };
                dBTexts.Add(dBText);
            }
            dBTexts.AddEntityToModeSpace();
        }
       
        /// <summary>
        /// 文字编号排序
        /// </summary>
        [CommandMethod("TFF")]
        public void TFF()
        {
            if (!获取.GetString(out string str, "输入前缀")) return;
            if (!获取.GetInt(out int value, "输入起始编号")) return;
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> dBTexts);
            if (dBTexts.Count == 0) return;
            Dictionary<string, string> keys = new Dictionary<string, string>();
            int i = value;
            foreach (DBText dBText1 in dBTexts)
            {
                if (!keys.ContainsKey(dBText1.TextString))
                {
                    if (i < 10) { keys.Add(dBText1.TextString, str + "-" + "0" + i + "~" + dBText1.TextString); }
                    else { keys.Add(dBText1.TextString, str + "-" + i + "~" + dBText1.TextString); }

                    i++;
                }
            }
            using (Transaction tr = CAD.Tr)
            {
                foreach (DBText dBText1 in dBTexts)
                {
                    dBText1.ObjectId.GetObject(OpenMode.ForWrite);
                    if (keys.ContainsKey(dBText1.TextString))
                    {
                        dBText1.TextString = keys[dBText1.TextString];
                    }
                }
                tr.Commit();
            }
        }
        /// <summary>
        /// 单行文字合并为多行文字，排序方式为对齐点坐标y值由大到小
        /// </summary>
        [CommandMethod("BBH")]
        public void BBH()
        {
            DBText dBText = new DBText();
            选择.OnlySelectEntities(dBText,out List<DBText> texts);
            if (texts.Count == 0) return;
            string str = null;
            texts = texts.OrderBy(x => x.Position.Y).ToList();
            texts.Reverse();
            BOX.GetBoxs(texts.ConvertAll(t => (Entity)t),out Tuple<Point3d, Point3d> box,out _);
            Point3d point3D = new Point3d(box.Item1.X, box.Item2.Y, box.Item1.Z);
            foreach (DBText dBText1 in texts)
            {
                str = str + dBText1.TextString + "\n";
            }
            MText mText = new MText
            {
                Contents = str,
                Layer = texts[0].Layer,
                Location = point3D,
                TextStyleId = texts[0].TextStyleId,
                TextHeight = texts[0].Height,
            };
            mText.AddEntityToModeSpace();
            texts.DeleateRntity();
        }
        /// <summary>
        /// 建立新图层并置为当前
        /// </summary>
        [CommandMethod("TG")]
        public static void TG()
        {
            if (!获取.GetString(out string str, "图层名：")) return;
            Layer.AddLayer(str, Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByColor, 7));
            Layer.SetLayerCurrent(str);
        }
        /// <summary>
        /// 将文字布到图元包围盒中心
        /// </summary>
        [CommandMethod("ZXB")]
        public static void ZXB()
        {
            if (!获取.GetEntity("选择文字", out DBText dBText)) return;
            if (!获取.GetEntity("选择求中心的图元", out Entity ent)) return;
            BOX.GetBoxs(new List<Entity> { ent },out Tuple<Point3d, Point3d> tuple,out   _);
            using (Transaction tr = CAD.Tr)
            {
                dBText.ObjectId.GetObject(OpenMode.ForWrite);
                dBText.Justify = AttachmentPoint.MiddleCenter;
                dBText.AlignmentPoint = Point.CenterPoint(new List<Point3d>() { tuple.Item1, tuple.Item2 });
                tr.Commit();
            }
        }
        /// <summary>
        /// 单行文字合并为单行文字
        /// </summary>
        [CommandMethod("BH")]
        public void BH()
        {
            DBText dBText = new DBText();
             选择.OnlySelectEntities(dBText,out List<DBText> texts);
            if (texts.Count == 0) return;
            string str = null;
            texts = texts.OrderBy(x => x.Position.X).ToList();
            //texts.Reverse();
            BOX.GetBoxs(texts.ConvertAll(t => (Entity)t) ,out Tuple<Point3d, Point3d> box,out _);
            Point3d point3D = texts[0].Position;
            foreach (DBText dBText1 in texts)
            {
                str += dBText1.TextString;
            }
            DBText mText = new DBText
            {
                TextString = str,
                Layer = texts[0].Layer,
                Position = point3D,
                TextStyleId = texts[0].TextStyleId,
                Height = texts[0].Height,
            };
            mText.AddEntityToModeSpace();
            texts.DeleateRntity();
        }
        /// <summary>
        /// 图层置为当前
        /// </summary>
        [CommandMethod("TC")]
        public static void TC()
        {
            if (获取.GetEntity("选择图元", out Entity entity)) return;
            Layer.SetLayerCurrent(entity.Layer);
        }
        [CommandMethod("CC0")]
        public static void CC0()
        {
            Editor ed = CAD.Ed;
            ed.Command("Circle", Point3d.Origin, 10);
            Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);
            视图.ViewEntity(circle, 1.2);
        }
        
        /// <summary>
        /// 刷标注图层
        /// </summary>
        [CommandMethod("GX")]
        public static void GX()
        {

            Autodesk.AutoCAD.DatabaseServices.Dimension dimension = new DiametricDimension();
           选择.OnlySelectEntities(dimension,out List<Autodesk.AutoCAD.DatabaseServices.Dimension> dimensions);
            if (dimensions.Count == 0) return;
            using (Transaction tr = CAD.Tr)
            {
                foreach (Autodesk.AutoCAD.DatabaseServices.Dimension dimension1 in dimensions)
                {
                    dimension1.ObjectId.GetObject(OpenMode.ForWrite);
                    dimension1.Layer = 系统变量.当前图层;
                    dimension1.DimensionStyleName = 系统变量.当前标注样式;
                    dimension1.TextStyleId = CAD.Db.Textstyle;
                }
                tr.Commit();
            }
            String NAME = 系统变量.当前图层;
            CAD.Ed.WriteMessage(NAME);

        }
        /// <summary>
        /// 刷文字图层
        /// </summary>
        [CommandMethod("WH")]
        public static void WH()
        {

            List<Entity> entities = new List<Entity>
            {
                new DBText(),
                new MText()
            };
             选择.OnlySelectEntities(entities,out List<Entity> dimensions);
            if (dimensions.Count == 0) return;
            using (Transaction tr = CAD.Tr)
            {
                foreach (Entity dimension1 in dimensions)
                {
                    dimension1.ObjectId.GetObject(OpenMode.ForWrite);
                    dimension1.Layer = 系统变量.当前图层;
                    if (dimension1 is DBText text1)
                    {
                        text1.TextStyleId = CAD.Db.Textstyle;
                    }
                    if (dimension1 is MText text)
                    {
                        text.TextStyleId = CAD.Db.Textstyle;
                    }
                }
                tr.Commit();
            }
            String NAME = 系统变量.当前图层;
            CAD.Ed.WriteMessage(NAME);
        }
        /// <summary>
        /// 曲线等分
        /// </summary>
        [CommandMethod("DFX")]
        public void DFX()
        {

            //if (!获取.GetEntity("选择曲线", out Curve entity)) return;
        }
        /// <summary>
        /// 左右偏移线
        /// </summary>
        [CommandMethod("OO")]
        public void OO()
        {
            if (!获取.GetEntity("1", out Curve entity)) return;
            if (!获取.GetDouble(out double value)) return;
            DBObjectCollection curves = entity.GetOffsetCurves(value);
            DBObjectCollection curves2 = entity.GetOffsetCurves(-value);
            List<Curve> curves1 = new List<Curve>();
            foreach (Curve curve in curves)
            {
                curves1.Add(curve);
            }
            foreach (Curve curve in curves2)
            {
                curves1.Add(curve);
            }
            curves1.AddEntityToModeSpace();
        }

        [CommandMethod("QQQ")]
        public void QQQ()
        {
            String S = 系统变量.当前文字样式.ToString();
            CAD.Ed.WriteMessage(S);

            //Dimension.SetDimStyleCurrent()
        }
    }
    public class 测试
    {
        [CommandMethod("Tex1",CommandFlags.Redraw| CommandFlags.UsePickSet)]
        public static void Tex1()
        {
            if(!获取.GetEntity("1",out BlockReference block))return;

            Transaction transaction = CAD.Tr;
            var ss= transaction.GetBlockSelectionFilter(block.Id);

            Editor editor = CAD.Ed;
            //PromptSelectionOptions promptSelectionOptions = new()
            //{
            //    MessageForAdding = "\n"
            //};
            
            PromptSelectionResult selection = editor.GetSelection(ss);
            SelectionSet selectionSet = selection.Value;
            
            if (selection.Status == PromptStatus.OK)
            {
                ObjectId[] objectIds = selection.Value.GetObjectIds();
                Database workingDatabase = HostApplicationServices.WorkingDatabase;
                using (workingDatabase.TransactionManager.StartTransaction())
                {
                    
                }
            }
            CAD.Ed.SetImpliedSelection(selectionSet);
        }
        [CommandMethod("Tex2")]
        public void Tex2()
        {
            if (!获取.GetEntity("文字", out DBText dbt)) return;
            using (Transaction transaction = CAD.Tr)
            {
                dbt.ObjectId.GetObject(OpenMode.ForRead);
                string str = dbt.TextString;
                int i = 0;
                i.print___NotePad(str);
            }
        }
        [CommandMethod("ELIST")]
        public void ELIST()
        {
            while (获取.GetEntity("xz", out Entity entity))
                _ = entity;
        }
    }
    public class 改快捷键
    {
        /// <summary>
        /// 多段线
        /// </summary>
        [CommandMethod("FF")]
        public void FF()
        {
            CAD.Ed.Command("PLINE");
        }
        /// <summary>
        /// 构造线
        /// </summary>
        [CommandMethod("XV")]
        public void XV()
        {

            CAD.Ed.Command("XLINE", "V");
        }
        [CommandMethod("XH")]
        public void XH()
        {

            CAD.Ed.Command("XLINE", "H");
        }
        [CommandMethod("WW")]
        public void WW()
        {

            CAD.Ed.Command("MIRROR");
        }
        [CommandMethod("DE")]
        public void DE()
        {

            CAD.Ed.Command("DIMLINEAR");
        }
        [CommandMethod("C")]
        public void C()
        {

            CAD.Ed.Command("COPY");
        }
        [CommandMethod("CC")]
        public void CC()
        {

            CAD.Ed.Command("CIRCLE");
        }
        /// <summary>
        /// 关闭块参照并保存
        /// </summary>
        [CommandMethod("REE")]
        public void REE()
        {
            CAD.Ed.Command("REFCLOSE", "S", " ");
        }
        /// <summary>
        /// 关闭块参照不保存
        /// </summary>
        [CommandMethod("RER")]
        public void RER()
        {
            CAD.Ed.Command("REFCLOSE", "D", " ");
        }
        /// <summary>
        /// 添加入块
        /// </summary>
        [CommandMethod("RA")]
        public void RA()
        {
            CAD.Ed.Command("REFSET", "A");
        }
        /// <summary>
        /// 从块中删除
        /// </summary>
        [CommandMethod("RR")]
        public void RR()
        {
            CAD.Ed.Command("REFSET", "R");
        }
        [CommandMethod("V1")]
        public void V1()
        {
            CAD.Ed.Command("-vports", "SI");
        }
        [CommandMethod("V2")]
        public void V2()
        {
            CAD.Ed.Command("-vports", "2", "V");
        }
        [CommandMethod("REF")]
        public void REF()
        {
            CAD.Ed.Command("REFEDIT");
        }
        [CommandMethod("Z")]
        public void Z()
        {
            CAD.Ed.Command("MOVE");
        }
        [CommandMethod("DA")]
        public void DA()
        {
            CAD.Ed.Command("DIMANGULAR");
        }
    }
    public class 命令代码
    {
        /// <summary>
        /// 框选点连线
        /// </summary>
        [CommandMethod("ASDD")]
        public void ASDD()
        {
            if (!选择.GetEntityAndBounds<Curve>("请框选线", out List<Curve> curves, out List<Extents3d> extents)) return;
            List<Point2d> points = [];
            if (curves.Count <= 1) return;
            for (int i = 0; i < extents.Count; i++)
            {

                for (int j = 0; j <= curves[i].EndParam; j++)
                {
                    Point3d startpoint = curves[i].GetPointAtParameter(j);
                    
                    if (startpoint.PointInBox(extents[i]))
                    {
                        points.Add(startpoint.Convert2d(new Plane()));
                    }
                }
            }

            Polyline poly = PL.CreatePolyline(points, false);
            poly.AddEntityToModeSpace();
        }
        /// <summary>
        /// 框选点删除
        /// </summary>
        [CommandMethod("ASSD")]
        public void ASSD()
        {

            if (!选择.GetEntityAndBounds<Polyline>("请框选线", out List<Polyline> curves, out List<Extents3d> extents)) return;

            List<Point2d> points = [];

            for (int i = 0; i < extents.Count; i++)
            {


                using (Transaction tr = CAD.Tr)
                {
                    for (int j = 0; j <= curves[i].EndParam; j++)
                    {
                        Point3d startpoint = curves[i].GetPointAtParameter(j);
                        if (startpoint.PointInBox(extents[i]))
                        {
                            curves[i].ObjectId.GetObject(OpenMode.ForWrite);
                            points.Add(startpoint.Convert2d(new Plane()));
                            curves[i].RemoveVertexAt(j);
                            j--;
                        }

                    }
                    tr.Commit();
                }
            }
            Polyline poly = PL.CreatePolyline(points, false);
            poly.AddEntityToModeSpace();
        }
        /// <summary>
        /// 多段线添加或删除点
        /// </summary>
        [CommandMethod("Cpt")]
        public void Cpt()
        {
            if (!获取.GetEntity("请选择一个多段线", out Polyline polyline)) return;
            CAD.Ed.SetImpliedSelection(new ObjectId[] { polyline.Id });
            CAD.Ed.Regen();
            //return;
            while (true)
            {
                if (获取.GetPoint(out Point3d point, "\n请选择点"))
                {
                    Point3d po = polyline.GetClosestPointTo(point, false);
                    bool online = false;
                    if (po.DistanceTo(point) < 0.00001)
                    {
                        online = true;
                    }
                    double p = polyline.GetParameterAtPoint(po);
                    using (Transaction tr = CAD.Tr)
                    {
                        polyline.ObjectId.GetObject(OpenMode.ForWrite);
                        if (Math.Floor(p) == p)
                        {
                            int num = (int)p;
                            if (online)
                            {
                                polyline.RemoveVertexAt(num);
                            }
                            else
                            {
                                polyline.AddVertexAt(num == 0 ? 0 : num + 1, point.Convert2d(new Plane()), 0, 0, 0);
                            }
                        }
                        else
                        {
                            polyline.AddVertexAt((int)Math.Ceiling(p), point.Convert2d(new Plane()), 0, 0, 0);
                        }
                        tr.Commit();
                    }
                }
                else { break; }
            }
        }
        /// <summary>
        /// 生成框 (修改SelectEntities)
        /// </summary>
        [CommandMethod("DLL")]
        public void DLL()
        {
            Layer.AddLayer("打印矩形图框", Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0));
            if (!选择.SelectEntities( out List<Entity> entities)) return;
            Layer.SetLayerCurrent("打印矩形图框");
            foreach (Entity entity in entities)
            {
                ///Tuple<Point3d, Point3d> tuple = BOX.GetBoxs(new List<Entity> { entity });
                BOX.GetBoxs([entity], out Tuple<Point3d, Point3d> tuple, out _);
                Point2d minpo = new(tuple.Item1.X, tuple.Item1.Y);
                Point2d maxpo = new(tuple.Item2.X, tuple.Item2.Y);
                List<Point2d> points =
                [       
                    minpo,
                    new Point2d(maxpo.X,minpo.Y),
                    maxpo,
                    new Point2d(minpo.X,maxpo.Y)
                ];
                Polyline polyline = PL.CreatePolyline(points, true);
                polyline.Layer = "打印矩形图框";
                添加.AddEntityToModeSpace(polyline);
            }
        }
        /// <summary>
        /// 生成框 (修改SelectEntities)
        /// </summary>
        [CommandMethod("DLL2")]
        public void DLL2()
        {
            Layer.AddLayer("打印矩形图框", Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0));
            if (!选择.SelectEntities(out List<Entity> entities)) return;
            Layer.SetLayerCurrent("打印矩形图框");
            
                ///Tuple<Point3d, Point3d> tuple = BOX.GetBoxs(new List<Entity> { entity });
                BOX.GetBoxs(entities, out Tuple<Point3d, Point3d> tuple, out _);
                Point2d minpo = new(tuple.Item1.X, tuple.Item1.Y);
                Point2d maxpo = new(tuple.Item2.X, tuple.Item2.Y);
                List<Point2d> points =
                [
                    minpo,
                    new Point2d(maxpo.X,minpo.Y),
                    maxpo,
                    new Point2d(minpo.X,maxpo.Y)
                ];
                Polyline polyline = PL.CreatePolyline(points, true);
                polyline.Layer = "打印矩形图框";
                添加.AddEntityToModeSpace(polyline);
            
        }
        /// <summary>
        /// 修改标注比例
        /// </summary>
        [CommandMethod("DSC")]
        public void DSC()
        {
            //object oldFileDia = Application.GetSystemVariable("DIMSCALE");
            int olddscvalue = 系统变量.当标注比例;
            if (!获取.GetInt(out int newdscvalue, "请输入新的DIMSCALE值", olddscvalue)) return;
            系统变量.当标注比例=newdscvalue;
            //Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("DIMSCALE", newdscvalue);
        }
        /// <summary>
        /// 生成单行文字
        /// </summary>
        [CommandMethod("BB")]
        public void BB()
        {
            int dscvalue = 系统变量.当标注比例;
            if (dscvalue == 0)
            {
                dscvalue = 1;
            }
            if (!获取.GetPoint(out Point3d point, "请输入一个点")) return;
            DBText dBText = new()
            {
                TextString = "单行文字",
                Height = dscvalue * 2,
                Position = point
            };
            dBText.AddEntityToModeSpace();
        }
        /// <summary>
        /// 插入多行文字
        /// </summary>
        [CommandMethod("BBB")]
        public void BBB()
        {
            int dscvalue = 系统变量.当标注比例;
            if (dscvalue == 0)
            {
                dscvalue = 1;
            }
            if (!获取.GetPoint(out Point3d point, "请输入一个点")) return;
            
            MText mText = new()
            {
                Contents = "多行文字\n多行文字",
                Location = point,
                TextHeight = dscvalue,
                Width = dscvalue * 6
            };
            mText.AddEntityToModeSpace();
            
        }
        [CommandMethod("QH")]
        public void QH()
        {
            DBText dBText = new();
             
            if (!选择.OnlySelectEntities(dBText, out List<DBText> texts)) return;
            double he = 0;
            foreach (DBText text2 in texts)
            {
                double result = 0;
                try
                {
                    result = double.Parse(text2.TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());
                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{text2.TextString}'不是数字");
                }
                he += result;
            }
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;
            DBText dBText1 = new()
            {
                TextString = he.ToString(),
                Height = texts[0].Height,
                Position = point,
            };
            dBText1.AddEntityToModeSpace();
        }
        /// <summary>
        /// 单行文字所有数字求积
        /// </summary>
        [CommandMethod("QJ")]
        public void QJ()
        {
            DBText dBText = new();
            if (!选择.OnlySelectEntities(dBText, out List<DBText> texts)) return;
            double he = 1;
            if (!获取.GetPoint(out Point3d point, "\n请输入一个点")) return;

            foreach (DBText text2 in texts)
            {
                double result = 1;
                try
                {
                    result = double.Parse(text2.TextString);
                    CAD.Ed.WriteMessage("\n");
                    //CAD.Ed.WriteMessage(result.ToString());

                }
                catch (FormatException)
                {
                    CAD.Ed.WriteMessage("\n");
                    CAD.Ed.WriteMessage($" '{text2.TextString}'不是数字");
                }
                he *= result;
            }
            DBText dBText1 = new()
            {
                TextString = he.ToString(),
                Height = texts[0].Height,
                Position = point,
            };
            dBText1.AddEntityToModeSpace();
        }
        [CommandMethod("ZML")]
        public void ZML()
        {
            幕墙.目录.Zml();
        }
        [CommandMethod("O2")]
        public void O2()
        {
            if(!获取.GetEntity("选择曲线",out Curve cur))
            {
                CAD.Ed.WriteMessage("不是曲线");
                return;
            }
            if (!获取.GetPoint(out Point3d point,"获取方向点"))return;
            //Point3d pt=cur.GetPointAtParameter(0.5);
            //Vector3d vector = point - pt;
            List<DBObjectCollection> collections = [];
            List<DBObjectCollection> collections1 = [];
            DBObjectCollection dBObject0= cur.GetOffsetCurves( 2.5);
            DBObjectCollection dBObject1 = cur.GetOffsetCurves( 20);
            DBObjectCollection dBObject2 = cur.GetOffsetCurves(-2.5);
            DBObjectCollection dBObject3 = cur.GetOffsetCurves(-20);
            if (dBObject0.Count != 0) 
            collections.Add(dBObject0);
            if (dBObject1.Count != 0) 
            collections.Add(dBObject1); 
            if (dBObject2.Count != 0)
            collections1.Add(dBObject2); 
            if (dBObject3.Count != 0) 
            collections1.Add(dBObject3);
            List<Curve> cuves = [];
            List<Curve> cuves1 = [];

            foreach (DBObjectCollection curve2 in collections)
            {
                foreach (Curve curve3 in curve2)
                {
                    cuves.Add(curve3);
                }
            }
            foreach (DBObjectCollection curve2 in collections1)
            {
                foreach (Curve curve3 in curve2)
                {
                    cuves1.Add(curve3);
                }
            }
            double d1= point.DistanceTo(cuves[0].GetClosestPointTo(point,true));
            double d2= point.DistanceTo(cuves1[0].GetClosestPointTo(point, true));
            if(d1 < d2)
            {
                cuves.AddEntityToModeSpace();
            }
            else
            {
                cuves1.AddEntityToModeSpace();
            }
           
        }

    }
}
