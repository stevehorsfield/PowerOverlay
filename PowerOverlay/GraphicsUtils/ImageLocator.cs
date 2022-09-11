using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.ApplicationModel.VoiceCommands;

namespace PowerOverlay.GraphicsUtils;

public static class ImageLocator
{
    const int indexRED = 2, indexGREEN = 1, indexBLUE = 0, indexALPHA = 3, pixelWIDTH = 4;

    private static byte[] GetScreenPixels(IntPtr hwnd, out int stride, out int width, out int height)
    {
        byte[] pixels;

        var r = new NativeUtils.tagRECT();
        if (NativeUtils.GetWindowRect(hwnd, ref r) == 0)
        {
            throw new Exception("Failed to get window coordinates");
        }
        var referenceArea = new Rectangle(new Point(r.left, r.top), new Size(r.right - r.left, r.bottom - r.top));

        using var g = Graphics.FromHwnd(hwnd);
        using var b = new Bitmap(r.right - r.left, r.bottom - r.top, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        using var m = Graphics.FromImage(b);
        m.CopyFromScreen(new Point(r.left, r.top), new Point(0, 0), /* TODO */ new Size(400,300));

        var hb = b.GetHbitmap();
        var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hb, IntPtr.Zero, new System.Windows.Int32Rect(0, 0, referenceArea.Size.Width, referenceArea.Size.Height), BitmapSizeOptions.FromEmptyOptions());

        width = referenceArea.Width;
        height = referenceArea.Height;
        stride = referenceArea.Width * 4 + ((referenceArea.Width * 4) % 4);

        pixels = new byte[stride * referenceArea.Height];

        source.CopyPixels(pixels, stride, 0);

        return pixels;
            
    }

    private static byte[] GetSearchPixels(BitmapSource image, out int stride, out int width, out int height)
    {
        width = image.PixelWidth;
        height = image.PixelHeight;
        stride = width * pixelWIDTH;
        byte[] pixels = new byte[stride * height];
        
        image.CopyPixels(pixels, stride, 0);

        return pixels;
    }

