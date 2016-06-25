namespace Nine.Graphics.Rendering
{
    public struct DXTexture
    {
        public SharpDX.Direct3D12.Resource Resource;

        public SharpDX.Direct3D12.GpuDescriptorHandle GpuDescriptorHandle;
        public SharpDX.Direct3D12.CpuDescriptorHandle CpuDescriptorHandle;

        public static bool operator ==(DXTexture r, DXTexture l)
        {
            return r.Resource == l.Resource;
        }

        public static bool operator !=(DXTexture r, DXTexture l)
        {
            return r.Resource != l.Resource;
        }
    }
}