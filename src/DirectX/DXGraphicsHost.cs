using SharpDX.DXGI;
using System.Threading;
using System;

namespace Nine.Graphics.Rendering
{
    using Content;
    using SharpDX;
    using SharpDX.Direct3D12;
    using SharpDX.Mathematics.Interop;
    using SharpDX.Windows;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class GraphicsHost : Rendering.IGraphicsHost
    {
        public readonly int FrameCount = 1;


        public IntPtr WindowHandle => window.Handle;

        /// <summary> Gets the current graphics device. </summary>
        public Device Device => device;
        /// <summary> Gets the current swapchain. </summary>
        public SwapChain3 SwapChain => swapChain;
        /// <summary> Gets the front buffer render target. </summary>
        public Resource RenderTarget => renderTargets[currentFrame];
        /// <summary> Gets the command queue. </summary>
        public CommandQueue CommandQueue => commandQueue;
        /// <summary> Gets the command allocator. </summary>
        public CommandAllocator CommandAllocator => commandAllocators[currentFrame];
        /// <summary> Gets the viewport. </summary>
        public ViewportF Viewport => viewport;
        /// <summary> Gets the ScissorRect. </summary>
        public Rectangle ScissorRect => scissorRect;
        /// <summary> Gets the current frame index. </summary>
        public int CurrentFrameIndex => currentFrame;

        public RootSignature RootSignature => rootSignature;

        //public DescriptorHeap RTVHeap => rtvHeap;
        //public int RTVDescriptorSize => rtvDescriptorSize;

        public GraphicsCommandList RequestBundle(PipelineState initialState = null) => bundlePool.GetObject(initialState);
        private readonly GraphicsCommandListPool bundlePool;

        public GraphicsCommandList RequestCommandList() => commandListPool.GetObject(null);
        private readonly GraphicsCommandListPool commandListPool;

        // Pipeline Objects
        private Device device;
        private SwapChain3 swapChain;
        private Resource[] renderTargets;
        private CommandAllocator[] commandAllocators;

        private CommandQueue commandQueue;

        private GraphicsCommandList commandList;
        private CommandAllocator bundleAllocator;
        //private ObjectPool<GraphicsCommandList> commandListPool;

        private DescriptorHeap rtvHeap;
        private int rtvDescriptorSize;

        public DescriptorHeap CBVHeap => cbvHeap;
        public DescriptorHeap SRVHeap => srvHeap;

        private DescriptorHeap cbvHeap; // Constant Buffer View
        private DescriptorHeap srvHeap; // Shader Render View

        private RootSignature rootSignature;

        // Synchronization Objects
        private int currentFrame;
        private AutoResetEvent fenceEvent;

        private Fence fence;
        private int[] fenceValues;

        // Window Objects
        private readonly RenderForm window;
        private RenderLoop renderLoop;

        private ViewportF viewport;
        private Rectangle scissorRect;

        public GraphicsHost(int width, int height, bool hidden = false) 
            : this(new RenderForm("Nine.Graphics") { Width = width, Height = height }, hidden)
        { }

        public GraphicsHost(RenderForm window, bool hidden = false)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            this.window = window;
            this.window.Visible = !hidden;

            // Buffering
            this.FrameCount = 3; // Triple Buffering
            this.renderTargets = new Resource[FrameCount];
            this.commandAllocators = new CommandAllocator[FrameCount];
            this.fenceValues = new int[FrameCount];

            this.renderLoop = new RenderLoop(this.window);

            this.CreateDeviceResources();
            this.CreateWindowResources();
            
            commandList = Device.CreateCommandList(CommandListType.Direct, CommandAllocator, null);
            commandList.Name = $"Main CommandList";
            commandList.Close();
            
            bundleAllocator = device.CreateCommandAllocator(CommandListType.Bundle);
            bundlePool = new GraphicsCommandListPool(this, bundleAllocator, CommandListType.Bundle, "Bundle");
            commandListPool = new GraphicsCommandListPool(this, CommandAllocator, CommandListType.Direct);
        }
        
