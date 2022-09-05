namespace PowerOverlay.Test;

[TestFixture]
public class ActiveAppTest
{
    private IntPtr hwnd;

    [SetUp]
    public void Setup()
    {
        this.hwnd = PowerOverlay.NativeUtils.GetActiveAppHwnd();
    }

    [TearDown]
    public void TearDown()
    {
        this.hwnd = IntPtr.Zero;
    }

    [Test]
    public void CurrentAppHwndIsNotNull()
    {
        Assert.AreNotEqual(IntPtr.Zero, this.hwnd);
    }

    [Test]
    public void CurrentAppTitleIsNotEmpty()
    {
        Assert.IsNotEmpty(PowerOverlay.NativeUtils.GetWindowTitle(this.hwnd));
    }
}