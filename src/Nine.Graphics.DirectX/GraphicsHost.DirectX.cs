using SharpDX.DXGI;
using System.Threading;
using System;

namespace Nine.Graphics.DirectX
{
    using SharpDX;
    using SharpDX.Direct3D12;
    using SharpDX.Mathematics.Interop;
    using SharpDX.Windows;
    using System.Runtime.CompilerServices;

    public class GraphicsHost : Rendering.IGraphicsHost
    {
        public IntPtr WindowHandle => window.Handle;

        public Device Device => device;
        public RootSignature RootSignature => rootSignature;
        public CommandAllocator CommandAllocator => commandAllocator;
        public GraphicsCommandList CommandList => commandList;
        public ViewportF Viewport => viewport;

        const int FrameCount = 2;

        // Pipeline Objects
        private Device device;
        private SwapChain3 swapChain;
        private readonly Resource[] renderTargets = new Resource[FrameCount];

        private RootSignature rootSignature;
        private CommandAllocator commandAllocator;
        private CommandQueue commandQueue;
        private DescriptorHeap renderTargetViewHeap;

        private GraphicsCommandList commandList;
        private int rtvDescriptorSize;

        // Synchronization Objects
        private int frameIndex;
        private AutoResetEvent fenceEvent;

        private Fence fence;
        private int fenceValue;

        // Window Objects
        private readonly RenderForm window;
        private RenderLoop renderLoop;

        private ViewportF viewport;
        private Rectangle scissorRect;

        public GraphicsHost(int width, int height, bool hidden = false) // FormBorderStyle = FormBorderStyle.FixedSingle
            : this(new RenderForm("Nine.Graphics") { Width = width, Height = height }, hidden)
        { }

        public GraphicsHost(RenderForm window, bool hidden = false)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            this.window = window;
            this.window.Visible = !hidden;

            this.renderLoop = new RenderLoop(this.window);

            int width = window.ClientSize.Width;
            int height = window.ClientSize.Height;

            viewport.Width = width;
            viewport.Height = height;
            viewport.MaxDepth = 1.0f;

            scissorRect.Right = width;
            scissorRect.Bottom = height;

            /// 
            /// Pipeline
            /// 

#if DEBUG
            Configuration.EnableObjectTracking = true;
            Configuration.ThrowOnShaderCompileError = false;

            // Enable the D3D12 debug layer.
            DebugInterface.Get().EnableDebugLayer();
#endif

            device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);
            using (var factory = new Factory4())
            {
                // Describe and create the command queue.
                CommandQueueDescription queueDesc = new CommandQueueDescription(CommandListType.Direct);
                commandQueue = device.CreateCommandQueue(queueDesc);


                // Describe and create the swap chain.
                SwapChainDescription swapChainDesc = new SwapChainDescription()
                {
                    BufferCount = FrameCount,
                    ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                    Usage = Usage.RenderTargetOutput,
                    SwapEffect = SwapEffect.FlipDiscard,
                    OutputHandle = window.Handle,
                    //Flags = SwapChainFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                    IsWindowed = true
                };

                SwapChain tempSwapChain = new SwapChain(factory, commandQueue, swapChainDesc);
                swapChain = tempSwapChain.QueryInterface<SwapChain3>();
                tempSwapChain.Dispose();
                frameIndex = swapChain.CurrentBackBufferIndex;
            }

