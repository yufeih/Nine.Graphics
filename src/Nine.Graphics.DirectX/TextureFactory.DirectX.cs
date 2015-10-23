using SharpDX.DXGI;

namespace Nine.Graphics.DirectX
{
    using Nine.Graphics.Content;
    using SharpDX.Direct3D12;
    using System.Runtime.InteropServices;

    partial class TextureFactory
    {
        private GraphicsHost graphicsHost = null;

        private Texture PlatformCreateTexture(TextureContent data)
        {
            // TODO: Access GraphicsHost

            //var pixelSize = sizeof(byte) * 4;
            //
            //var textureDesc = ResourceDescription.Texture2D(Format.R8G8B8A8_UNorm, data.Width, data.Height);
            //var texture = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, textureDesc, ResourceStates.CopyDestination);
            //
            //long uploadBufferSize = GetRequiredIntermediateSize(texture, 0, 1);
            //
            //// Create the GPU upload buffer.
            //var textureUploadHeap = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, 
            //    ResourceDescription.Texture2D(Format.R8G8B8A8_UNorm, data.Width, data.Height), ResourceStates.GenericRead);
            //
            //// Copy data to the intermediate upload heap and then schedule a copy 
            //// from the upload heap to the Texture2D.
            //var handle = GCHandle.Alloc(data.Pixels, GCHandleType.Pinned);
            //var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data.Pixels, 0);
            //textureUploadHeap.WriteToSubresource(0, null, ptr, pixelSize * data.Width, data.Pixels.Length);
            //handle.Free();
            //
            //graphicsHost.CommandList.CopyTextureRegion(new TextureCopyLocation(texture, 0), 0, 0, 0, new TextureCopyLocation(textureUploadHeap, 0), null);
            //graphicsHost.CommandList.ResourceBarrierTransition(texture, ResourceStates.CopyDestination, ResourceStates.PixelShaderResource);

            return new Texture(null, data.Width, data.Height, data.Left, data.Right, data.Top, data.Bottom, data.IsTransparent);
        }

        private long GetRequiredIntermediateSize(Resource destinationResource, int firstSubresource, int NumSubresources)
        {
            var desc = destinationResource.Description;
            long requiredSize;
            graphicsHost.Device.GetCopyableFootprints(ref desc, firstSubresource, NumSubresources, 0, null, null, null, out requiredSize);
            return requiredSize;
        }
    }
}
