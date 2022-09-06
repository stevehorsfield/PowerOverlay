#include "pch.h"

#include "MultimediaAudioInterop.h"
#include <Mmdeviceapi.h>
#include <endpointvolume.h>
#include <Functiondiscoverykeys_devpkey.h>
#include <propvarutil.h>


using System::Collections::Generic::List;

_COM_SMARTPTR_TYPEDEF(IMMDeviceEnumerator, __uuidof(IMMDeviceEnumerator));
_COM_SMARTPTR_TYPEDEF(IMMDeviceCollection, __uuidof(IMMDeviceCollection));
_COM_SMARTPTR_TYPEDEF(IMMDevice, __uuidof(IMMDevice));
_COM_SMARTPTR_TYPEDEF(IMMEndpoint, __uuidof(IMMEndpoint));
_COM_SMARTPTR_TYPEDEF(IPropertyStore, __uuidof(IPropertyStore));
_COM_SMARTPTR_TYPEDEF(IAudioEndpointVolume, __uuidof(IAudioEndpointVolume));

namespace PowerOverlay {
	namespace Interop {

		const CLSID CLSID_MMDeviceEnumerator = __uuidof(MMDeviceEnumerator);
		const IID IID_IMMDeviceEnumerator = __uuidof(IMMDeviceEnumerator);
		const IID IID_IMMEndpoint = __uuidof(IMMEndpoint);
		const IID IID_IAudioEndpointVolume = __uuidof(IAudioEndpointVolume);

		System::Collections::Generic::List<AudioEndpointDescriptor^>^ EnumerateAudioEndpoints() {

			IMMDeviceEnumeratorPtr enumerator;
			IMMDeviceCollectionPtr deviceCollection;

			HRESULT hr = CoCreateInstance(
				CLSID_MMDeviceEnumerator, NULL,
				CLSCTX_ALL, IID_IMMDeviceEnumerator,
				(void**)&enumerator);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to create MMDeviceEnumerator");
			}

			hr = enumerator->EnumAudioEndpoints(EDataFlow::eAll,
				DEVICE_STATE_ACTIVE | DEVICE_STATE_UNPLUGGED, &deviceCollection);

			auto collection = gcnew List<AudioEndpointDescriptor^>();

			UINT deviceCount = 0;
			hr = deviceCollection->GetCount(&deviceCount);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to enumerate audio devices");
			}

