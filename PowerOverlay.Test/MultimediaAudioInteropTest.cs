using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PowerOverlay.Interop;

namespace PowerOverlay.Test;

[TestFixture]
public class MultimediaAudioInteropTest
{
    [Test]
    public void CanCreateInteropClass()
    {
        Assert.NotNull(new MultimediaAudioInterop());
    }
}
