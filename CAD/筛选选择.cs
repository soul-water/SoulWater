using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dimension = Autodesk.AutoCAD.DatabaseServices.Dimension;

namespace SoulWater.筛选选择
{
    internal class 筛选选择
    {
        [CommandMethod("GTT", CommandFlags.Redraw )]
        public static void GTT()
        {
            Dimension dimension = new DiametricDimension();
            if (!选择.OnlySelectEntities(dimension, out _, out SelectionSet selectionSet)) return;
            CAD.Ed.WriteMessage("选中所有的标注");
            CAD.Ed.SetImpliedSelection(selectionSet);
        }
        [CommandMethod("GTD", CommandFlags.Redraw )]
        public static void GTD()
        {
            
            List<Entity> entities =
            [
                new DBText(),
                new MText(),
                new AttributeDefinition()
            ];
            if (!选择.OnlySelectEntities(entities, out _, out SelectionSet selectionSet)) return;
            CAD.Ed.WriteMessage("选中所有的文字");
            CAD.Ed.SetImpliedSelection(selectionSet);
        }
        
        [CommandMethod("GTW", CommandFlags.Redraw )]
        public static void GTW()
        {
            if (!选择.OnlySelectEntities(new Wipeout(), out _, out SelectionSet selectionSet)) return;
            CAD.Ed.WriteMessage("选中所有的遮罩");
            CAD.Ed.SetImpliedSelection(selectionSet);
        }
        /// <summary>
        /// 只能选原，改进一下
        /// </summary>
        [CommandMethod("GTC", CommandFlags.Redraw)]
        public static void GTC()
        {
            Curve curve = new Circle();
            if (!选择.OnlySelectEntities(curve, out _, out SelectionSet selectionSet)) return;
            CAD.Ed.WriteMessage("选中所有的遮罩");
            CAD.Ed.SetImpliedSelection(selectionSet);
        }

        [CommandMethod("GTB")]
        public static void GTB()
        {

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
        public static bool OnlySelectEntities<T>(List<T> ents, out List<T> entities,out SelectionSet selectionSet) where T : Entity
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
                 selectionSet = promptSelectionResult.Value;
                return true;

            }
            else
            {
                entities = null;
                selectionSet = null;
                return false;
            }



        }
        public static bool OnlySelectEntities<T>(T ent, out List<T> entities,out SelectionSet selectionSet) where T : Entity
        {
            return OnlySelectEntities([ent], out entities,out selectionSet);
        }
    }
}
