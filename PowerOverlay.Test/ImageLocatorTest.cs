using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.ApplicationModel.Resources.Core;

namespace PowerOverlay.Test;

[TestFixture]
public class ImageLocatorTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void CanTakeScreenshotOfWindow()
    {
        IntPtr hwnd = NativeUtils.GetActiveAppHwnd();
        var r = new NativeUtils.tagRECT();
        if (NativeUtils.GetWindowRect(hwnd, ref r) == 0)
        {
            throw new Exception("Failed to get window coordinates");
        }
        var referenceArea = new Rectangle(new Point(r.left, r.top), new Size(r.right - r.left, r.bottom - r.top));

        using var g = Graphics.FromHwnd(hwnd);
        using var b = new Bitmap(r.right - r.left, r.bottom - r.top, g);
        using var m = Graphics.FromImage(b);
        m.CopyFromScreen(new Point(r.left, r.top), new Point(0, 0), referenceArea.Size);

        var hb = b.GetHbitmap();

        var w = new System.Windows.Window();
        w.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        w.Content = new System.Windows.Controls.ScrollViewer()
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
            HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            Content = new System.Windows.Controls.Image()
            {
                Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hb, IntPtr.Zero, new System.Windows.Int32Rect(0, 0, referenceArea.Size.Width, referenceArea.Size.Height), BitmapSizeOptions.FromEmptyOptions()),
            }
        };

        w.ShowDialog();


    }

    [Test, Apartment(ApartmentState.STA)]
    public void CanTakeScreenshotOfDesktop()
    {
        IntPtr hwnd = NativeUtils.GetDesktopWindow();
        var r = new NativeUtils.tagRECT();
        if (NativeUtils.GetWindowRect(hwnd, ref r) == 0)
        {
            throw new Exception("Failed to get window coordinates");
        }
        var referenceArea = new Rectangle(new Point(r.left, r.top), new Size(r.right - r.left, r.bottom - r.top));

        using var g = Graphics.FromHwnd(hwnd);
        using var b = new Bitmap(r.right - r.left, r.bottom - r.top, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        using var m = Graphics.FromImage(b);
        m.CopyFromScreen(new Point(r.left, r.top), new Point(0, 0), referenceArea.Size);

        var hb = b.GetHbitmap();

        var w = new System.Windows.Window();
        w.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        w.Content = new System.Windows.Controls.ScrollViewer()
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
            HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            Content = new System.Windows.Controls.Image()
            {
                Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hb, IntPtr.Zero, new System.Windows.Int32Rect(0, 0, referenceArea.Size.Width, referenceArea.Size.Height), BitmapSizeOptions.FromEmptyOptions()),
            }
        };

        w.ShowDialog();
    }

    [Test,Apartment(ApartmentState.STA)]
    public void CanLocateImage()
    {
        const string screenImage = "pack://siteoforigin:,,,/LocateImageTestBitmap-test-simple-2.png";
        const string searchImage = "pack://siteoforigin:,,,/LocateImageTestBitmap-test-simple-2.png";

        System.Windows.Window testWindow, testWindow2;
        #region show image to detect

        var a = new System.Windows.Application(); // forces registration of 'pack' scheme
        var s = new BitmapImage(new Uri(screenImage));
        s.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        Console.WriteLine($"{s.PixelHeight},{s.PixelWidth}");
        testWindow = new System.Windows.Window()
        {
            SizeToContent = System.Windows.SizeToContent.Manual,
            Width = s.PixelWidth + 4,
            Height = s.PixelHeight + 4,
            Background = new SolidColorBrush(Colors.DarkOliveGreen),
            WindowStyle = System.Windows.WindowStyle.None,
            ResizeMode = System.Windows.ResizeMode.NoResize,
            BorderThickness = new System.Windows.Thickness(0),
            SnapsToDevicePixels = true,
            Content = new System.Windows.Controls.Image()
            {
                MinWidth = 0,
                StretchDirection = System.Windows.Controls.StretchDirection.DownOnly,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Source = s,
                Stretch = Stretch.None,
                Margin = new System.Windows.Thickness(0),
                SnapsToDevicePixels = true,
            }
        };
        testWindow.Show();
        Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Thread.Sleep(500);
        Console.WriteLine(testWindow.ActualWidth);
        Console.WriteLine(testWindow.ActualHeight);
        Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        #endregion

        IntPtr hwnd = new WindowInteropHelper(testWindow).Handle;

        byte[]? pixels;
        int stride;
        int width, height;
        const int indexRED = 2, indexGREEN = 1, indexBLUE = 0, indexALPHA = 3, pixelWIDTH = 4;

        #region get screen pixels from window
        {
            var r = new NativeUtils.tagRECT();
            if (NativeUtils.GetWindowRect(hwnd, ref r) == 0)
            {
                throw new Exception("Failed to get window coordinates");
            }
            var referenceArea = new Rectangle(new Point(r.left, r.top), new Size(r.right - r.left, r.bottom - r.top));

            using var g = Graphics.FromHwnd(hwnd);
            using var b = new Bitmap(r.right - r.left, r.bottom - r.top, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using var m = Graphics.FromImage(b);
            m.CopyFromScreen(new Point(r.left, r.top), new Point(0, 0), referenceArea.Size);

            var hb = b.GetHbitmap();
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hb, IntPtr.Zero, new System.Windows.Int32Rect(0, 0, referenceArea.Size.Width, referenceArea.Size.Height), BitmapSizeOptions.FromEmptyOptions());

            width = referenceArea.Width;
            height = referenceArea.Height;
            stride = referenceArea.Width * 4 + ((referenceArea.Width * 4) % 4);

            pixels = new byte[stride * referenceArea.Height];

            Console.WriteLine($"Height: {height}, Width: {width}, Stride: {stride}");
            source.CopyPixels(pixels, stride, 0);

            //for (int j = 0; j < 40; ++j)
            //{
            //    for (int i = 0; i < 40; ++i)
            //    {
            //        Console.Write("{0:X2} ", pixels[(stride * j) + i * pixelWIDTH + indexRED]);
            //        Console.Write("{0:X2} ", pixels[(stride * j) + i * pixelWIDTH + indexGREEN]);
            //        Console.Write("{0:X2} ", pixels[(stride * j) + i * pixelWIDTH + indexBLUE]);
            //        Console.Write("    ");
            //    }
            //    Console.WriteLine();
            //}

            testWindow2 = new System.Windows.Window()
            {
                Left = 500,
                SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
                Background = new SolidColorBrush(Colors.White),
                WindowStyle = System.Windows.WindowStyle.None,
                MinWidth = 0,
                MinHeight = 0,
                ResizeMode = System.Windows.ResizeMode.NoResize,
                BorderThickness = new System.Windows.Thickness(0),
                Content = new System.Windows.Controls.Image()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    Source = source,
                    Stretch = Stretch.None,
                    Margin = new System.Windows.Thickness(0),
                    MinWidth = 0,
                    MinHeight = 0
                },
            };
            testWindow2.Show();
            Thread.Sleep(200);
        }


        #endregion

        int searchWidth, searchHeight, searchStride;
        byte[]? searchPixels;
        int nonMaskTestOffsetX = -1, nonMaskTestOffsetY = -1;
        
        #region get search pixels and mask
        {
            var searchBitmap = new System.Windows.Media.Imaging.BitmapImage(
                new Uri(searchImage));
            searchBitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            searchWidth = searchBitmap.PixelWidth;
            searchHeight = searchBitmap.PixelHeight;

            //var ht = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            //    translatedBitmap.GetHbitmap(),
            //    IntPtr.Zero, new System.Windows.Int32Rect(0,0,searchWidth, searchHeight), BitmapSizeOptions.FromEmptyOptions());

            searchStride = searchWidth * pixelWIDTH;
            searchPixels = new byte[searchStride * searchHeight];
            searchBitmap.CopyPixels(searchPixels, searchStride, 0);

            for (int j = 0; j < searchHeight; ++j)
            {
                for (int i = 0; i < searchWidth; ++i)
                {
                    int offset = (j * searchStride) + (i * pixelWIDTH) + indexALPHA;
                    if (searchPixels[offset] == 0xFF)
                    {
                        nonMaskTestOffsetX = i;
                        nonMaskTestOffsetY = j;
                        break;
                    }
                }
                if (nonMaskTestOffsetX != -1) break;
            }
            if (nonMaskTestOffsetX == -1) throw new Exception("No valid pixels in search bitmap");
        }

        //for (int j = 0; j < 40; ++j)
        //{
        //    for (int i = 0; i < 40; ++i)
        //    {
        //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexALPHA]);
        //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexRED]);
        //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexGREEN]);
        //        Console.Write("{0:X2} ", searchPixels[(searchStride * j) + i * pixelWIDTH + indexBLUE]);
        //        Console.Write("    ");
        //    }
        //    Console.WriteLine();
        //}
        #endregion

        #region search for matches

        int currentX = 0;
        int currentY = 0;

        for (int originY = 0; originY < (height - searchHeight); ++originY)
        {
            Console.WriteLine($"Testing row {originY}");
            for (int originX = 0; originX < (width - searchWidth); ++originX)
            {
                bool matches = true;

                int testImageOffset = 
                    (stride * (originY + nonMaskTestOffsetY)) +
                    (pixelWIDTH * (originX + nonMaskTestOffsetX))
                    ;
                int testSearchOffset = 
                    (searchStride * nonMaskTestOffsetY) +
                    (pixelWIDTH * nonMaskTestOffsetX)
                    ;
                if (pixels[testImageOffset] != searchPixels[testSearchOffset])
                {
                    //Console.WriteLine($"No match for {originX},{originY}");
                    continue;
                }
                
                #region check at current origin
                Console.WriteLine($"Starting test at {originX},{originY}");
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
                            Console.WriteLine($"masked for {originX},{originY},{checkX},{checkY}");
                            continue;
                        }
                        // check pixel
                        if (
                            (searchPixels[searchOffset + indexBLUE] 
                               != pixels[imageOffset + indexBLUE])
                            ||
                            (searchPixels[searchOffset + indexGREEN]
                               != pixels[imageOffset + indexGREEN])
                            ||
                            (searchPixels[searchOffset + indexRED]
                               != pixels[imageOffset + indexRED])
                           )
                        {
                            //Console.WriteLine($"Mismatch for {originX},{originY},{checkX},{checkY}");
                            matches = false;
                            Console.WriteLine($"({checkX},{checkY}: " +
                                $"{searchPixels[searchOffset + indexBLUE]}," +
                                $"{searchPixels[searchOffset + indexGREEN]}," +
                                $"{searchPixels[searchOffset + indexRED]}," +
                                $" != " +
                                $"{pixels[imageOffset + indexBLUE]}," +
                                $"{pixels[imageOffset + indexGREEN]}," +
                                $"{pixels[imageOffset + indexRED]}");
                            break;
                        }
                        //Console.WriteLine($"Found matching pixel at {originX},{originY},{checkX},{checkY}");
                    }
                    if (!matches) break;

                }
                #endregion
                if (matches)
                {
                    Console.WriteLine($"Found at {originX}, {originY}");
                }
            }
        }

        #endregion

        Thread.Sleep(5000);

        testWindow.Close();
        testWindow2.Close();
    }
}