			for (UINT i = 0; i < deviceCount; ++i) {
				auto desc = gcnew AudioEndpointDescriptor();

				IMMDevicePtr device;
				hr = deviceCollection->Item(i, &device);
				if (!SUCCEEDED(hr)) {
					throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access audio device in collection");
				}
				IPropertyStorePtr propertyStore;
				hr = device->OpenPropertyStore(STGM_READ, &propertyStore);
				if (!SUCCEEDED(hr)) {
					throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access audio device property store");
				}


				PROPVARIANT pVariant;
				PropVariantInit(&pVariant);

				{
					hr = propertyStore->GetValue(PKEY_DeviceInterface_FriendlyName, &pVariant);
					if (!SUCCEEDED(hr)) {
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access device interface friendly name");
					}

					PWSTR text = NULL;

					hr = PropVariantToStringAlloc(pVariant, &text);
					if (!SUCCEEDED(hr)) {
						PropVariantClear(&pVariant);
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to retrieve device interface friendly name");
					}

					desc->InterfaceFriendlyName = gcnew System::String(text);

					CoTaskMemFree(text);
					PropVariantClear(&pVariant);
				}

				{
					hr = propertyStore->GetValue(PKEY_Device_FriendlyName, &pVariant);
					if (!SUCCEEDED(hr)) {
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access device friendly name");
					}

					PWSTR text = NULL;

					hr = PropVariantToStringAlloc(pVariant, &text);
					if (!SUCCEEDED(hr)) {
						PropVariantClear(&pVariant);
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to retrieve device friendly name");
					}

					desc->EndpointFriendlyName = gcnew System::String(text);

					CoTaskMemFree(text);
					PropVariantClear(&pVariant);
				}

				{
					LPWSTR deviceId = NULL;
					hr = device->GetId(&deviceId);
					if (!SUCCEEDED(hr)) {
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access device ID");
					}

					desc->EndpointID = gcnew System::String(deviceId);

					CoTaskMemFree(deviceId);
				}

				DWORD deviceState = 0UL;
				hr = device->GetState(&deviceState);
				if (!SUCCEEDED(hr)) {
					throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to get audio device state");
				}
				desc->IsActive = deviceState == DEVICE_STATE_ACTIVE;
				desc->IsUnplugged = deviceState == DEVICE_STATE_UNPLUGGED;
				desc->IsNotPresent = deviceState == DEVICE_STATE_NOTPRESENT;
				desc->IsDisabled = deviceState == DEVICE_STATE_DISABLED;

				IMMEndpointPtr endpoint;
				hr = device.QueryInterface(IID_IMMEndpoint, &endpoint);
				if (!SUCCEEDED(hr)) {
					throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access IMMEndpoint for device");
				}
				EDataFlow flow = EDataFlow::eRender;
				
				hr = endpoint->GetDataFlow(&flow);
				if (!SUCCEEDED(hr)) {
					throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to retrieve device flow direction");
				}

				desc->IsAudioInput = flow == EDataFlow::eAll || flow == EDataFlow::eCapture;
				desc->IsAudioOutput = flow == EDataFlow::eAll || flow == EDataFlow::eRender;

				if (deviceState == DEVICE_STATE_ACTIVE) {

					IAudioEndpointVolumePtr volume;
					hr = device->Activate(IID_IAudioEndpointVolume, CLSCTX_ALL, NULL, (void**)&volume);
					if (!SUCCEEDED(hr)) {
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Failed to active IAudioEndpointVolume");
					}

					float level = 0.0;
					hr = volume->GetMasterVolumeLevelScalar(&level);
					if (!SUCCEEDED(hr)) {
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to retrieve master volume");
					}
					desc->NormalizedVolumeLevel = (double)level;

					BOOL mute = FALSE;
					hr = volume->GetMute(&mute);
					if (!SUCCEEDED(hr)) {
						throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to retrieve mute state");
					}
					desc->IsMuted = mute;
				}

				collection->Add(desc);
			}
			return collection;
		}

		DefaultAudioEndpoint^ DescribeDefaultAudioEndpoint(IMMDeviceEnumeratorPtr& enumerator, EDataFlow flow, ERole role)
		{
			auto descriptor = gcnew DefaultAudioEndpoint();
			descriptor->Direction = flow == EDataFlow::eCapture ?
				AudioEndpointDirection::Microphone : AudioEndpointDirection::Speaker;
			switch (role) {
			case ERole::eCommunications:
				descriptor->Role = AudioEndpointRole::Communications;
				break;
			case ERole::eConsole:
				descriptor->Role = AudioEndpointRole::Console;
				break;
			case ERole::eMultimedia:
				descriptor->Role = AudioEndpointRole::Multimedia;
				break;
			}

			HRESULT hr;
			IMMDevicePtr device;
			hr = enumerator->GetDefaultAudioEndpoint(flow, role, &device);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Failed to access default audio endpoint device");
			}

			LPWSTR text = NULL;
			hr = device->GetId(&text);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Failed to access default audio endpoint device ID");
			}
			descriptor->EndpointID = gcnew System::String(text);
			CoTaskMemFree(text);

			return descriptor;
		}

		System::Collections::Generic::List<DefaultAudioEndpoint^>^ EnumerateDefaultAudioEndpoints()
		{
			IMMDeviceEnumeratorPtr enumerator;
			HRESULT hr = CoCreateInstance(
				CLSID_MMDeviceEnumerator, NULL,
				CLSCTX_ALL, IID_IMMDeviceEnumerator,
				(void**)&enumerator);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to create MMDeviceEnumerator");
			}

			auto collection = gcnew List<DefaultAudioEndpoint^>();
			
			collection->Add(DescribeDefaultAudioEndpoint(enumerator, EDataFlow::eRender, ERole::eConsole));
			collection->Add(DescribeDefaultAudioEndpoint(enumerator, EDataFlow::eRender, ERole::eMultimedia));
			collection->Add(DescribeDefaultAudioEndpoint(enumerator, EDataFlow::eRender, ERole::eCommunications));

			collection->Add(DescribeDefaultAudioEndpoint(enumerator, EDataFlow::eCapture, ERole::eConsole));
			collection->Add(DescribeDefaultAudioEndpoint(enumerator, EDataFlow::eCapture, ERole::eMultimedia));
			collection->Add(DescribeDefaultAudioEndpoint(enumerator, EDataFlow::eCapture, ERole::eCommunications));

			return collection;
		}

		void AudioEndpointSetMute(String^ endpointID, bool mute) {
			if (String::IsNullOrEmpty(endpointID)) throw gcnew ArgumentException(L"endpointID");

			IMMDeviceEnumeratorPtr enumerator;
			HRESULT hr = CoCreateInstance(
				CLSID_MMDeviceEnumerator, NULL,
				CLSCTX_ALL, IID_IMMDeviceEnumerator,
				(void**)&enumerator);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to create MMDeviceEnumerator");
			}

			pin_ptr<const wchar_t> eid = PtrToStringChars(endpointID);

			IMMDevicePtr device;
			hr = enumerator->GetDevice(eid, &device);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access specified device");
			}

			IAudioEndpointVolumePtr pVolume;
			hr = device->Activate(IID_IAudioEndpointVolume, CLSCTX_ALL, NULL, (void**)&pVolume);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access IAudioEndpointVolume for device");
			}
			hr = pVolume->SetMute(mute ? TRUE : FALSE, NULL);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to set mute for device");
			}
		}

		void AudioEndpointSetVolume(String^ endpointID, double volume) {
			if (! Double::IsNormal(volume)) throw gcnew ArgumentOutOfRangeException(L"volume");
			if (String::IsNullOrEmpty(endpointID)) throw gcnew ArgumentException(L"endpointID");

			if (volume > 1.0L) volume = 1.0L;
			if (volume < 0.0L) volume = 0.0L;
			
			float targetVolume = (float)volume;

			IMMDeviceEnumeratorPtr enumerator;
			HRESULT hr = CoCreateInstance(
				CLSID_MMDeviceEnumerator, NULL,
				CLSCTX_ALL, IID_IMMDeviceEnumerator,
				(void**)&enumerator);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to create MMDeviceEnumerator");
			}


			pin_ptr<const wchar_t> eid = PtrToStringChars(endpointID);

			IMMDevicePtr device;
			hr = enumerator->GetDevice(eid, &device);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access specified device");
			}

			IAudioEndpointVolumePtr pVolume;
			hr = device->Activate(IID_IAudioEndpointVolume, CLSCTX_ALL, NULL, (void**)&pVolume);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to access IAudioEndpointVolume for device");
			}
			hr = pVolume->SetMasterVolumeLevelScalar(targetVolume, NULL);
			if (!SUCCEEDED(hr)) {
				throw gcnew System::ComponentModel::Win32Exception(hr, L"Unable to set master volume for device");
			}
		}

	}
}