            // Create descriptor heaps.
            // Describe and create a render target view (RTV) descriptor heap.
            DescriptorHeapDescription rtvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = FrameCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };

            renderTargetViewHeap = device.CreateDescriptorHeap(rtvHeapDesc);

            rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

            // Create frame resources.
            CpuDescriptorHandle rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            for (int n = 0; n < FrameCount; n++)
            {
                renderTargets[n] = swapChain.GetBackBuffer<Resource>(n);
                device.CreateRenderTargetView(renderTargets[n], null, rtvHandle);
                rtvHandle += rtvDescriptorSize;
            }

            commandAllocator = device.CreateCommandAllocator(CommandListType.Direct);

            /// 
            /// Assets
            /// 

            // Create an empty root signature.
            var rootSignatureDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout);
            rootSignature = device.CreateRootSignature(rootSignatureDesc.Serialize());

            // Create the command list.
            commandList = device.CreateCommandList(CommandListType.Direct, commandAllocator, null);

            // Command lists are created in the recording state, but there is nothing
            // to record yet. The main loop expects it to be closed, so close it now.
            commandList.Close();

            // Create synchronization objects.
            fence = device.CreateFence(0, FenceFlags.None);
            fenceValue = 1;

            // Create an event handle to use for frame synchronization.
            fenceEvent = new AutoResetEvent(false);
        }
        
        public bool DrawFrame(Action<int, int> draw, [CallerMemberName]string frameName = null)
        {
            if (!this.renderLoop.NextFrame())
                return false;

            // Command list allocators can only be reset when the associated 
            // command lists have finished execution on the GPU; apps should use 
            // fences to determine GPU execution progress.
            commandAllocator.Reset();

            // However, when ExecuteCommandList() is called on a particular command 
            // list, that command list can then be reset at any time and must be before 
            // re-recording.
            commandList.Reset(commandAllocator, null);

            // Indicate that the back buffer will be used as a render target.
            commandList.ResourceBarrierTransition(renderTargets[frameIndex], ResourceStates.Present, ResourceStates.RenderTarget);

            CpuDescriptorHandle rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            rtvHandle += frameIndex * rtvDescriptorSize;

            // Record commands.
            var clearColor = new RawColor4(Branding.Color.R / 255.0f, Branding.Color.G / 255.0f, Branding.Color.B / 255.0f, Branding.Color.A / 255.0f);
            commandList.ClearRenderTargetView(rtvHandle, clearColor, 0, null);

            draw(window.Width, window.Height);

            // Indicate that the back buffer will now be used to present.
            commandList.ResourceBarrierTransition(renderTargets[frameIndex], ResourceStates.RenderTarget, ResourceStates.Present);

            commandList.Close();

            // Execute the command list.
            commandQueue.ExecuteCommandList(commandList);

            // Present the frame.
            swapChain.Present(1, PresentFlags.None);

            WaitForPrevFrame();
            return true;
        }

        public Content.TextureContent GetTexture()
        {
            var buffer = swapChain.GetBackBuffer<Resource>(0);

            // TODO: Format?
            if (buffer.Description.Format == Format.R8G8B8A8_UNorm)
            {
                var width  = (int)buffer.Description.Width;
                var height = (int)buffer.Description.Height;

                // TODO: Get Data

                return new Content.TextureContent(width, height, null);
            }

            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Wait the previous command list to finish executing. 
        /// </summary>
        private void WaitForPrevFrame()
        {
            // WAITING FOR THE FRAME TO COMPLETE BEFORE CONTINUING IS NOT BEST PRACTICE. 
            // This is code implemented as such for simplicity. 

            int fence = fenceValue;
            commandQueue.Signal(this.fence, fence);
            fenceValue++;

            if (this.fence.CompletedValue < fence)
            {
                this.fence.SetEventOnCompletion(fence, fenceEvent.SafeWaitHandle.DangerousGetHandle());
                fenceEvent.WaitOne();
            }

            frameIndex = swapChain.CurrentBackBufferIndex;
        }

        public void Dispose()
        {
            WaitForPrevFrame();

            swapChain.SetFullscreenState(false, null);

            foreach (var target in renderTargets)
            {
                target.Dispose();
            }

            commandAllocator.Dispose();
            commandQueue.Dispose();
            rootSignature.Dispose();
            renderTargetViewHeap.Dispose();
            commandList.Dispose();
            fence.Dispose();
            swapChain.Dispose();
            device.Dispose();

            window?.Dispose();
        }
    }
}
