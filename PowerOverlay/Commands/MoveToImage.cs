using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Linq.Expressions;
using System.IO;

namespace PowerOverlay.Commands;

public enum MoveToImageReferenceSource
{
    Cursor,
    WindowFrame,
    WindowContent,
    CurrentMonitor,
    EntireDesktop,
}

public enum MoveToImageClipMode
{
    None,
    Monitor,
    Window,
}

public enum MoveToImageAnchor
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    CenterPoint,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}

public class MoveToImage : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return MoveToImageDefinition.Instance; } }

    private readonly FrameworkElement configElement = MoveToImageDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    private BitmapSource? imageSource;
    private byte[]? imageData;

    private MoveToImageReferenceSource referenceSource = MoveToImageReferenceSource.WindowFrame;
    private MoveToImageAnchor searchAnchor = MoveToImageAnchor.TopLeft;
    private MoveToImageAnchor moveAnchor = MoveToImageAnchor.CenterPoint;
    private MoveToImageClipMode clipMode = MoveToImageClipMode.None;
    private int offsetLeft = 0, offsetTop = 0;
    private int rectWidth = 0, rectHeight = 0;
    private int colourDeltaThresholdTenths = 15;

    public byte[]? ImageData
    {
        get
        {
            return imageData;
        }
        set
        {
            imageData = value;
            imageSource = null;
            if (imageData != null)
            {
                try
                {
                    var ms = new MemoryStream(imageData);
                    var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    imageSource = decoder.Frames[0].Clone();
                    imageSource.DecodeFailed += (s,e2) =>
                    {
                        DebugLog.Log($"Failed to decode image: {e2.ErrorException.Message}");
                    };
                }
                catch (Exception e)
                {
                    DebugLog.Log($"Unable to decode image: {e.Message}");
                }
            }
            RaisePropertyChanged(nameof(ImageData));
            RaisePropertyChanged(nameof(ImageSource));
        }
    }
    public BitmapSource? ImageSource
    {
        get
        {
            return imageSource;
        }
        set
        {
            imageSource = null;
            imageData = null;
            if (value != null)
            {
                try
                {
                    imageSource = value.Clone();
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(value));
                    using var ms = new MemoryStream();
                    encoder.Save(ms);
                    imageData = ms.ToArray();
                }
                catch (Exception e)
                {
                    DebugLog.Log($"Unable to encode bitmap image: {e.Message}");
                }
            }
            RaisePropertyChanged(nameof(ImageData));
            RaisePropertyChanged(nameof(ImageSource));
        }
    }

    public MoveToImageReferenceSource ReferenceSource
    {
        get { return referenceSource; }
        set
        {
            referenceSource = value;
            RaisePropertyChanged(nameof(ReferenceSource));
            RaisePropertyChanged(nameof(ReferenceSourceIsCursor));
            RaisePropertyChanged(nameof(ReferenceSourceIsWindowFrame));
            RaisePropertyChanged(nameof(ReferenceSourceIsWindowContent));
            RaisePropertyChanged(nameof(ReferenceSourceIsCurrentMonitor));
            RaisePropertyChanged(nameof(ReferenceSourceIsEntireDesktop));
        }
    }

    public bool ReferenceSourceIsCursor { get { return ReferenceSource == MoveToImageReferenceSource.Cursor; } set { if (value) ReferenceSource = MoveToImageReferenceSource.Cursor; } }
    public bool ReferenceSourceIsWindowFrame { get { return ReferenceSource == MoveToImageReferenceSource.WindowFrame; } set { if (value) ReferenceSource = MoveToImageReferenceSource.WindowFrame; } }
    public bool ReferenceSourceIsWindowContent { get { return ReferenceSource == MoveToImageReferenceSource.WindowContent; } set { if (value) ReferenceSource = MoveToImageReferenceSource.WindowContent; } }
    public bool ReferenceSourceIsCurrentMonitor { get { return ReferenceSource == MoveToImageReferenceSource.CurrentMonitor; } set { if (value) ReferenceSource = MoveToImageReferenceSource.CurrentMonitor; } }
    public bool ReferenceSourceIsEntireDesktop { get { return ReferenceSource == MoveToImageReferenceSource.EntireDesktop; } set { if (value) ReferenceSource = MoveToImageReferenceSource.EntireDesktop; } }

    public MoveToImageAnchor SearchAnchor
    {
        get { return searchAnchor; }
        set
        {
            searchAnchor = value;
            RaisePropertyChanged(nameof(SearchAnchor));
            RaisePropertyChanged(nameof(IsSearchAnchorTopLeft));
            RaisePropertyChanged(nameof(IsSearchAnchorTopCenter));
            RaisePropertyChanged(nameof(IsSearchAnchorTopRight));
            RaisePropertyChanged(nameof(IsSearchAnchorMiddleLeft));
            RaisePropertyChanged(nameof(IsSearchAnchorCenterPoint));
            RaisePropertyChanged(nameof(IsSearchAnchorMiddleRight));
            RaisePropertyChanged(nameof(IsSearchAnchorBottomLeft));
            RaisePropertyChanged(nameof(IsSearchAnchorBottomCenter));
            RaisePropertyChanged(nameof(IsSearchAnchorBottomRight));
        }
    }

    public bool IsSearchAnchorTopLeft { get { return SearchAnchor == MoveToImageAnchor.TopLeft; } set { if (value) SearchAnchor = MoveToImageAnchor.TopLeft; } }
    public bool IsSearchAnchorTopCenter { get { return SearchAnchor == MoveToImageAnchor.TopCenter; } set { if (value) SearchAnchor = MoveToImageAnchor.TopCenter; } }
    public bool IsSearchAnchorTopRight { get { return SearchAnchor == MoveToImageAnchor.TopRight; } set { if (value) SearchAnchor = MoveToImageAnchor.TopRight; } }
    public bool IsSearchAnchorMiddleLeft { get { return SearchAnchor == MoveToImageAnchor.MiddleLeft; } set { if (value) SearchAnchor = MoveToImageAnchor.MiddleLeft; } }
    public bool IsSearchAnchorCenterPoint { get { return SearchAnchor == MoveToImageAnchor.CenterPoint; } set { if (value) SearchAnchor = MoveToImageAnchor.CenterPoint; } }
    public bool IsSearchAnchorMiddleRight { get { return SearchAnchor == MoveToImageAnchor.MiddleRight; } set { if (value) SearchAnchor = MoveToImageAnchor.MiddleRight; } }
    public bool IsSearchAnchorBottomLeft { get { return SearchAnchor == MoveToImageAnchor.BottomLeft; } set { if (value) SearchAnchor = MoveToImageAnchor.BottomLeft; } }
    public bool IsSearchAnchorBottomCenter { get { return SearchAnchor == MoveToImageAnchor.BottomCenter; } set { if (value) SearchAnchor = MoveToImageAnchor.BottomCenter; } }
    public bool IsSearchAnchorBottomRight { get { return SearchAnchor == MoveToImageAnchor.BottomRight; } set { if (value) SearchAnchor = MoveToImageAnchor.BottomRight; } }

    public MoveToImageAnchor MoveAnchor
    {
        get { return moveAnchor; }
        set
        {
            moveAnchor = value;
            RaisePropertyChanged(nameof(MoveAnchor));
            RaisePropertyChanged(nameof(IsMoveAnchorTopLeft));
            RaisePropertyChanged(nameof(IsMoveAnchorTopCenter));
            RaisePropertyChanged(nameof(IsMoveAnchorTopRight));
            RaisePropertyChanged(nameof(IsMoveAnchorMiddleLeft));
            RaisePropertyChanged(nameof(IsMoveAnchorCenterPoint));
            RaisePropertyChanged(nameof(IsMoveAnchorMiddleRight));
            RaisePropertyChanged(nameof(IsMoveAnchorBottomLeft));
            RaisePropertyChanged(nameof(IsMoveAnchorBottomCenter));
            RaisePropertyChanged(nameof(IsMoveAnchorBottomRight));
        }
    }

    public bool IsMoveAnchorTopLeft { get { return MoveAnchor == MoveToImageAnchor.TopLeft; } set { if (value) MoveAnchor = MoveToImageAnchor.TopLeft; } }
    public bool IsMoveAnchorTopCenter { get { return MoveAnchor == MoveToImageAnchor.TopCenter; } set { if (value) MoveAnchor = MoveToImageAnchor.TopCenter; } }
    public bool IsMoveAnchorTopRight { get { return MoveAnchor == MoveToImageAnchor.TopRight; } set { if (value) MoveAnchor = MoveToImageAnchor.TopRight; } }
    public bool IsMoveAnchorMiddleLeft { get { return MoveAnchor == MoveToImageAnchor.MiddleLeft; } set { if (value) MoveAnchor = MoveToImageAnchor.MiddleLeft; } }
    public bool IsMoveAnchorCenterPoint { get { return MoveAnchor == MoveToImageAnchor.CenterPoint; } set { if (value) MoveAnchor = MoveToImageAnchor.CenterPoint; } }
    public bool IsMoveAnchorMiddleRight { get { return MoveAnchor == MoveToImageAnchor.MiddleRight; } set { if (value) MoveAnchor = MoveToImageAnchor.MiddleRight; } }
    public bool IsMoveAnchorBottomLeft { get { return MoveAnchor == MoveToImageAnchor.BottomLeft; } set { if (value) MoveAnchor = MoveToImageAnchor.BottomLeft; } }
    public bool IsMoveAnchorBottomCenter { get { return SearchAnchor == MoveToImageAnchor.BottomCenter; } set { if (value) MoveAnchor = MoveToImageAnchor.BottomCenter; } }
    public bool IsMoveAnchorBottomRight { get { return SearchAnchor == MoveToImageAnchor.BottomRight; } set { if (value) MoveAnchor = MoveToImageAnchor.BottomRight; } }

    public MoveToImageClipMode ClipMode
    {
        get { return clipMode; }
        set
        {
            clipMode = value;
            RaisePropertyChanged(nameof(ClipMode));
        }
    }

    public bool ClipModeIsNone { get { return ClipMode== MoveToImageClipMode.None; } set { if (value) ClipMode = MoveToImageClipMode.None; } }
    public bool ClipModeIsWindow { get { return ClipMode == MoveToImageClipMode.Window; } set { if (value) ClipMode = MoveToImageClipMode.Window; } }
    public bool ClipModeIsMonitor { get { return ClipMode == MoveToImageClipMode.Monitor; } set { if (value) ClipMode = MoveToImageClipMode.Monitor; } }

    public int OffsetLeft { get { return offsetLeft; } set { offsetLeft = value; RaisePropertyChanged(nameof(OffsetLeft)); } }
    public int OffsetTop { get { return offsetTop; } set { offsetTop = value; RaisePropertyChanged(nameof(OffsetTop)); } }
    public int RectWidth { get { return rectWidth; } set { rectWidth = value; RaisePropertyChanged(nameof(RectWidth)); } }
    public int RectHeight { get { return rectHeight; } set { rectHeight = value; RaisePropertyChanged(nameof(RectHeight)); } }
    public int ColourDeltaThresholdTenths { get { return colourDeltaThresholdTenths; } set { colourDeltaThresholdTenths = value; RaisePropertyChanged(nameof(ColourDeltaThresholdTenths)); } }

    public override ActionCommand Clone()
    {
        return new MoveToImage()
        {
            ImageData = ImageData,
            OffsetLeft = OffsetLeft,
            OffsetTop = OffsetTop,
            RectWidth = RectWidth,
            RectHeight = RectHeight,
            ColourDeltaThresholdTenths = ColourDeltaThresholdTenths,
            ClipMode = ClipMode,
            ReferenceSource = ReferenceSource,
            MoveAnchor = MoveAnchor,
            SearchAnchor = SearchAnchor,
        };
    }

    public override void WriteJson(JsonObject o)
    {
        if (ImageData != null && ImageData.Length > 0)
        {
            o.AddLowerCamelValue(nameof(ImageData), Convert.ToBase64String(ImageData));
        }
        o.AddLowerCamelValue(nameof(OffsetLeft), OffsetLeft);
        o.AddLowerCamelValue(nameof(OffsetTop), OffsetTop);
        o.AddLowerCamelValue(nameof(RectWidth), RectWidth);
        o.AddLowerCamelValue(nameof(RectHeight), RectHeight);
        o.AddLowerCamelValue(nameof(ColourDeltaThresholdTenths), ColourDeltaThresholdTenths);
        o.AddLowerCamelValue(nameof(ClipMode), ClipMode.ToString());
        o.AddLowerCamelValue(nameof(ReferenceSource), ReferenceSource.ToString());
        o.AddLowerCamelValue(nameof(MoveAnchor), MoveAnchor.ToString());
        o.AddLowerCamelValue(nameof(SearchAnchor), SearchAnchor.ToString());
    }

    public static MoveToImage CreateFromJson(JsonObject o)
    {
        MoveToImage result = new();

        o.TryGet<string>(nameof(ImageData), s =>
        {
            try
            {
                result.ImageData = Convert.FromBase64String(s);
            }
            catch (Exception e)
            {
                DebugLog.Log($"Unable to decode image data from JSON: {e.Message}");
            }
        });
        o.TryGetValue<int>(nameof(OffsetLeft), d => result.OffsetLeft = d);
        o.TryGetValue<int>(nameof(OffsetTop), d => result.OffsetTop = d);
        o.TryGetValue<int>(nameof(RectWidth), d => result.RectWidth = d);
        o.TryGetValue<int>(nameof(RectHeight), d => result.RectHeight = d);
        o.TryGetValue<int>(nameof(ColourDeltaThresholdTenths), d => result.ColourDeltaThresholdTenths = d);
        o.TryGet<string>(nameof(ClipMode), s =>
        {
            MoveToImageClipMode m;
            if (Enum.TryParse<MoveToImageClipMode>(s, true, out m))
            {
                result.ClipMode = m;
            }
        });
        o.TryGet<string>(nameof(ReferenceSource), s =>
        {
            MoveToImageReferenceSource m;
            if (Enum.TryParse<MoveToImageReferenceSource>(s, true, out m))
            {
                result.ReferenceSource = m;
            }
        });
        o.TryGet<string>(nameof(MoveAnchor), s =>
        {
            MoveToImageAnchor m;
            if (Enum.TryParse<MoveToImageAnchor>(s, true, out m))
            {
                result.MoveAnchor = m;
            }
        });
        o.TryGet<string>(nameof(SearchAnchor), s =>
        {
            MoveToImageAnchor m;
            if (Enum.TryParse<MoveToImageAnchor>(s, true, out m))
            {
                result.SearchAnchor = m;
            }
        });

        return result;
    }

    public override bool CanExecute(object? parameter)
    {
        return (ImageSource != null);
    }

    public override Task ExecuteWithContext(CommandExecutionContext context)
    {
        if (ImageSource == null) return Task.CompletedTask;

        System.Drawing.Point p;
        if (! PowerOverlay.GraphicsUtils.ImageLocator.FindImage(ImageSource, 1.5, out p))
        {
            return Task.CompletedTask;
        };
        NativeUtils.SetCursorPos(p.X, p.Y);
        return Task.CompletedTask;
    }
}

public class MoveToImageDefinition : ActionCommandDefinition
{
    public static MoveToImageDefinition Instance = new();

    public override string ActionName => "MoveToImage";
    public override string ActionDisplayName => "Move cursor to image";

    public override string ActionShortName => "Move to image";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/MoveToImage.png"));
    public override ActionCommand Create()
    {
        return new MoveToImage();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return MoveToImage.CreateFromJson(o);
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new MoveToImageConfigControl();
    }
}