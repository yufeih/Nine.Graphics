namespace Nine.Graphics.DirectX
{
    using SharpDX.Direct3D12;
    using System.Diagnostics;
    using Factory4 = SharpDX.DXGI.Factory4;

    static class DXDebug
    {
        /// <returns> Is Valid </returns>
        public static bool ValidateDevice(Device device)
        {
            // The D3D Device is no longer valid if the default adapter changed since the device
            // was created or if the device has been removed.

            // First, get the LUID for the adapter from when the device was created.
            var previousAdapterLuid = device.AdapterLuid;

            // Next, get the information for the current default adapter.
            using (var factory = new Factory4())
            {
                var currentDefaultAdapter = factory.Adapters[0];
                var currentDesc = currentDefaultAdapter.Description;

                // If the adapter LUIDs don't match, or if the device reports that it has been removed,
                // a new D3D device must be created.
                if (previousAdapterLuid != currentDesc.Luid ||
                    device.DeviceRemovedReason.Failure)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