    private static Point FindFirstTestPixel(byte[] searchPixels, int stride, int width, int height)
    {
        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                int offset = (j * stride) + (i * pixelWIDTH) + indexALPHA;
                if (searchPixels[offset] == 0xFF)
                {
                    return new Point(i, j);
                }
            }
        }
        throw new Exception("No valid pixels in search bitmap");
    }

    public static bool FindImage(BitmapSource searchImage, double matchThreshold, out Point point)
    {
        IntPtr hwnd = NativeUtils.GetDesktopWindow();// new WindowInteropHelper(testWindow).Handle;

        int stride, width, height;
        byte[] pixels = GetScreenPixels(hwnd, out stride, out width, out height);
        
        int searchStride, searchWidth, searchHeight;
        byte[] searchPixels = GetSearchPixels(searchImage, out searchStride, out searchWidth, out searchHeight);

        Point testPixelLocation = FindFirstTestPixel(searchPixels, searchStride, searchWidth, searchHeight);

        // tuple of location and greatest matching delta
        List<Tuple<Point, double>> matchList = new List<Tuple<Point, double>>();

        for (int originY = 0; originY < (height - searchHeight); ++originY)
        {
            //Console.WriteLine($"Testing row {originY}");
            for (int originX = 0; originX < (width - searchWidth); ++originX)
            {
                bool matches = true;
                double matchMaxDelta = 0.0;

                int testImageOffset =
                    (stride * (originY + testPixelLocation.Y)) +
                    (pixelWIDTH * (originX + testPixelLocation.X))
                    ;
                int testSearchOffset =
                    (searchStride * testPixelLocation.Y) +
                    (pixelWIDTH * testPixelLocation.X)
                    ;

                if (!checkPixel(matchThreshold, pixels, searchPixels, testImageOffset, testSearchOffset, out matchMaxDelta))
                {
                    continue;
                }

                #region check at current origin
                //Console.WriteLine($"Starting test at {originX},{originY}");
                for (int checkY = 0; checkY < searchHeight; ++checkY)
                {
                    for (int checkX = 0; checkX < searchWidth; ++checkX)
                    {
                        int imageOffset =
                                ((originY + checkY) * stride) +
                                ((originX + checkX) * pixelWIDTH);
                        int searchOffset =
                                (checkY * searchStride) +
                                (checkX * pixelWIDTH);

                        // check mask
                        if (searchPixels[searchOffset + indexALPHA] == 0x00)
                        {
                            continue;
                        }
                        // check pixel
                        double delta;
                        if (!checkPixel(matchThreshold, pixels, searchPixels, imageOffset, searchOffset, out delta))
                        {
                            matches = false;
                            break;
                        }
                        matchMaxDelta = delta > matchMaxDelta ? delta : matchMaxDelta;
                    }
                    if (!matches) break;

                }
                #endregion
                if (matches)
                {
                    matchList.Add(Tuple.Create(new Point(originX, originY), matchMaxDelta));
                }
            }
        }

        if (matchList.Count > 0)
        {
            point = matchList[0].Item1;
            return true;
        }
        point = new Point(0, 0);
        return false;
    }

    //private void referenceTestCode()
    //{


    //    int searchWidth, searchHeight, searchStride;
    //    byte[]? searchPixels;
    //    int nonMaskTestOffsetX = -1, nonMaskTestOffsetY = -1;

    //    #region get search pixels and mask
    //    {
    //        var searchBitmap = new System.Windows.Media.Imaging.BitmapImage(
    //            new Uri(searchImage));
    //        searchBitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
    //        searchWidth = searchBitmap.PixelWidth;
    //        searchHeight = searchBitmap.PixelHeight;

    //        //var ht = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
    //        //    translatedBitmap.GetHbitmap(),
    //        //    IntPtr.Zero, new System.Windows.Int32Rect(0,0,searchWidth, searchHeight), BitmapSizeOptions.FromEmptyOptions());

    //        searchStride = searchWidth * pixelWIDTH;
    //        searchPixels = new byte[searchStride * searchHeight];
    //        searchBitmap.CopyPixels(searchPixels, searchStride, 0);

    //        for (int j = 0; j < searchHeight; ++j)
    //        {
    //            for (int i = 0; i < searchWidth; ++i)
    //            {
    //                int offset = (j * searchStride) + (i * pixelWIDTH) + indexALPHA;
    //                if (searchPixels[offset] == 0xFF)
    //                {
    //                    nonMaskTestOffsetX = i;
    //                    nonMaskTestOffsetY = j;
    //                    break;
    //                }
    //            }
    //            if (nonMaskTestOffsetX != -1) break;
    //        }
    //        if (nonMaskTestOffsetX == -1) throw new Exception("No valid pixels in search bitmap");
    //    }

    //    //for (int j = 0; j < 40; ++j)
    //    //{
    //    //    for (int i = 0; i < 40; ++i)
    //    //    {
    //    //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexALPHA]);
    //    //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexRED]);
    //    //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexGREEN]);
    //    //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexBLUE]);
    //    //        Console.Write("    ");
    //    //    }
    //    //    Console.WriteLine();
    //    //}
    //    #endregion

    //    #region search for matches

    //    int currentX = 0;
    //    int currentY = 0;

    //    for (int originY = 0; originY < (height - searchHeight); ++originY)
    //    {
    //        //Console.WriteLine($"Testing row {originY}");
    //        for (int originX = 0; originX < (width - searchWidth); ++originX)
    //        {
    //            bool matches = true;

    //            int testImageOffset =
    //                (stride * (originY + nonMaskTestOffsetY)) +
    //                (pixelWIDTH * (originX + nonMaskTestOffsetX))
    //                ;
    //            int testSearchOffset =
    //                (searchStride * nonMaskTestOffsetY) +
    //                (pixelWIDTH * nonMaskTestOffsetX)
    //                ;
    //            if (!checkPixel(matchThreshold, pixels, searchPixels, testImageOffset, testSearchOffset))
    //            {
    //                //Console.WriteLine($"No match for {originX},{originY}");
    //                continue;
    //            }

    //            #region check at current origin
    //            //Console.WriteLine($"Starting test at {originX},{originY}");
    //            for (int checkY = 0; checkY < searchHeight; ++checkY)
    //            {
    //                for (int checkX = 0; checkX < searchWidth; ++checkX)
    //                {
    //                    int imageOffset =
    //                            ((originY + checkY) * stride) +
    //                            ((originX + checkX) * pixelWIDTH);
    //                    int searchOffset =
    //                            (checkY * searchStride) +
    //                            (checkX * pixelWIDTH);

    //                    // check mask
    //                    if (searchPixels[searchOffset + indexALPHA] == 0x00)
    //                    {
    //                        //Console.WriteLine($"masked for {originX},{originY},{checkX},{checkY}");
    //                        continue;
    //                    }
    //                    // check pixel
    //                    if (!checkPixel(matchThreshold, pixels, searchPixels, imageOffset, searchOffset))
    //                    {
    //                        //Console.WriteLine($"Mismatch for {originX},{originY},{checkX},{checkY}");
    //                        matches = false;
    //                        //Console.WriteLine($"({checkX},{checkY}: " +
    //                        //    $"{searchPixels[searchOffset + indexBLUE]}," +
    //                        //    $"{searchPixels[searchOffset + indexGREEN]}," +
    //                        //    $"{searchPixels[searchOffset + indexRED]}," +
    //                        //    $" != " +
    //                        //    $"{pixels[imageOffset + indexBLUE]}," +
    //                        //    $"{pixels[imageOffset + indexGREEN]}," +
    //                        //    $"{pixels[imageOffset + indexRED]}");
    //                        break;
    //                    }
    //                    //Console.WriteLine($"Found matching pixel at {originX},{originY},{checkX},{checkY}");
    //                }
    //                if (!matches) break;

    //            }
    //            #endregion
    //            if (matches)
    //            {
    //                Console.WriteLine($"Found at {originX}, {originY}");
    //            }
    //        }
    //    }

    //    #endregion

    //    //Thread.Sleep(5000);

    //    testWindow.Close();
    //    testWindow2.Close();
    //}

    private static bool checkPixel(double deltaThreshold, byte[] pixels, byte[] searchPixels, int imageOffset, int searchOffset, out double matchedThreshold)
    {
        const int indexRED = 2, indexGREEN = 1, indexBLUE = 0, indexALPHA = 3, pixelWIDTH = 4;

        matchedThreshold = 0.0;

        if (searchPixels[searchOffset + indexALPHA] == 0x00) return true;

        byte r, g, b, r_, g_, b_;
        r = pixels[imageOffset + indexRED];
        g = pixels[imageOffset + indexGREEN];
        b = pixels[imageOffset + indexBLUE];

        r_ = searchPixels[searchOffset + indexRED];
        g_ = searchPixels[searchOffset + indexGREEN];
        b_ = searchPixels[searchOffset + indexBLUE];

        if ((r == r_) && (g == g_) && (b == b_)) return true;

        matchedThreshold = ColorCompare.delta(r, g, b, r_, g_, b_);

        if (matchedThreshold < deltaThreshold) return true;

        return false;
    }


}
