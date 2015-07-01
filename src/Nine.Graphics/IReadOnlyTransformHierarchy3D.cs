namespace Nine.Graphics
{
    using System.Numerics;

    public interface IReadOnlyTransformHierarchy3D<T>
    {
        /// <summary>
        /// Flattens this 2D transform hierarchy.
        /// </summary>
        /// <param name="nodes">
        /// Ref to an output array that contains the nodes.
        /// The array is resized when the capacity is not enough to hold the nodes.
        /// </param>
        /// <param name="startIndex">
        /// Nodes will be appended to the values arrary starting from this index.
        /// </param>
        /// <param name="transforms">
        /// Ref to an output array that contains the transform for a corresponding node in values.
        /// The array is resized when the capacity is not enough to hold the transforms.
        /// </param>
        /// <param name="startTransformIndex">
        /// Transforms will be appended to the transforms array starting from this index.
        /// </param>
        /// <returns>
        /// The count of nodes as the result of this flatten operation.
        /// </returns>
        int Flatten(ref T[] nodes, int startIndex, ref Matrix4x4[] transforms, int startTransformIndex);
    }
}
