using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulWater
{
    internal class 视图
    {
        ///<summary>
        ///根据实体范围设置当前视图
        ///</summary>
        ///<param name="ent">实体</param>
        ///<param name="scale">视图比例</param>
        public static void ViewEntity(Entity ent, double scale)
        {
            //Database db = ent.Database;
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            using (ViewTableRecord vtr = new ViewTableRecord())
            {
                Point2d ptMin = new Point2d(ent.GeometricExtents.MinPoint.X, ent.GeometricExtents.MinPoint.Y);
                Point2d ptMax = new Point2d(ent.GeometricExtents.MaxPoint.X, ent.GeometricExtents.MaxPoint.Y);
                vtr.CenterPoint = new Point2d((ptMin.X + ptMax.X) / 2, (ptMin.Y + ptMax.Y) / 2);
                vtr.Width = Math.Abs(ptMax.X - ptMin.X) * scale;
                vtr.Height = Math.Abs(ptMax.Y - ptMin.Y) * scale;
                ed.SetCurrentView(vtr);

            }
        }
    }
}
