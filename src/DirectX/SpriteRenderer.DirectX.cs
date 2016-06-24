using SharpDX.DXGI;

namespace Nine.Graphics.DirectX
{
    using System;
    using SharpDX.Direct3D12;
    using System.Numerics;
    using System.Runtime.InteropServices;

    partial class SpriteRenderer
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

        private readonly GraphicsHost graphicsHost;
        
        public SpriteRenderer(GraphicsHost graphicsHost, TextureFactory textureFactory, QuadListIndexBuffer quadIndexBuffer, int initialSpriteCapacity = 1024)
        {
            if (graphicsHost == null) throw new ArgumentNullException(nameof(graphicsHost));
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));
            if (quadIndexBuffer == null) throw new ArgumentNullException(nameof(quadIndexBuffer));

            this.graphicsHost = graphicsHost;
            this.textureFactory = textureFactory;
            this.quadIndexBuffer = quadIndexBuffer;

            this.constantBufferData = new ConstantBuffer();

            this.CreateBuffers(initialSpriteCapacity);
            this.PlatformCreateBuffers();
            this.PlatformCreateShaders();
        }

        private void PlatformCreateBuffers()
        {
            // Create a upload resource, we are going to update the resource every 
            // frame so we just upload it every time, instead of doing extra copies.
            vertexBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, 
                ResourceDescription.Buffer(vertexData.Length * Vertex.SizeInBytes), ResourceStates.GenericRead);
            vertexBuffer.Name = "[SpriteRenderer] Vertex Buffer";

            // Create the view.
            vertexBufferView = new VertexBufferView();
            vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
            vertexBufferView.StrideInBytes = Vertex.SizeInBytes;
            vertexBufferView.SizeInBytes = vertexData.Length * Vertex.SizeInBytes;
            
            //// Describe and create a SRV for the texture.
            //var srvDesc = new ShaderResourceViewDescription
            //{
            //    Shader4ComponentMapping = DXHelper.DefaultComponentMapping(),
            //    Format = textureDesc.Format,
            //    Dimension = ShaderResourceViewDimension.Texture2D,
            //    Texture2D = { MipLevels = 1 },
            //};
            //
            //graphicsHost.Device.CreateShaderResourceView(this.texture, srvDesc, graphicsHost.SRVHeap.CPUDescriptorHandleForHeapStart);
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
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = new DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                Flags = PipelineStateFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                StreamOutput = new StreamOutputDescription()
            };
            psoDesc.RenderTargetFormats[0] = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            
            pipelineState = graphicsHost.Device.CreateGraphicsPipelineState(psoDesc);

            // TODO: Move buffer
            var constantBufferDesc = ResourceDescription.Buffer(1024 * 64);
            constantBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, constantBufferDesc, ResourceStates.GenericRead);
            constantBuffer.Name = "[SpriteRenderer] Constant Buffer";

            var cbvDesc = new ConstantBufferViewDescription()
            {
                BufferLocation = constantBuffer.GPUVirtualAddress,
                SizeInBytes = (SharpDX.Utilities.SizeOf<ConstantBuffer>() + 255) & ~255,
            };
            graphicsHost.Device.CreateConstantBufferView(cbvDesc, graphicsHost.CBVHeap.CPUDescriptorHandleForHeapStart);

            mappedConstantBuffer = constantBuffer.Map(0);
            SharpDX.Utilities.Write(mappedConstantBuffer, ref constantBufferData);
        }

        private unsafe void PlatformBeginDraw(ref Matrix4x4 projection)
        {
            // Update constant buffer resource.
            constantBufferData.transform = projection;
            SharpDX.Utilities.Write(mappedConstantBuffer, ref constantBufferData);

            // Request a new bundle.
            commandList = graphicsHost.RequestBundle(pipelineState);

            // Record the commands.
            commandList.SetGraphicsRootDescriptorTable(0, graphicsHost.CBVHeap.GPUDescriptorHandleForHeapStart);
            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

        private unsafe void PlatformDraw(Vertex* pVertex, int vertexCount, DXTexture texture, bool isTransparent)
        {
            // TODO: texture, isTransparent

            commandList.SetGraphicsRootDescriptorTable(1, graphicsHost.SRVHeap.GPUDescriptorHandleForHeapStart);

            // Update the vertex buffer
            IntPtr dataBegin = vertexBuffer.Map(0);
            SharpDX.Utilities.CopyMemory(dataBegin, new IntPtr(pVertex), vertexCount * Vertex.SizeInBytes);
            vertexBuffer.Unmap(0);

            // Record the commands.
            commandList.SetVertexBuffer(0, vertexBufferView);
            commandList.SetIndexBuffer(quadIndexBuffer.indexBufferView);

            // Call draw.
            commandList.DrawIndexedInstanced(vertexCount / 4 * 6, 1, 0, 0, 0);
        }

        private void PlatformEndDraw()
        {
            // Close CommandList.
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
