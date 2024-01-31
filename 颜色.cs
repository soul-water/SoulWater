using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulWater
{
    internal static class 颜色
    {
        public static Color Rainbow紫到蓝(this double a, double min, double max)
        {
            if (min > max)
            {
                (max, min) = (min, max);
            }
            if (a > max) a = max; if (a < min) a = min;
            double step = (a - min);
            double numOfSteps = max - min;
            double r = 0.0;
            double g = 0.0;
            double b = 0.0;
            double h = step / numOfSteps;
            double i = (int)(h * 5);
            double f = h * 5.0 - i;
            double q = 1 - f;
            switch (i % 5)
            {
                case 0: r = 0; g = f; b = 1; break;
                case 1: r = 1; g = 1; b = q; break;
                case 2: r = f; g = 1; b = 0; break;
                case 3: r = 1; g = q; b = 1; break;
                case 4: r = 1; g = 0; b = f; break;

            }
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
        public static Color Rainbow紫到蓝(this int value, double minValue, double maxValue)
        {
            if (minValue > maxValue)
            {
                int temp = (int)minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            if (value > maxValue)
                value = (int)maxValue;

            if (value < minValue)
                value = (int)minValue;

            double step = (double)(value - minValue);
            double numOfSteps = (double)(maxValue - minValue);
            double hue = step / numOfSteps;

            double red = 0.0;
            double green = 0.0;
            double blue = 0.0;


            double hueInterval = hue * 5.0;
            int hueIndex = (int)hueInterval;
            double hueFraction = hueInterval - hueIndex;
            double hueComplement = 1.0 - hueFraction;

            switch (hueIndex % 5)
            {
                case 0: red = 0; green = hueFraction; blue = 1; break;
                case 1: red = 1; green = 1; blue = hueComplement; break;
                case 2: red = hueFraction; green = 1; blue = 0; break;
                case 3: red = 1; green = hueComplement; blue = 1; break;
                case 4: red = 1; green = 0; blue = hueFraction; break;
            }

            byte r = (byte)(red * 255);
            byte g = (byte)(green * 255);
            byte b = (byte)(blue * 255);

            return Color.FromRgb(r, g, b);
        }
    }
}
