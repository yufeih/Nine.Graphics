using SharpDX.DXGI;

namespace Nine.Graphics.DirectX
{
    using Nine.Graphics.Content;
    using SharpDX.Direct3D12;
    using System.Runtime.InteropServices;

    partial class TextureFactory
    {
        private Texture PlatformCreateTexture(TextureContent data)
        {
            var graphicsHost = this.graphicsHost as Nine.Graphics.DirectX.GraphicsHost;
            if (graphicsHost == null) return null;

            var textureDesc = ResourceDescription.Texture2D(Format.R8G8B8A8_UNorm, data.Width, data.Height);
            var texture = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, textureDesc, ResourceStates.CopyDestination);

            var textureUploadHeap = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, 
                ResourceDescription.Texture2D(Format.R8G8B8A8_UNorm, data.Width, data.Height), ResourceStates.GenericRead);

            var handle = GCHandle.Alloc(data.Pixels, GCHandleType.Pinned);
            var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data.Pixels, 0);
            textureUploadHeap.WriteToSubresource(0, null, ptr, 4 * data.Width, data.Pixels.Length);
            handle.Free();

            var commandList = graphicsHost.RequestCommandList();
            commandList.CopyTextureRegion(new TextureCopyLocation(texture, 0), 0, 0, 0, new TextureCopyLocation(textureUploadHeap, 0), null);
            commandList.ResourceBarrierTransition(texture, ResourceStates.CopyDestination, ResourceStates.PixelShaderResource);
            commandList.Close();

            CpuDescriptorHandle CpuDescriptorHandle;
            graphicsHost.Device.CreateShaderResourceView(texture, null, CpuDescriptorHandle);

            DXTexture platformTexture = new DXTexture();
            platformTexture.Resource = texture;

            return new Texture(platformTexture, data.Width, data.Height, data.Left, data.Right, data.Top, data.Bottom, data.IsTransparent);
        }
    }
}
