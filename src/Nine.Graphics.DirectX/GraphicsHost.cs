namespace Nine.Graphics.DirectX
{
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D12;
    using SharpDX.DXGI;
    using SharpDX.Windows;
    using System;
    using System.Threading;

    using Device = SharpDX.Direct3D12.Device;
    using Resource = SharpDX.Direct3D12.Resource;

    public class GraphicsHost : IGraphicsHost
    {
        private readonly RenderForm window;

        private Device device;
        private SwapChain swapChain;

        private CommandQueue commandQueue;
        private CommandAllocator commandListAllocator;
        private GraphicsCommandList commandList;

        private DescriptorHeap descriptorHeap;
        private Resource renderTarget;

        private ViewportF viewport;
        private Rectangle scissorRectangle;

        private AutoResetEvent eventHandle;
        private Fence fence;
        private long currentFence;

        private const int SwapBufferCount = 2;
        private int indexLastSwapBuf;

        public int Width => window.Width;
        public int Height => window.Height;

        public GraphicsHost(int width, int height, bool hidden = false) // FormBorderStyle = FormBorderStyle.FixedSingle
            : this(new RenderForm("Nine.Graphics") { Width = width, Height = height }, hidden)
        { }

        public GraphicsHost(RenderForm window, bool hidden = false)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            this.window = window;
            if (!hidden)
            {
                this.window.Visible = true;
            }

#if DEBUG
            Configuration.EnableObjectTracking = true;
            Configuration.ThrowOnShaderCompileError = false;
#endif

            var swapChainDescription = new SwapChainDescription()
            {
                BufferCount = SwapBufferCount,
                ModeDescription = new ModeDescription(Format.R8G8B8A8_UNorm),
                Usage = Usage.RenderTargetOutput,
                OutputHandle = window.Handle,
                SwapEffect = SwapEffect.FlipSequential,
                SampleDescription = new SampleDescription(1, 0),
                IsWindowed = true
            };

            try
            {
                device = new Device(DriverType.Hardware, DeviceCreationFlags.None, FeatureLevel.Level_9_1);
                commandQueue = device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));
                using (var factory = new Factory1())
                    swapChain = new SwapChain(factory, commandQueue, swapChainDescription);
            }
            catch (SharpDXException)
            {
                device = new Device(DriverType.Warp, DeviceCreationFlags.None, FeatureLevel.Level_9_1);
                commandQueue = device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Direct));
                using (var factory = new Factory1())
                    swapChain = new SwapChain(factory, commandQueue, swapChainDescription);
            }

            commandListAllocator = device.CreateCommandAllocator(CommandListType.Direct);

            descriptorHeap = device.CreateDescriptorHeap(new DescriptorHeapDescription()
            {
                Type = DescriptorHeapType.RenderTargetView,
                DescriptorCount = 1
            });

            commandList = device.CreateCommandList(CommandListType.Direct, commandListAllocator, null);

            renderTarget = swapChain.GetBackBuffer<Resource>(0);
            device.CreateRenderTargetView(renderTarget, null, descriptorHeap.CPUDescriptorHandleForHeapStart);

            viewport = new ViewportF(0, 0, this.Width, this.Height);
            scissorRectangle = new Rectangle(0, 0, this.Width, this.Height);

            fence = device.CreateFence(0, FenceMiscFlags.None);
            currentFence = 1;

            commandList.Close();

            eventHandle = new AutoResetEvent(false);

            WaitForPrevFrame();
        }

        public bool BeginFrame()
        {
            commandListAllocator.Reset();
            commandList.Reset(commandListAllocator, null);

            commandList.SetViewport(viewport);
            commandList.SetScissorRectangles(scissorRectangle);

            commandList.ResourceBarrierTransition(renderTarget, ResourceUsage.Present, ResourceUsage.RenderTarget);

            commandList.ClearRenderTargetView(descriptorHeap.CPUDescriptorHandleForHeapStart, new Color4(1.0f, 1.0f, 1.0f, 1.0f), null, 0);
            commandList.ResourceBarrierTransition(renderTarget, ResourceUsage.RenderTarget, ResourceUsage.Present);

            commandList.Close();

            return true;
        }

        public void EndFrame()
        {
            commandQueue.ExecuteCommandList(commandList);

            swapChain.Present(1, PresentFlags.None);
            indexLastSwapBuf = (indexLastSwapBuf + 1) % SwapBufferCount;
            Utilities.Dispose(ref renderTarget);
            renderTarget = swapChain.GetBackBuffer<Resource>(indexLastSwapBuf);
            device.CreateRenderTargetView(renderTarget, null, descriptorHeap.CPUDescriptorHandleForHeapStart);

            WaitForPrevFrame();
        }

        public TextureContent GetTexture()
        {
            var buffer = swapChain.GetBackBuffer<Resource>(0);

            // TODO: Format?
            if (buffer.Description.Format == Format.R8G8B8A8_UNorm)
            {
                var width  = (int)buffer.Description.Width;
                var height = (int)buffer.Description.Height;

                // TODO: Get Data

                return new TextureContent(width, height, null);
            }

            throw new NotImplementedException();
        }

        private void WaitForPrevFrame()
        {
            long localFence = currentFence;
            commandQueue.Signal(fence, localFence);
            currentFence++;

            if (fence.CompletedValue < localFence)
            {
                fence.SetEventOnCompletion(localFence, eventHandle.SafeWaitHandle.DangerousGetHandle());
                eventHandle.WaitOne();
            }
        }

        public void Dispose()
        {
            WaitForPrevFrame();

            swapChain.SetFullscreenState(false, null);

            eventHandle.Close();

            Utilities.Dispose(ref commandList);

            Utilities.Dispose(ref descriptorHeap);
            Utilities.Dispose(ref renderTarget);
            Utilities.Dispose(ref commandListAllocator);
            Utilities.Dispose(ref commandQueue);
            Utilities.Dispose(ref device);
            Utilities.Dispose(ref swapChain);

            window?.Dispose();
        }
    }
}
