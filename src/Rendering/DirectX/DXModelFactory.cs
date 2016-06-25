namespace Nine.Graphics.Rendering
{
    using System;
    using Nine.Graphics.Content;

    public class DXModelFactory : ModelFactory<int>
    {
        public DXModelFactory(IModelLoader loader, int capacity = 1024)
            : base(loader, capacity)
        { }

        public override Model CreateModel(ModelContent data)
        {
            throw new NotImplementedException();
        }
    }
}
