using SharpDX.DXGI;

namespace Nine.Graphics.DirectX
{
    using System.Diagnostics;
    using System;
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D12;
    using System.Numerics;

    partial class SpriteRenderer
    {
        private static readonly string structShaderSource = @"
struct VS_IN
{
	float3 in_position : POSITION;
    float4 in_color : COLOR;
    float2 in_uv : TEXCOORD;
};

struct PS_IN
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
	float2 uv : SV_POSITION;
};";

        private static readonly string vertexShaderSource = structShaderSource + @"

float4x4 transform;

PS_IN main(VS_IN input)
{
    PS_IN output = (PS_IN)0;

	output.uv = input.in_uv;
	output.color = input.in_color;
	output.position = mul(float4(in_position, 1), transform);
	
    return output;
}";

        static readonly string pixelShaderSource = structShaderSource + @"

//Texture2D Texture;
//SamplerState Sampler;

float4 main(PS_IN input) : SV_Target
{
    return input.color;// * Texture.Sample(Sampler, input.uv);
}";

        private PipelineState pipelineState;
        private Resource vertexBuffer;
        private VertexBufferView vertexBufferView;

        public SpriteRenderer(Device device, TextureFactory textureFactory, QuadListIndexBuffer quadIndexBuffer, int initialSpriteCapacity = 1024)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));
            if (quadIndexBuffer == null) throw new ArgumentNullException(nameof(quadIndexBuffer));

            this.textureFactory = textureFactory;
            this.quadIndexBuffer = quadIndexBuffer;
            this.CreateBuffers(initialSpriteCapacity);
            this.PlatformCreateBuffers(device);
            this.PlatformCreateShaders(device);
        }

        private void PlatformCreateBuffers(Device device)
        {
            int vertexBufferSize = vertexData.Length * Vertex.SizeInBytes;

            // Note: using upload heaps to transfer static data like vert buffers is not 
            // recommended. Every time the GPU needs it, the upload heap will be marshalled 
            // over. Please read up on Default Heap usage. An upload heap is used here for 
            // code simplicity and because there are very few verts to actually transfer.
            vertexBuffer = device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer(vertexBufferSize), ResourceStates.GenericRead);

            // Initialize the vertex buffer view.
            vertexBufferView = new VertexBufferView();
            vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
            vertexBufferView.StrideInBytes = Vertex.SizeInBytes;
            vertexBufferView.SizeInBytes = vertexBufferSize;
        }

        private void PlatformCreateShaders(Device device)
        {
            throw new NotImplementedException();

            // TODO: Need access to the host
            RootSignature rootSignature = null;

            // Compile shaders
            byte[] compiledpixelShader = null;
            byte[] compiledvertexShader = null;

#if DEBUG
            compiledvertexShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(vertexShaderSource, "main", "vs_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug).Bytecode;
#else
            compiledvertexShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(vertexShaderSource, "VSMain", "vs_5_0").Bytecode;
#endif

#if DEBUG
            compiledpixelShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(pixelShaderSource, "main", "ps_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug).Bytecode;
#else
            compiledpixelShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(pixelShaderSource, "PSMain", "ps_5_0").Bytecode;
#endif

            var vertexShader = new SharpDX.Direct3D12.ShaderBytecode(compiledvertexShader);
            var pixelShader = new SharpDX.Direct3D12.ShaderBytecode(compiledpixelShader);

            // Define the vertex input layout.
            var inputElementDescs = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0), 
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, 0)
            };

            // Describe and create the graphics pipeline state object (PSO).
            var psoDesc = new GraphicsPipelineStateDescription()
            {
                InputLayout = new InputLayoutDescription(inputElementDescs),
                RootSignature = rootSignature,
                VertexShader = vertexShader,
                PixelShader = pixelShader,
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

            pipelineState = device.CreateGraphicsPipelineState(psoDesc);
        }

        private unsafe void PlatformBeginDraw(ref Matrix4x4 projection)
        {
            // TODO: Vertex* to Vertex[]
            //// Copy the triangle data to the vertex buffer.
            //IntPtr pVertexDataBegin = vertexBuffer.Map(0);
            //SharpDX.Utilities.Write(pVertexDataBegin, arrayVertex, 0, vertexCount);
            //vertexBuffer.Unmap(0);

            // TODO: Need access to the host
            //commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            //commandList.SetVertexBuffer(0, vertexBufferView);
        }

        private unsafe void PlatformDraw(Vertex* pVertex, int vertexCount, Resource texture, bool isTransparent)
        {
            // TODO: Need access to the host
            //commandList.DrawInstanced(vertexCount, 1, 0, 0);
        }

        private void PlatformEndDraw()
        {

        }

        private void PlatformDispose()
        {
            pipelineState.Dispose();
            vertexBuffer.Dispose();
        }
    }
}
