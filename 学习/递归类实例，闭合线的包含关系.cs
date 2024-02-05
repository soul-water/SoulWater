using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.MacroRecorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace SoulWater.学习
{
    internal class 线的包含关系的树形型结构
    {
        int 序号;
        List<线的包含关系的树形型结构> 上级;
        List<线的包含关系的树形型结构> 下级;
        线的包含关系的树形型结构(int my)
        {
            this.序号 = my;
            上级 = new List<线的包含关系的树形型结构>();
            下级 = new List<线的包含关系的树形型结构>();
        }

        public static List<List<Curve>> Make(List<Curve> cursall)
        {
            List<List<Curve>> outlineandholes = new List<List<Curve>>();
            List<线的包含关系的树形型结构> bigx = new List<线的包含关系的树形型结构>();
            for (int i = 0; i < cursall.Count; i++)
            {
                bigx.Add(new 线的包含关系的树形型结构(i));
            }
            for (int i = 0; i < cursall.Count; i++)
            {
                for (int j = 0; j < cursall.Count; j++)
                {
                    if (i == j) continue;
                    if (cursall[j].StartPoint.Point3dCurveRayway(cursall[i]))
                    {
                        bigx[j].上级.Add(bigx[i]);
                        bigx[i].下级.Add(bigx[j]);
                    }
                }
            }
            Queue<线的包含关系的树形型结构> duilie = new Queue<线的包含关系的树形型结构>();
            foreach (var xs in bigx)
            {
                if(xs.上级.Count==0) duilie.Enqueue(xs);
            }
            while(duilie.Count > 0)
            {
                var de=duilie.Dequeue();
                List<Curve> yizu=new List<Curve>() { cursall[de.序号] };
                foreach(var c in de.下级)
                {
                    if (c.上级.Count == 1)
                    {
                        yizu.Add(cursall[c.序号]);
                        foreach(var cc in c.下级)
                        {
                            cc.上级.Remove(c);
                            cc.上级.Remove(de);
                            if(cc.上级.Count == 0)duilie.Enqueue(cc);
                        }
                    }
                }
                outlineandholes.Add(yizu);
            }
            return outlineandholes;
        }
        
    }
    internal static class 线条
    {
        static Random rand = new Random();
        public static bool Point3dCurveRayway(this Point3d pt, Curve cur)
        {
            pt = pt.Project(new Plane(), Vector3d.ZAxis);
            cur = cur.GetProjectedCurve(new Plane(), Vector3d.ZAxis);
            Ray ray = new Ray();
            ray.BasePoint = pt;
            ray.UnitDir = new Vector3d(rand.NextDouble(), rand.NextDouble(),0);//射线法
            List<Point3d> list = cur.求两个曲线交点(ray);
            if (list.Count % 2 == 0) { return false; }
            return true;
        }
    }
}
