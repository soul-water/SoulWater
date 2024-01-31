using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;

namespace SoulWater
{
    internal class 矩形标注生成文字
    {
        [CommandMethod("WHT")]
        public static void WHT()
        {
            RotatedDimension dimension = new();
            CAD.Ed.WriteMessage("\n选择宽度尺寸");

            if (!选择.OnlySelectEntities(dimension, out List<RotatedDimension> dimensions0)) return;
            List<RoAlDimension> roAls0 = [];
            CAD.Ed.WriteMessage("\n选择高度尺寸");
            if (!选择.OnlySelectEntities(dimension, out List<RotatedDimension> dimensions1)) return;


            List<RoAlDimension> roAls1 = [];
            foreach (RotatedDimension rotated in dimensions1)
            {
                RoAlDimension roAlDimension = new(rotated);
                roAls1.Add(roAlDimension);
            }

            List<Xline> lines0 = [];
            
            foreach (RoAlDimension roAl in roAls0)
            {
                lines0.Add(roAl.Xl);
            }
            List<Xline> lines1 = [];
            
            foreach (RoAlDimension roAl in roAls1)
            {
                lines1.Add(roAl.Xl);
            }
            
            List<DBText> texts = [];
            for (int i = 0; i < lines0.Count; i++)
            {
                for (int j = 0; j < lines1.Count; j++)
                {
                    List<Point3d> point3Ds = lines0[i].求两个曲线交点(lines1[j]);
                    DBText dBText = new()
                    {
                        Justify = AttachmentPoint.MiddleCenter,
                        AlignmentPoint = point3Ds[0],
                        TextString = roAls0[i].XSText + "*" + roAls1[j].XSText,
                        Height = Math.Round(roAls0[i].RText / 10),
                        Rotation = roAls0[i].Rotation - Math.PI / 2,
                        WidthFactor = 0.65
                    };

                    texts.Add(dBText);
                }

            }
            //lines0.AddEntityToModeSpace();
            //lines1.AddEntityToModeSpace();
            texts.AddEntityToModeSpace();
        }
    }
}
