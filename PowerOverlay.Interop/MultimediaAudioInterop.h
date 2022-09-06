#pragma once

using namespace System;

namespace PowerOverlay {
namespace Interop {

	public ref class AudioEndpointDescriptor
	{
	public:
		String^ InterfaceFriendlyName;
		String^ EndpointFriendlyName;
		String^ EndpointID;
		bool IsAudioOutput;
		bool IsAudioInput;
		bool IsActive;
		bool IsUnplugged;
		bool IsNotPresent;
		bool IsDisabled;

		// Only available when active
		double NormalizedVolumeLevel;
		bool IsMuted;
	};

	public enum class AudioEndpointDirection {
		Speaker = 0,
		Microphone = 1,
	};

	public enum class AudioEndpointRole {
		Console = 0,
		Multimedia = 1,
		Communications = 2,
	};

	public ref class DefaultAudioEndpoint {
	public:
		String^ EndpointID;
		AudioEndpointRole Role;
		AudioEndpointDirection Direction;
	};

	System::Collections::Generic::List<AudioEndpointDescriptor^>^ EnumerateAudioEndpoints();
	System::Collections::Generic::List<DefaultAudioEndpoint^>^ EnumerateDefaultAudioEndpoints();

	void AudioEndpointSetMute(String^ endpointID, bool mute);
	void AudioEndpointSetVolume(String^ endpointID, double volume);

	public ref class MultimediaAudioInterop
	{

	public:
		System::Collections::Generic::List<AudioEndpointDescriptor^>^ EnumerateAudioEndpoints() {
			return ::PowerOverlay::Interop::EnumerateAudioEndpoints();
		}

		System::Collections::Generic::List<DefaultAudioEndpoint^>^ EnumerateDefaultAudioEndpoints() {
			return ::PowerOverlay::Interop::EnumerateDefaultAudioEndpoints();
		}

		void SetMute(String^ endpointID, bool mute) {
			AudioEndpointSetMute(endpointID, mute);
		}

		void SetVolume(String^ endpointID, double volume) {
			AudioEndpointSetVolume(endpointID, volume);
		}

	};


}
}
