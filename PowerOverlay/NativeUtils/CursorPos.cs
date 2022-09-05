using System.Drawing;

namespace PowerOverlay;

public partial class NativeUtils
{
    public static Point? GetCursorPosition()
    {
        var p = new tagPOINT();
        if (GetCursorPos(ref p) != 0)
        {
            var result = new Point();
            result.X = p.x;
            result.Y = p.y;
            return result;
        }
        return null;
    }
}