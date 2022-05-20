using System;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.IoC;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Utility;

namespace Robust.Shared.Serialization.TypeSerializers.Interfaces
{
    public interface ITypeReader<TType, TNode> : ITypeValidator<TType, TNode> where TNode : DataNode
    {
        /// <summary>
        ///     Deserializes a node into an object, populating it.
        /// </summary>
        /// <param name="serializationManager">The serialization manager to use, if any.</param>
        /// <param name="node">The node to deserialize.</param>
        /// <param name="dependencies">The dependencies to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="value">The value to read into. If none is supplied, a new object will be created.</param>
        /// <returns>The deserialized object.</returns>

        TType Read(ISerializationManager serializationManager,
            TNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null, TType? value = default);

        /// <summary>
        ///     Tries to deserializes a node into an object, populating it.
        /// </summary>
        /// <param name="serializationManager">The serialization manager to use, if any.</param>
        /// <param name="node">The node to deserialize.</param>
        /// <param name="dependencies">The dependencies to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <param name="result"> The deserialized object</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="value">The value to read into. If none is supplied, a new object will be created.</param>
        /// <returns>If the deserialization succeeded</returns>
        bool TryRead(ISerializationManager serializationManager,
            TNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            [NotNullWhen(true)] out TType? result,
            ISerializationContext? context = null, TType? value = default)
        {
            try
            {
                result = Read(serializationManager, node, dependencies, skipHook, context, value)!;
                return true;
            }
            catch (Exception ex) when (ex is FormatException or InvalidMappingException)
            {
                result = value;
                return false;
            }
        }
    }
}
