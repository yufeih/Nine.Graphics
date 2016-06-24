namespace Nine.Graphics.Rendering
{
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D12;
    using System;
    using System.Runtime.InteropServices;

    static class DXHelper
    {
        public static byte[] CompileShader(string shaderSource, string entryPoint, string profile)
        {
            byte[] compiled = null;

#if DEBUG
            var compileResult = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderSource, entryPoint, profile, ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            if (compileResult.HasErrors)
                throw new ArgumentNullException(nameof(compileResult));

            compiled = compileResult.Bytecode.Data;
#else
            var compileResult = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderSource, entryPoint, profile);
            if (compileResult.HasErrors)
                throw new ArgumentNullException(nameof(compileResult));

            compiled = compileResult.Bytecode;
#endif

            return compiled;
        }

        public static void BeginEvent(this GraphicsCommandList commandList, string message)
        {
            // TODO: message.Length
            IntPtr hMessage = Marshal.StringToHGlobalUni(message);
            commandList.BeginEvent(1, hMessage, message.Length);
            Marshal.FreeHGlobal(hMessage);
        }

        public static void SetMarker(this GraphicsCommandList commandList, string message)
        {
            // TODO: message.Length
            IntPtr hMessage = Marshal.StringToHGlobalUni(message);
            commandList.SetMarker(1, hMessage, message.Length);
            Marshal.FreeHGlobal(hMessage);
        }

        #region Texture

        public const int ComponentMappingMask = 0x7;
        public const int ComponentMappingShift = 3;
        public const int ComponentMappingAlwaysSetBitAvoidingZeromemMistakes = (1 << (ComponentMappingShift * 4));

        public static int ComponentMapping(int src0, int src1, int src2, int src3)
        {
            return ((((src0) & ComponentMappingMask) |
                   (((src1) & ComponentMappingMask) << ComponentMappingShift) |
                   (((src2) & ComponentMappingMask) << (ComponentMappingShift * 2)) |
                   (((src3) & ComponentMappingMask) << (ComponentMappingShift * 3)) |
                   ComponentMappingAlwaysSetBitAvoidingZeromemMistakes));
        }

        public static int DefaultComponentMapping()
        {
            return ComponentMapping(0, 1, 2, 3);
        }

        public static int ComponentMapping(int ComponentToExtract, int Mapping)
        {
            return ((Mapping >> (ComponentMappingShift * ComponentToExtract) & ComponentMappingMask));
        }

        #endregion
    }
}
