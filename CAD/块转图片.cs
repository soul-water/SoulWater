using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Drawing;

namespace 块转图片
{
    public class Class1
    {
        [CommandMethod ("Bkbmp")]
        public static void Bkbmp()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            PromptEntityResult per = ed.GetEntity("\n请选择块参照：");
            if (per.Status != PromptStatus.OK) return;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                if (obj is BlockReference br)
                {
                    ObjectId bkid = br.BlockTableRecord;
                    _ = tr.GetObject(bkid, OpenMode.ForRead) as BlockTableRecord;
                    IntPtr ip = Autodesk.AutoCAD.Internal.Utils.GetBlockImage(bkid, 1000, 1000, Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0));
                    Bitmap bmp = Bitmap.FromHbitmap(ip);
                    System.Windows.Forms.SaveFileDialog sfd = new()
                    {
                        Filter = "bmp文件(*.bmp)|*.bmp",
                        Title = "保存bmp文件"
                    };
                    sfd.ShowDialog();
                    bmp.Save(sfd.FileName);
                }
            }
        }
        
    }
}
