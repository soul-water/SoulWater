using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace SoulWater
{
    internal class 获取
    {
        public static bool GetDouble(out double d, string message = "输入数字", double value = 0)
        {
            d = 0;
            Editor editor = CAD.Ed;
            PromptDoubleOptions pdo = new(message)
            {
                DefaultValue = value
            };
            PromptDoubleResult pdr = editor.GetDouble(pdo);
            if (pdr.Status == PromptStatus.OK)
            {
                d = pdr.Value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取用户输入整数
        /// </summary>
        /// <param name="message">提示词</param>
        /// <param name="num">传出的整数</param>
        /// <returns></returns>
        public static bool GetInt(out int num, string message = "", int value = 0)
        {
            num = 0;
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptIntegerOptions pio = new(message)
            {
                DefaultValue = value
            };
            PromptIntegerResult pir = editor.GetInteger(pio);
            if (pir.Status == PromptStatus.OK)
            {
                num = pir.Value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取一个点
        /// </summary>
        /// <param name="message">提示词</param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool GetPoint(out Point3d point, string message = "选择点", Point3d? p = null)
        {
            point = Point3d.Origin;
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions ppo = new(message);
            if (p != null)
            {
                ppo.BasePoint = p.Value;
                ppo.UseBasePoint = true;
            }
            PromptPointResult ppr = editor.GetPoint(ppo);
            if (ppr.Status == PromptStatus.OK)
            {
                point = ppr.Value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取用户输入文字
        /// </summary>
        /// <param name="message">提示词</param>
        /// <param name="str">传出的文字</param>
        /// <returns></returns>
        public static bool GetString(out string str, string message = "")
        {
            str = "";
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptResult pr = editor.GetString(message);
            if (pr.Status == PromptStatus.OK)
            {
                str = pr.StringResult;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="message">提示消息</param>
        /// <param name="entity">输出实体</param>
        /// <returns>是否选择到</returns>
        public static bool GetEntity<T>(string message, out T entity) where T : Entity
            //选择某一种entity
        {
            entity = null;
            Editor editor = CAD.Ed;
            PromptEntityResult per = editor.GetEntity("\n" + message);
            if (per.Status == PromptStatus.OK)
            {
                ObjectId objectId = per.ObjectId;
                //Database database = CAD.Db;
                using (Transaction tr = CAD.Tr)
                {
                    entity = objectId.GetObject(OpenMode.ForRead) as T;
                    if (entity == null) return false;
                    else return true;
                }
            }
            return false;
        }
    }
    internal class 选择
    {
        /// <summary>
        /// 只选择某几种类型的实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="ents">实体</param>
        /// <returns>实体列表</returns>
        public static bool OnlySelectEntities<T>(List<T> ents, out List<T> entities) where T : Entity
        {
            // 创建一个空的实体列表
            List<T> list = [];
            // 获取实体列表中每个实体的 DXF 名称
            List<string> strings = ents.Select(ent => ent.GetRXClass().DxfName).ToList();

            // 获取当前文档的编辑器
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            // 创建一个 TypedValue 数组，用于存储选择过滤器的条件
            TypedValue[] typedValues = new TypedValue[strings.Count + 2];
            typedValues[0] = new TypedValue((int)DxfCode.Operator, "<or");
            typedValues[strings.Count + 1] = new TypedValue((int)DxfCode.Operator, "or>");

            // 将每个实体的 DXF 名称添加到 TypedValue 数组中
            for (int i = 0; i < ents.Count; i++)
            {
                typedValues[i + 1] = new TypedValue((int)DxfCode.Start, strings[i]);
            }

            // 创建一个选择过滤器，并使用上面创建的 TypedValue 数组作为条件
            SelectionFilter selectionFilter = new(typedValues);
            // 显示选择对话框，并获取用户选择的实体
            var promptSelectionResult = editor.GetSelection(selectionFilter);

            // 如果用户选择了实体，则将其添加到实体列表中
            if (promptSelectionResult.Status == PromptStatus.OK)
            {
                using (Transaction transaction = CAD.Tr)
                {
                    SelectionSet SS = promptSelectionResult.Value;
                    foreach (ObjectId id in SS.GetObjectIds())
                    {
                        T entity = (T)transaction.GetObject(id, OpenMode.ForWrite, true);
                        if (entity != null) { list.Add(entity); }
                    }
                    transaction.Commit();
                }
            }

            // 如果实体列表不为空，则将其赋值给传入的实体列表，并返回 true；否则返回 false
            if (list.Count > 0)
            {
                entities = list;
                return true;
            }
            else
            {
                entities = null;
                return false;
            }



        }
        public static bool OnlySelectEntities<T>(T ent, out List<T> entities) where T : Entity
        {
            return OnlySelectEntities([ent], out entities);
        }
        /// <summary>
        /// 获取当前空间全部实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>实体列表</returns>
        public static List<T> SelectAllEntities<T>() where T : Entity
        {
            List<T> list = [];
            Database workingDatabase = HostApplicationServices.WorkingDatabase;
            using (Transaction transaction = workingDatabase.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)workingDatabase.BlockTableId.GetObject(OpenMode.ForRead);
                BlockTableRecord blockTableRecord = (BlockTableRecord)blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead);

                list = blockTableRecord.Cast<ObjectId>()
                    .Select(id => transaction.GetObject(id, OpenMode.ForRead))
                    .OfType<T>()
                    .ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取选择的实体和选择时框选的框
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="message">提示消息</param>
        /// <param name="entitys">输出实体列表</param>
        /// <param name="extents3D">输出框选范围的Extents3d列表</param>
        /// <returns>是否选择完成</returns>
        public static bool GetEntityAndBounds<T>(string message, out List<T> entitys, out List<Extents3d> extents3D) where T : Entity
        {
            entitys = [];

            extents3D = [];
            PromptSelectionOptions selOpt = new()
            {
                PrepareOptionalDetails = true
            };
            CAD.Ed.WriteMessage("\n" + message);
            PromptSelectionResult promptSelectionResult = CAD.Ed.GetSelection(selOpt);//定义用户自己选择object
            SelectionSet mySS = promptSelectionResult.Value;
            if (promptSelectionResult.Status == PromptStatus.OK)
            {

                List<int> list = [];
                using (Transaction tr = CAD.Tr)
                {
                    ObjectId[] objectIds = promptSelectionResult.Value.GetObjectIds();

                    //for (int i = 0; i < objectIds.Length; i++)
                    //{
                    //    if (objectIds[i] is T)
                    //    {
                    //        entitys.Add(objectIds[i].GetObject(OpenMode.ForRead) as T);
                    //    }
                    //}
                    ObjectId[] array = objectIds;
                    int i = 0;
                    foreach (ObjectId objectId in array)
                    {
                        Entity entity = (Entity)objectId.GetObject(OpenMode.ForRead);
                        if (entity is T)
                        {
                            entitys.Add(objectIds[i].GetObject(OpenMode.ForRead) as T);

                        }
                        else list.Add(i);
                        i++;
                    }
                }
                int d = 0;
                foreach (SelectedObject ssobj in mySS)
                {
                    d++;

                    if (ssobj.SelectionMethod == SelectionMethod.Crossing)
                    {
                        CrossingOrWindowSelectedObject pickPtObj = ssobj as CrossingOrWindowSelectedObject;
                        PickPointDescriptor[] pickPt = pickPtObj.GetPickPoints();
                        //Point3d pt = pickPt[0].PointOnLine;
                        List<Point2d> pt2 = [];
                        for (int i = 0; i < pickPt.Length; i++)
                        {

                            pt2.Add(pickPt[i].PointOnLine.Convert2d(new Plane()));

                        }
                        if (list.Contains(d - 1)) { continue; }
                        Polyline polyline = PL.CreatePolyline(pt2, true);
                        extents3D.Add(polyline.Bounds.Value);

                        //Polyline polyline1 = PolyLine.CreatePolyline(new List<Point2d> { minpt.Convert2d(new Plane()), maxpt.Convert2d(new Plane()) }, false);
                        //AddEntity.AddEntityToModeSpace(polyline1);
                    }
                }

            }
            if (extents3D != null && entitys != null) return true; return false;
        }
        /// <summary>
        /// 选择一类实体
        /// </summary>
        /// <typeparam name="T"><实体类型/typeparam>
        /// <param name="result">返回实体列表</param>
        /// <param name="message">提示消息</param>
        /// <returns>是否选择成功</returns>
        public static bool SelectEntities<T>(out List<T> result, string message = "选择图形") where T : Entity
        {
            result = [];
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions promptSelectionOptions = new()
            {
                MessageForAdding = "\n" + message
            };
            PromptSelectionResult selection = editor.GetSelection(promptSelectionOptions);
            if (selection.Status == PromptStatus.OK)
            {
                ObjectId[] objectIds = selection.Value.GetObjectIds();
                Database workingDatabase = HostApplicationServices.WorkingDatabase;
                using (workingDatabase.TransactionManager.StartTransaction())
                {
                    ObjectId[] array = objectIds;
                    foreach (ObjectId objectId in array)
                    {
                        Entity entity = (Entity)objectId.GetObject(OpenMode.ForRead);
                        if (entity is T)
                        {
                            result.Add(entity as T);
                        }
                    }
                }
            }

            return result.Count() > 0;
        }
    }
    internal static class 添加
    {

        public static void AddEntityToModeSpace<T>(this List<T> ents, int n = 0) where T : Entity
        {

            if (n == -1)
            {

                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction tran = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord modelspace = tran.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    foreach (var item in ents)
                    {
                        if (item.IsNewObject)
                        {
                            modelspace.AppendEntity(item);
                            tran.AddNewlyCreatedDBObject(item, true);
                        }
                        else
                        {
                            ;
                        }
                    }
                    tran.Commit();
                }
            }
            else
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction tran = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)bt[n == 0 ? BlockTableRecord.ModelSpace : BlockTableRecord.PaperSpace].GetObject(OpenMode.ForWrite);
                    foreach (var item in ents)
                    {
                        if (item.IsNewObject)
                        {
                            btr.AppendEntity(item);
                            tran.AddNewlyCreatedDBObject(item, true);
                        }
                        else
                        {
                            ;
                        }
                    }
                    tran.Commit();
                }
            }
        }
        public static void AddEntityToModeSpace(this Entity ent, int n = 0)
        {
            AddEntityToModeSpace(new List<Entity>() { ent }, n);
        }
    }
    internal static class 删除
    {
        /// <summary>
        /// 删除列表中的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public static void DeleateRntity<T>(this List<T> entities) where T : Entity
        {
            using (Transaction trans = CAD.Tr)
            {
                foreach (Entity entity in entities)
                {
                    entity.ObjectId.GetObject(OpenMode.ForWrite).Erase();
                }
                trans.Commit();
            }
        }
        public static void DeleateRntity<T>(this Entity entitity) where T : Entity
        {
            DeleateRntity(new List<Entity>() { entitity });
        }
    }
}
