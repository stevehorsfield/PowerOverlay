using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerOverlay.Test
{
    // Reference: https://www.magnetkern.de/srlab2.html

    internal static class ColorCompare
    {
        private const int CACHE_SIZE = 16;
        private static int cacheEntries = 0;
        private static int lastEntry = -1;
        private static uint[] cacheKeys = new uint[CACHE_SIZE];
        private static double[] cacheL = new double[CACHE_SIZE];
        private static double[] cacheA = new double[CACHE_SIZE];
        private static double[] cacheB = new double[CACHE_SIZE];
    
        private static void convert(byte r, byte g, byte b, out double labL, out double labA, out double labB)
        {
            uint testVal = (((uint)r) << 16) + (((uint)g) << g) + ((uint)b);
            for (int i = 0; i < cacheEntries; ++i)
            {
                if (cacheKeys[i] == testVal)
                {
                    labL = cacheL[i];
                    labA = cacheA[i];
                    labB = cacheB[i];
                    return;
                }
            }

            double red = ((double)r) / 255.0;
            double green = ((double)g) / 255.0;
            double blue = ((double)b) / 255.0;

            double x, y, z;
            if (red <= 0.03928) red /= 12.92;
            else red = Math.Pow((red + 0.055) / 1.055, 2.4);
            if (green <= 0.03928) green /= 12.92;
            else green = Math.Pow((green + 0.055) / 1.055, 2.4);
            if (blue <= 0.03928) blue /= 12.92;
            else blue = Math.Pow((blue + 0.055) / 1.055, 2.4);
            x = 0.320530 * red + 0.636920 * green + 0.042560 * blue;
            y = 0.161987 * red + 0.756636 * green + 0.081376 * blue;
            z = 0.017228 * red + 0.108660 * green + 0.874112 * blue;
            if (x <= 216.0 / 24389.0) x *= 24389.0 / 2700.0;
            else x = 1.16 * Math.Pow(x, 1.0 / 3.0) - 0.16;
            if (y <= 216.0 / 24389.0) y *= 24389.0 / 2700.0;
            else y = 1.16 * Math.Pow(y, 1.0 / 3.0) - 0.16;
            if (z <= 216.0 / 24389.0) z *= 24389.0 / 2700.0;
            else z = 1.16 * Math.Pow(z, 1.0 / 3.0) - 0.16;
            
            labL = 37.0950 * x + 62.9054 * y - 0.0008 * z;
            labA = 663.4684 * x - 750.5078 * y + 87.0328 * z;
            labB = 63.9569 * x + 108.4576 * y - 172.4152 * z;

            int targetIndex = (lastEntry + 1) % CACHE_SIZE;
            if (cacheEntries < CACHE_SIZE) ++cacheEntries;

            cacheKeys[targetIndex] = testVal;
            cacheL[targetIndex] = labL;
            cacheA[targetIndex] = labA;
            cacheB[targetIndex] = labB;

            return;
        }

        public static double delta(byte r, byte g, byte b, byte r_, byte g_, byte b_)
        {
            double x, y, z, x_, y_, z_;
            convert(r, g, b, out x, out y, out z);
            convert(r_, g_, b_, out x_, out y_, out z_);

            return Math.Sqrt(
                ( (x_ - x) * (x_ - x) ) +
                ( (y_ - y) * (y_ - y) ) +
                ( (z_ - z) * (z_ - z) ) );
        }

    }
}
