using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    [Test]
    public void CanDescribeEndpoints()
    {
        try
        {
            var details = new MultimediaAudioInterop().EnumerateAudioEndpoints();
            Assert.NotNull(details);
            details.ForEach(x =>
            {
                Console.WriteLine(
                    $"{x.InterfaceFriendlyName},{x.EndpointFriendlyName},{x.EndpointID},vol:{x.NormalizedVolumeLevel},mute:{x.IsMuted},input:{x.IsAudioInput},output:{x.IsAudioOutput},active:{x.IsActive},unplugged:{x.IsUnplugged},disabled:{x.IsDisabled},not present:{x.IsNotPresent}"
                    );
            }
                );
        }
        catch (Win32Exception e)
        {
            Console.WriteLine(e.ErrorCode);
            throw;
        }
    }

    [Test]
    public void CanDescribeDefaultEndpoints()
    {
        try
        {
            var details = new MultimediaAudioInterop().EnumerateDefaultAudioEndpoints();
            Assert.NotNull(details);
            details.ForEach(x =>
            {
                Console.WriteLine(
                    $"{x.Direction},{x.Role},{x.EndpointID}"
                    );
            }
                );
        }
        catch (Win32Exception e)
        {
            Console.WriteLine(e.ErrorCode);
            throw;
        }
    }

    [Test]
    public void CanSetSpeakerMute()
    {
        try
        {
            var interop = new MultimediaAudioInterop();
            var endpointID = interop.EnumerateDefaultAudioEndpoints().First(x => x.Direction == AudioEndpointDirection.Speaker && x.Role == AudioEndpointRole.Multimedia).EndpointID;
            var state = interop.EnumerateAudioEndpoints().First(x => x.EndpointID.Equals(endpointID, StringComparison.Ordinal));

            var isMuted = state.IsMuted;

            interop.SetMute(endpointID, !isMuted);
            try
            {
                var newState = interop.EnumerateAudioEndpoints().First(x => x.EndpointID.Equals(endpointID, StringComparison.Ordinal));

                Assert.AreNotEqual(isMuted, newState.IsMuted);
            }
            finally
            {
                interop.SetMute(endpointID, isMuted);
            }
        }
        catch (Win32Exception e)
        {
            Console.WriteLine(e.ErrorCode);
            throw;
        }
    }

    [Test]
    public void CanSetMicrophoneMute()
    {
        try
        {
            var interop = new MultimediaAudioInterop();
            var endpointID = interop.EnumerateDefaultAudioEndpoints().First(x => x.Direction == AudioEndpointDirection.Microphone&& x.Role == AudioEndpointRole.Multimedia).EndpointID;
            var state = interop.EnumerateAudioEndpoints().First(x => x.EndpointID.Equals(endpointID, StringComparison.Ordinal));

            var isMuted = state.IsMuted;

            interop.SetMute(endpointID, !isMuted);
            try
            {
                var newState = interop.EnumerateAudioEndpoints().First(x => x.EndpointID.Equals(endpointID, StringComparison.Ordinal));

                Assert.AreNotEqual(isMuted, newState.IsMuted);
            }
            finally
            {
                interop.SetMute(endpointID, isMuted);
            }
        }
        catch (Win32Exception e)
        {
            Console.WriteLine(e.ErrorCode);
            throw;
        }
    }

    [Test]
    public void CanSetSpeakerVolume()
    {
        try
        {
            var interop = new MultimediaAudioInterop();
            var endpointID = interop.EnumerateDefaultAudioEndpoints().First(x => x.Direction == AudioEndpointDirection.Speaker && x.Role == AudioEndpointRole.Multimedia).EndpointID;
            var state = interop.EnumerateAudioEndpoints().First(x => x.EndpointID.Equals(endpointID, StringComparison.Ordinal));

            var volume = state.NormalizedVolumeLevel;
            var newVolume = volume == 0.95 ? 0.05 : 0.95;

            interop.SetVolume(endpointID, newVolume);
            try
            {
                var newState = interop.EnumerateAudioEndpoints().First(x => x.EndpointID.Equals(endpointID, StringComparison.Ordinal));

                Assert.AreNotEqual(volume, newState.NormalizedVolumeLevel);
            }
            finally
            {
                interop.SetVolume(endpointID, volume);
            }
        }
        catch (Win32Exception e)
        {
            Console.WriteLine(e.ErrorCode);
            throw;
        }
    }
}