        public bool DrawFrame(Action<int, int> draw, [CallerMemberName]string frameName = null)
        {
            if (!this.renderLoop.NextFrame())
                return false;

            CommandAllocator.Reset();
            bundleAllocator.Reset();

            commandList.Reset(CommandAllocator, null);

            BeginFrame(commandList);

            commandList.SetViewport(viewport);
            commandList.SetScissorRectangles(scissorRect);

            var commandLists = commandListPool.ToArray();
            commandQueue.ExecuteCommandLists(commandLists.Length, commandLists);
            bundlePool.Reset();

            commandList.ResourceBarrierTransition(RenderTarget, ResourceStates.Present, ResourceStates.RenderTarget);

            CpuDescriptorHandle rtvHandle = rtvHeap.CPUDescriptorHandleForHeapStart;
            rtvHandle += CurrentFrameIndex * rtvDescriptorSize;
            commandList.SetRenderTargets(1, rtvHandle, false, null);

            var clearColor = new RawColor4(Branding.Color.R / 255.0f, Branding.Color.G / 255.0f, Branding.Color.B / 255.0f, Branding.Color.A / 255.0f);
            commandList.ClearRenderTargetView(rtvHandle, clearColor, 0, null);

            draw(window.Width, window.Height);

            foreach (var bundle in bundlePool)
            {
                commandList.ExecuteBundle(bundle);
            }

            bundlePool.Reset();

            commandList.ResourceBarrierTransition(RenderTarget, ResourceStates.RenderTarget, ResourceStates.Present);

            commandList.Close();

            commandQueue.ExecuteCommandList(commandList);

            swapChain.Present(1, PresentFlags.None);

            WaitForPrevFrame();

            return true;
        }

        public TextureContent GetTexture()
        {
            throw new NotImplementedException();
        }
        
        internal void BeginFrame(GraphicsCommandList commandList)
        {
            var heaps = new DescriptorHeap[] { cbvHeap };

            commandList.SetGraphicsRootSignature(rootSignature);
            commandList.SetDescriptorHeaps(heaps.Length, heaps);
        }

        /// <summary> Wait the previous command list to finish executing. </summary>
        private void WaitForPrevFrame()
        {
            // Schedule a Signal command in the queue.
            int currentFenceValue = fenceValues[currentFrame];
            commandQueue.Signal(fence, currentFenceValue);

            // Advance the frame index.
            currentFrame = (currentFrame + 1) % FrameCount;

            // Check to see if the next frame is ready to start.
            if (fence.CompletedValue < fenceValues[currentFrame])
            {
                fence.SetEventOnCompletion(fenceValues[currentFrame], fenceEvent.SafeWaitHandle.DangerousGetHandle());
                fenceEvent.WaitOne();
            }

            // Increment the fence value for the current frame.
            fenceValues[currentFrame] = currentFenceValue + 1;
        }

        /// <summary> Wait for pending GPU work to complete. </summary>
        private void WaitForGPU()
        {
            // Schedule a Signal command in the queue.
            commandQueue.Signal(fence, fenceValues[currentFrame]);

            // Wait until the fence has been crossed.
            fence.SetEventOnCompletion(fenceValues[currentFrame], fenceEvent.SafeWaitHandle.DangerousGetHandle());
            fenceEvent.WaitOne();

            // Increment the fence value for the current frame.
            fenceValues[currentFrame]++;
        }

        private void CreateDeviceResources()
        {
#if DEBUG
            Configuration.EnableObjectTracking = true;
            Configuration.ThrowOnShaderCompileError = false;

            // Enable the D3D12 debug layer.
            DebugInterface.Get().EnableDebugLayer();
#endif

            using (var factory = new Factory4())
            {
                // Create the Direct3D 12 API device object
                device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);
                if (device == null)
                {
                    // TODO: We want to be able to specify adaptor
                    var adapter = factory.Adapters[0];
                    device = new Device(adapter, SharpDX.Direct3D.FeatureLevel.Level_11_0);
                }

                // Create the command queue.
                var queueDesc = new CommandQueueDescription(CommandListType.Direct);
                commandQueue = device.CreateCommandQueue(queueDesc);
                commandQueue.Name = $"CommandQueue";

                // Create Command Allocator buffers.
                for (int i = 0; i < FrameCount; i++)
                {
                    commandAllocators[i] = device.CreateCommandAllocator(CommandListType.Direct);
                    commandAllocators[i].Name = $"CommandAllocator F{i}";
                }
            }

