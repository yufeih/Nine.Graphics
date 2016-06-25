namespace Nine.Graphics.Rendering
{
    using System.Runtime.InteropServices;
    using Nine.Graphics.Content;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    public class DXTextureFactory : TextureFactory<DXTexture>
    {
        public DXTextureFactory(ITextureLoader loader, int capacity = 1024)
            : base(loader, capacity)
        { }

        public override Texture<DXTexture> CreateTexture(TextureContent data)
        {
            return null;

            //var textureDesc = ResourceDescription.Texture2D(Format.R8G8B8A8_UNorm, data.Width, data.Height);
            //var texture = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, textureDesc, ResourceStates.CopyDestination);
            
            //var textureUploadHeap = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, 
            //    ResourceDescription.Texture2D(Format.R8G8B8A8_UNorm, data.Width, data.Height), ResourceStates.GenericRead);
            
            //var handle = GCHandle.Alloc(data.Pixels, GCHandleType.Pinned);
            //var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data.Pixels, 0);
            //textureUploadHeap.WriteToSubresource(0, null, ptr, 4 * data.Width, data.Pixels.Length);
            //handle.Free();
            
            //var commandList = graphicsHost.RequestCommandList();
            //commandList.CopyTextureRegion(new TextureCopyLocation(texture, 0), 0, 0, 0, new TextureCopyLocation(textureUploadHeap, 0), null);
            //commandList.ResourceBarrierTransition(texture, ResourceStates.CopyDestination, ResourceStates.PixelShaderResource);
            //commandList.Close();
            
            //var srvDesc = new ShaderResourceViewDescription()
            //{
            //    Shader4ComponentMapping = DXHelper.DefaultComponentMapping(),
            //    Format = textureDesc.Format,
            //    Dimension = ShaderResourceViewDimension.Texture2D,
            //    Texture2D = { MipLevels = 1 },
            //};
            //graphicsHost.Device.CreateShaderResourceView(texture, srvDesc, graphicsHost.SRVHeap.CPUDescriptorHandleForHeapStart);

            //var platformTexture = new DXTexture()
            //{
            //    Resource = texture,
            //};
            
            //return new Texture<DXTexture>(platformTexture, data.Width, data.Height, data.Left, data.Right, data.Top, data.Bottom, data.IsTransparent);
        }
    }
}
