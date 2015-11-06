namespace Nine.Graphics.DirectX
{
    using SharpDX.Direct3D12;
    using System;

    partial class QuadListIndexBuffer
    {
        internal Resource indexBuffer = null;
        internal IndexBufferView indexBufferView;

        public void Apply()
        {
            if (indexBuffer == null)
            {
                var graphicsHost = this.graphicsHost as GraphicsHost;
                indexBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None,
                    ResourceDescription.Buffer(indexData.Length * sizeof(ushort)), ResourceStates.GenericRead);

                indexBuffer.Name = "[SpriteRenderer] Index Buffer";

                indexBufferView = new IndexBufferView();
                indexBufferView.BufferLocation = indexBuffer.GPUVirtualAddress;
                indexBufferView.SizeInBytes = indexData.Length * sizeof(ushort);
                indexBufferView.Format = SharpDX.DXGI.Format.R16_UInt;
            }

            IntPtr pIndexDataBegin = indexBuffer.Map(0);
            SharpDX.Utilities.Write(pIndexDataBegin, indexData, 0, indexData.Length);
            indexBuffer.Unmap(0);
        }
    }
}