            // Create RootSignature.
            var rootSignatureDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout,
                new[]
                {
                    new RootParameter(ShaderVisibility.Vertex,
                        new DescriptorRange()
                        {
                            RangeType = DescriptorRangeType.ConstantBufferView,
                            DescriptorCount = 1,
                            OffsetInDescriptorsFromTableStart = int.MinValue,
                            BaseShaderRegister = 0,
                        }),
                    new RootParameter(ShaderVisibility.Pixel,
                        new DescriptorRange()
                        {
                            RangeType = DescriptorRangeType.ShaderResourceView,
                            DescriptorCount = 1,
                            OffsetInDescriptorsFromTableStart = int.MinValue,
                            BaseShaderRegister = 0
                        })
                },
                new[]
                {
                    new StaticSamplerDescription(ShaderVisibility.Pixel, 0, 0)
                    {
                        Filter = Filter.MinimumMinMagMipPoint,
                        AddressUVW = TextureAddressMode.Border,
                    }
                });

            rootSignature = device.CreateRootSignature(rootSignatureDesc.Serialize());

            // Create Constant Buffer View Heap.
            var cbvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = 1,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
            };
            cbvHeap = device.CreateDescriptorHeap(cbvHeapDesc);
            cbvHeap.Name = "CBV Heap";

            // Create Shader Render View Heap.
            var srvHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = 1,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
            };
            srvHeap = device.CreateDescriptorHeap(srvHeapDesc);
            srvHeap.Name = "SRV Heap";

            // Create synchronization objects.
            fence = device.CreateFence(fenceValues[currentFrame], FenceFlags.None);
            fence.Name = $"Fence";
            fenceValues[currentFrame]++;

            // Create an event handle to use for frame synchronization.
            fenceEvent = new AutoResetEvent(false);
        }
        
        private void CreateWindowResources()
        {
            // Wait until all previous GPU work is complete.
            WaitForGPU();

            // Clear the previous window size specific content.
            for (int i = 0; i < FrameCount; i++)
                renderTargets[i] = null;

            // Calculate the necessary render target size in pixels.
            var outputSize = new Size2();
            outputSize.Width = window.ClientSize.Width;
            outputSize.Height = window.ClientSize.Height;

            // Prevent zero size DirectX content from being created.
            outputSize.Width = Math.Max(outputSize.Width, 640);
            outputSize.Height = Math.Max(outputSize.Width, 480);

            if (swapChain != null)
            {
                // If the swap chain already exists, resize it.
                swapChain.ResizeBuffers(FrameCount, outputSize.Width, outputSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

                if (!DXDebug.ValidateDevice(device))
                    throw new ArgumentNullException(nameof(device));
            }
            else
            {
                using (var factory = new Factory4())
                {
                    // Otherwise, create a new one using the same adapter as the existing Direct3D device.
                    SwapChainDescription swapChainDesc = new SwapChainDescription()
                    {
                        BufferCount = FrameCount,
                        ModeDescription = new ModeDescription(outputSize.Width, outputSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                        Usage = Usage.RenderTargetOutput,
                        SwapEffect = SwapEffect.FlipDiscard,
                        OutputHandle = window.Handle,
                        SampleDescription = new SampleDescription(1, 0),
                        IsWindowed = true,
                    };

                    var tempSwapChain = new SwapChain(factory, commandQueue, swapChainDesc);
                    swapChain = tempSwapChain.QueryInterface<SwapChain3>();
                    swapChain.DebugName = "SwapChain";
                    tempSwapChain.Dispose();
                }
            }

            // Create a render target view of the swap chain back buffer.
            var descriptorHeapDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = FrameCount,
                Type = DescriptorHeapType.RenderTargetView,
                Flags = DescriptorHeapFlags.None
            };
            rtvHeap = device.CreateDescriptorHeap(descriptorHeapDesc);
            rtvHeap.Name = "Render Target View Descriptor Heap";

            // All pending GPU work was already finished. Update the tracked fence values
            // to the last value signaled.
            for (int i = 0; i < FrameCount; i++)
                fenceValues[i] = fenceValues[currentFrame];

            currentFrame = 0;
            var rtvDescriptor = rtvHeap.CPUDescriptorHandleForHeapStart;
            rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
            for (int i = 0; i < FrameCount; i++)
            {
                renderTargets[i] = swapChain.GetBackBuffer<Resource>(i);
                device.CreateRenderTargetView(renderTargets[i], null, rtvDescriptor + (rtvDescriptorSize * i));

                renderTargets[i].Name = $"Render Target {i}";
            }

            viewport = new ViewportF();
            viewport.Width = outputSize.Width;
            viewport.Height = outputSize.Height;
            viewport.MaxDepth = 1.0f;

            scissorRect = new Rectangle();
            scissorRect.Right = outputSize.Width;
            scissorRect.Bottom = outputSize.Height;
        }

        public void Dispose()
        {
            WaitForPrevFrame();

            swapChain.SetFullscreenState(false, null);

            foreach (var target in renderTargets)
                target.Dispose();

            CommandAllocator.Dispose();
            CommandQueue.Dispose();
            rtvHeap.Dispose();
            fence.Dispose();
            swapChain.Dispose();
            device.Dispose();

            window?.Dispose();
        }
    }
}
