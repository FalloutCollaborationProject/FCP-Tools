#pragma warning disable CS0436 // intentional: polyfill conflicts with the framework's own type on newer targets
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute { }
}
#pragma warning restore CS0436
