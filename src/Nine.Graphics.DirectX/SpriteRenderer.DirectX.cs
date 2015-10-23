using SharpDX.DXGI;

namespace Nine.Graphics.DirectX
{
    using System;
    using SharpDX.Direct3D12;
    using System.Numerics;

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

float4x4 transform;

PS_IN main(float2 position : POSITION, float4 color : COLOR, float2 uv : TEXCOORD)
{
    PS_IN output = (PS_IN)0;

    output.uv = uv;
    output.color = color;
    output.position = float4(position, 0, 1); //mul(float4(position, 0, 1), transform);
	
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

        private readonly GraphicsHost graphicsHost;

        public SpriteRenderer(GraphicsHost graphicsHost, TextureFactory textureFactory, QuadListIndexBuffer quadIndexBuffer, int initialSpriteCapacity = 1024)
        {
            if (graphicsHost == null) throw new ArgumentNullException(nameof(graphicsHost));
            if (textureFactory == null) throw new ArgumentNullException(nameof(textureFactory));
            if (quadIndexBuffer == null) throw new ArgumentNullException(nameof(quadIndexBuffer));

            this.graphicsHost = graphicsHost;
            this.textureFactory = textureFactory;
            this.quadIndexBuffer = quadIndexBuffer;
            this.CreateBuffers(initialSpriteCapacity);
            this.PlatformCreateBuffers();
            this.PlatformCreateShaders();
        }

        private void PlatformCreateBuffers()
        {
            var triangleVertices = new[]
            {
                new Vertex() {Position=new Vector2(10, 10), Color=Color.Red.Bgra, TextureCoordinate=Vector2.Zero },
                new Vertex() {Position=new Vector2(10, 100), Color=Color.Red.Bgra, TextureCoordinate=Vector2.Zero },
                new Vertex() {Position=new Vector2(100, 100), Color=Color.Red.Bgra, TextureCoordinate=Vector2.Zero },
            };

            int vertexBufferSize = triangleVertices.Length * Vertex.SizeInBytes;

            // Note: using upload heaps to transfer static data like vert buffers is not 
            // recommended. Every time the GPU needs it, the upload heap will be marshalled 
            // over. Please read up on Default Heap usage. An upload heap is used here for 
            // code simplicity and because there are very few verts to actually transfer.
            vertexBuffer = graphicsHost.Device.CreateCommittedResource(new HeapProperties(HeapType.Upload), HeapFlags.None, ResourceDescription.Buffer(vertexBufferSize), ResourceStates.GenericRead);

            IntPtr pVertexDataBegin = vertexBuffer.Map(0);
            SharpDX.Utilities.Write(pVertexDataBegin, triangleVertices, 0, triangleVertices.Length);
            vertexBuffer.Unmap(0);

            // Initialize the vertex buffer view.
            vertexBufferView = new VertexBufferView();
            vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
            vertexBufferView.StrideInBytes = Vertex.SizeInBytes;
            vertexBufferView.SizeInBytes = vertexBufferSize;
        }

        private void PlatformCreateShaders()
        {
            // Compile shaders
            byte[] compiledpixelShader = null;
            byte[] compiledvertexShader = null;

#if DEBUG
            var compileResultVS = SharpDX.D3DCompiler.ShaderBytecode.Compile(vertexShaderSource, "main", "vs_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug);

            if (compileResultVS.HasErrors)
                throw new ArgumentNullException(nameof(compileResultVS));

            compiledvertexShader = compileResultVS.Bytecode.Data;
#else
            compiledvertexShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(vertexShaderSource, "VSMain", "vs_5_0").Bytecode;
#endif

#if DEBUG
            var compileResultPS = SharpDX.D3DCompiler.ShaderBytecode.Compile(pixelShaderSource, "main", "ps_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug);

            if (compileResultPS.HasErrors)
                throw new ArgumentNullException(nameof(compileResultVS));

            compiledpixelShader = compileResultPS.Bytecode.Data;
#else
            compiledpixelShader = SharpDX.D3DCompiler.ShaderBytecode.Compile(pixelShaderSource, "PSMain", "ps_5_0").Bytecode;
#endif

            var vertexShader = new SharpDX.Direct3D12.ShaderBytecode(compiledvertexShader);
            var pixelShader = new SharpDX.Direct3D12.ShaderBytecode(compiledpixelShader);

            // Define the vertex input layout.
            var inputElementDescs = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float,   0,     0),
                new InputElement("COLOR",    0, Format.R8G8B8A8_UNorm, 8,     0), 
                new InputElement("TEXCOORD", 0, Format.R32G32_Float,   8 + 4, 0)
            };

            // Describe and create the graphics pipeline state object (PSO).
            var psoDesc = new GraphicsPipelineStateDescription()
            {
                InputLayout = new InputLayoutDescription(inputElementDescs),
                RootSignature = graphicsHost.RootSignature,
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

            pipelineState = graphicsHost.Device.CreateGraphicsPipelineState(psoDesc);
        }

        private unsafe void PlatformBeginDraw(ref Matrix4x4 projection)
        {
            graphicsHost.CommandList.Reset(graphicsHost.CommandAllocator, pipelineState);

            // TODO: projection
            // TODO: Update buffers

            graphicsHost.CommandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            graphicsHost.CommandList.SetVertexBuffer(0, vertexBufferView);
        }

        private unsafe void PlatformDraw(Vertex* pVertex, int vertexCount, Resource texture, bool isTransparent)
        {
            // TODO: texture

            graphicsHost.CommandList.DrawInstanced(vertexCount, 1, 0, 0);
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
