using ComethSDK.Scripts.Types;
using UnityEngine.Device;

namespace ComethSDK.Scripts.Services
{
	public static class DeviceService
	{
		public static DeviceData GetDeviceData()
		{
			return new DeviceData
			{
				browser = "",
				os = SystemInfo.operatingSystem,
				platform = Application.platform.ToString()
			};
		}
	}
}