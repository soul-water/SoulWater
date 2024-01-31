using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using SoulWater;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
[assembly: CommandClass(typeof(鼠标泡泡.Class1))]

namespace 鼠标泡泡
{
    public class Class1
    {
        static TransientManager tm = TransientManager.CurrentTransientManager;
        static Timer timer;
        static Random rand;
        static bool isrun;
        Point3d mpt;
        double viewheight;
        List<Pao> paos;
        int k;
        Document doc;
        Editor ed;
        DocumentLock dl;


        [CommandMethod("Show")]
        public void Show()
        {
            if (!isrun)
            {
                
                isrun = true;
                timer = new Timer();
                rand = new Random();
                paos = [];

                doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                
                dl = doc.LockDocument();
                ed = doc.Editor;
                viewheight = GetViewHeight();
                timer.Interval = 1;
                ed.PointMonitor += Ed_PointMonitor;
                doc.ViewChanged += Doc_ViewChanged;
                timer.Tick += 气泡产生事件;
                timer.Tick += 气泡膨胀事件;
                doc.BeginDocumentClose += Doc_BeginDocumentClose;
                timer.Start();
            }
            else
            {
                isrun= false;
                timer.Tick -= 气泡产生事件;
                timer.Tick -= 气泡膨胀事件;
                ed.PointMonitor -= Ed_PointMonitor;
                doc.ViewChanged -= Doc_ViewChanged;
                paos.ForEach(pao => tm.EraseTransient(pao.cir, []));
                paos.Clear();
                dl.Dispose();
            }

        }

        private void Doc_BeginDocumentClose(object sender, DocumentBeginCloseEventArgs e)
        {
            isrun = false;
            timer.Tick -= 气泡产生事件;
            timer.Tick -= 气泡膨胀事件;
            ed.PointMonitor -= Ed_PointMonitor;
            doc.ViewChanged -= Doc_ViewChanged;
            paos.ForEach(pao => tm.EraseTransient(pao.cir, []));
            paos.Clear();
            dl.Dispose();
        }

        private void Doc_ViewChanged(object sender, EventArgs e)
        {
            viewheight= GetViewHeight();
        }

        private void Ed_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            mpt = e.Context.RawPoint;
        }

        private void 气泡产生事件(object sender, EventArgs e)
        {
            k++;
            int t = rand.Next(5, 100);
            if (k > 1)
            {
                double ang=rand.NextDouble()*Math.PI*2;
                Color cc = rand.Next(100).Rainbow紫到蓝(0,100);
                paos.Add(new Pao(viewheight, mpt, cc, ang));
                k = 0;
            }
        }
        private void 气泡膨胀事件(object sender, EventArgs e)
        {
            foreach (var pao in paos)
            {
                pao.膨胀(mpt, viewheight);

            }
            for (int i = paos.Count - 1; i >= 0; i--)
            {
                if (paos[i].bao) paos.RemoveAt(i);
            }
            ed.UpdateScreen();
            System.Windows.Forms.Application.DoEvents();
        }



        double GetViewHeight()
        {
            return ed.GetCurrentView().Height;
        }
        class Pao
        {
            public Circle cir;
            double rmin, rmax, ang;
            int tt;
            public bool bao;

            public Pao(double viewheight, Point3d cent, Color cc, double ang)
            {
                rmin = viewheight / 200;
                rmax = viewheight / 50;
                cir = new Circle(cent, Vector3d.ZAxis, rmin)
                {
                    Color = cc
                };
                this.ang = ang;
                tm.AddTransient(cir, TransientDrawingMode.Main, 0, []);
            }
            public void 膨胀(Point3d pt, double viewheight)
            {
                rmin = viewheight / 200;
                rmax = viewheight / 50;
                double dt = (rmax - rmin) / 150;
                cir.Center = pt.PolarPoint(ang, dt*tt * 10);
                cir.Radius = rmin + dt * tt;
                tm.UpdateTransient(cir, []);

                tt++;
                if (tt > 100)
                {
                    tm.EraseTransient(cir, []);
                    bao = true;
                }
            }
        }
    }
}
