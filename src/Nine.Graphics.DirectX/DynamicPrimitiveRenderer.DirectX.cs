using SharpDX.DXGI;

namespace Nine.Graphics.DirectX
{
    using SharpDX.Direct3D12;
    using System;
    using System.Numerics;

    partial class DynamicPrimitiveRenderer
    {
        private static readonly string structShaderSource = @"
struct PS_IN
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 uv : TEXCOORD;
};";
        private static readonly string vertexShaderSource = structShaderSource + @"

cbuffer ConstantBuffer : register(b0)
{
	row_major float4x4 transform;
};

PS_IN main(float4 position : POSITION, float4 color : COLOR, float2 uv : TEXCOORD)
{
    PS_IN output = (PS_IN)0;

    output.uv = uv;
    output.color = color;
    output.position = mul(position, transform); 
	
    return output;
}";
        static readonly string pixelShaderSource = structShaderSource + @"

Texture2D g_texture : register(t0);
SamplerState g_sampler : register(s0);

float4 main(PS_IN input) : SV_Target
{
    return g_texture.Sample(g_sampler, input.uv) * input.color;
}";

        struct ConstantBuffer
        {
            public Matrix4x4 transform;
        }

        private PipelineState pipelineState;
        private GraphicsCommandList commandList;

        private ConstantBuffer constantBufferData;
        private Resource constantBuffer;
        private IntPtr mappedConstantBuffer;

        private Resource vertexBuffer;
        private VertexBufferView vertexBufferView;

        private Resource indexBuffer;
        private IndexBufferView indexBufferView;

        private readonly GraphicsHost graphicsHost;

        public DynamicPrimitiveRenderer(GraphicsHost graphicsHost, TextureFactory textureFactory, int initialBufferCapacity = 32)
        {
            if (graphicsHost == null) throw new ArgumentNullException(nameof(graphicsHost));
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));

            this.graphicsHost = graphicsHost;
            this.textureFactory = textureFactory;

            this.PlatformCreateBuffers(initialBufferCapacity);
            this.PlatformCreateShaders();
        }

        private void PlatformCreateBuffers(int initialBufferCapacity)
        {
            this.vertexData = new Vertex[512];
            this.indexData = new ushort[512];

            vertexBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None,
                ResourceDescription.Buffer(vertexData.Length * Vertex.SizeInBytes), ResourceStates.GenericRead);
            vertexBuffer.Name = "[DynamicPrimitiveRenderer] Vertex Buffer";

            vertexBufferView = new VertexBufferView();
            vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
            vertexBufferView.StrideInBytes = Vertex.SizeInBytes;
            vertexBufferView.SizeInBytes = vertexData.Length * Vertex.SizeInBytes;

            indexBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None,
                ResourceDescription.Buffer(indexData.Length * sizeof(ushort)), ResourceStates.GenericRead);

            indexBuffer.Name = "[DynamicPrimitiveRenderer] Index Buffer";

            indexBufferView = new IndexBufferView();
            indexBufferView.BufferLocation = indexBuffer.GPUVirtualAddress;
            indexBufferView.SizeInBytes = indexData.Length * sizeof(ushort);
            indexBufferView.Format = SharpDX.DXGI.Format.R16_UInt;
        }

        private void PlatformCreateShaders()
        {
            var inputElementDescs = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float,   0,     0),
                new InputElement("COLOR",    0, Format.B8G8R8A8_UNorm, 8,     0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float,   8 + 4, 0)
            };

            var psoDesc = new GraphicsPipelineStateDescription()
            {
                InputLayout = new InputLayoutDescription(inputElementDescs),
                RootSignature = graphicsHost.RootSignature,
                VertexShader = DXHelper.CompileShader(vertexShaderSource, "main", "vs_5_0"),
                PixelShader = DXHelper.CompileShader(pixelShaderSource, "main", "ps_5_0"),
                RasterizerState = RasterizerStateDescription.Default(),
                BlendState = BlendStateDescription.Default(),
                DepthStencilFormat = Format.D32_Float,
                DepthStencilState = new DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                Flags = PipelineStateFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                StreamOutput = new StreamOutputDescription()
            };
            psoDesc.RenderTargetFormats[0] = SharpDX.DXGI.Format.R8G8B8A8_UNorm;

            pipelineState = graphicsHost.Device.CreateGraphicsPipelineState(psoDesc);

            // TODO: Move buffer
            var constantBufferDesc = ResourceDescription.Buffer(1024 * 64);
            constantBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, constantBufferDesc, ResourceStates.GenericRead);
            constantBuffer.Name = "[DynamicPrimitiveRenderer] Constant Buffer";

            var cbvDesc = new ConstantBufferViewDescription()
            {
                BufferLocation = constantBuffer.GPUVirtualAddress,
                SizeInBytes = (SharpDX.Utilities.SizeOf<ConstantBuffer>() + 255) & ~255,
            };
            graphicsHost.Device.CreateConstantBufferView(cbvDesc, graphicsHost.CBVHeap.CPUDescriptorHandleForHeapStart);

            mappedConstantBuffer = constantBuffer.Map(0);
            SharpDX.Utilities.Write(mappedConstantBuffer, ref constantBufferData);
        }

        private void PlatformUpdateBuffers()
        {
            IntPtr vertexDataBegin = vertexBuffer.Map(0);
            SharpDX.Utilities.Write(vertexDataBegin, vertexData, 0, currentBaseVertex + currentVertex);
            vertexBuffer.Unmap(0);

            IntPtr indexDataBegin = indexBuffer.Map(0);
            SharpDX.Utilities.Write(indexDataBegin, indexData, 0, currentBaseIndex + currentIndex);
            indexBuffer.Unmap(0);
        }

        private void PlatformBeginDraw(ref Matrix4x4 wvp)
        {
            constantBufferData.transform = wvp;
            SharpDX.Utilities.Write(mappedConstantBuffer, ref constantBufferData);

            commandList = graphicsHost.RequestBundle(pipelineState);

            commandList.SetGraphicsRootDescriptorTable(0, graphicsHost.CBVHeap.GPUDescriptorHandleForHeapStart);
            
            commandList.SetVertexBuffer(0, vertexBufferView);
            commandList.SetIndexBuffer(indexBufferView);
        }

        private void PlatformDrawBatch(PrimitiveGroupEntry entry)
        {
            commandList.PrimitiveTopology = DXHelper.ToDXPrimitiveType(entry.PrimitiveType);

            var texture = textureFactory.GetTexture(entry.Texture ?? TextureId.White);
            if (texture == null)
                return;

            // TODO: Bind texture

            if (entry.IndexCount > 0)
            {
                commandList.DrawIndexedInstanced(entry.IndexCount, 1, entry.StartIndex, 0, 0);
            }
            else
            {
                commandList.DrawInstanced(entry.VertexCount, 1, 0, 0);
            }
        }

        private void PlatformEndDraw()
        {
            commandList.Close();
        }

        private void PlatformDispose()
        {
            constantBuffer.Dispose();
            pipelineState.Dispose();
            vertexBuffer.Dispose();
        }
    }
}
