using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulWater
{
    static class 向量
    {
        /// <summary>
        /// 叉积,求法向量
        /// </summary>
        /// <param name="a">向量a</param>
        /// <param name="b">向量b</param>
        /// <returns>右手坐标系系法向量</returns>
        public static Vector3d CrossNormal(this Vector3d a, Vector3d b)
        {
            //叉乘:依次用手指盖住每列,交叉相乘再相减,注意主副顺序
            //(a.X  a.Y  a.Z)
            //(b.X  b.Y  b.Z)
            return new Vector3d(a.Y * b.Z - b.Y * a.Z,  //主-副(左上到右下是主,左下到右上是副)
                                a.Z * b.X - b.Z * a.X,  //副-主
                                a.X * b.Y - b.X * a.Y); //主-副
        }
        /// <summary>
        /// 点加点
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Point3d PointJia(this Point3d a, Point3d b)
        {
            Vector3d vector3D = b - a;
            return a + vector3D;
        }
        /// <summary>
        /// 点加坐标
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Point3d PointJiaPo(this Point3d a, Point3d b)
        {

            return new Point3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }
}
