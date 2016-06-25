namespace Nine.Graphics.Rendering
{
    using SharpDX.Direct3D12;
    using System;
    using System.Collections.Concurrent;

    class DXGraphicsCommandListPool : ConcurrentBag<GraphicsCommandList>
    {
        private ConcurrentBag<GraphicsCommandList> spareObjects;

        private readonly DXGraphicsHost graphicsHost;
        private readonly CommandAllocator allocator;
        private readonly CommandListType type;
        private readonly string debugName;

        public DXGraphicsCommandListPool(DXGraphicsHost graphicsHost, CommandAllocator allocator, CommandListType type, string debugName = "CMDLIST")
            : base()
        {
            if (graphicsHost == null) throw new ArgumentNullException(nameof(graphicsHost));
            if (allocator == null) throw new ArgumentNullException(nameof(allocator));

            this.graphicsHost = graphicsHost;
            this.allocator = allocator;
            this.type = type;
            this.debugName = debugName;
            this.spareObjects = new ConcurrentBag<GraphicsCommandList>();
        }

        public GraphicsCommandList GetObject(PipelineState initialState = null)
        {
            GraphicsCommandList item;

            // Try to take object
            if (spareObjects.TryTake(out item))
            {
                // Reset the object
                item.Reset(allocator, initialState);
                graphicsHost.BeginFrame(item);
                return item;
            }

            // Create new object
            this.Add(item = CreateNew(initialState));
            return item;
        }
        
        public void Reset()
        {
            // Empty spareObjects bag
            GraphicsCommandList someItem;
            while (!spareObjects.IsEmpty)
                spareObjects.TryTake(out someItem);

            // Fill spareObjects bag with all objects
            foreach (var item in this)
                spareObjects.Add(item);
        }

        private GraphicsCommandList CreateNew(PipelineState initialState)
        {
            var result = graphicsHost.Device.CreateCommandList(type, allocator, initialState);
            result.Name = $"{debugName} {this.Count}";
            graphicsHost.BeginFrame(result);
            return result;
        }
    }
}
