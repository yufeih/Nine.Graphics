namespace Nine.Graphics.Rendering.DirectX
{
    using System.Diagnostics;
    using System;
    using Nine.Graphics.Content.DirectX;
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;

    // TODO: DirectX requires access to the device

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

        //private VertexShader vertexShader;
        //private PixelShader pixelShader;
        
        //private BufferDescription vertexBufferDesc;
        //private Buffer vertexBuffer;

        private void PlatformCreateBuffers()
        {
            //this.vertexBufferDesc = new BufferDescription(1024, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            //this.vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, this.vertexBufferDesc);
        }

        private void PlatformCreateShaders()
        {
            //var vertexShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.Compile(vertexShaderSource, "main", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            //this.vertexShader = new VertexShader(device, vertexShaderByteCode);
            //
            //var pixelShaderByteCode = SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(pixelShaderSource, "main", "ps_4_0", ShaderFlags.None, EffectFlags.None);
            //this.pixelShader = new PixelShader(device, pixelShaderByteCode);
            //
            //var layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode),
            //    new[]
            //    {
            //        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            //        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0), // TODO: I am not sure what type Color is (R8G8B8A8_UNORM??)
            //        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
            //    });
            //
            //vertexShaderByteCode.Dispose();
            //pixelShaderByteCode.Dispose();
        }

        private unsafe void PlatformDraw(Vertex* pVertex, ushort* pIndex, int vertexCount, int indexCount, Texture texture)
        {
            //context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 32, 0));
        }

        private void PlatformDispose()
        {
            //vertexShader.Dispose();
            //pixelShader.Dispose();
            //vertexBuffer.Dispose();
        }
    }
}
