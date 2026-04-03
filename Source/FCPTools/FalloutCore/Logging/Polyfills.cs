// net472 doesn't include System.Runtime.CompilerServices.InterpolatedStringHandlerAttribute in its BCL.
// Roslyn 4.0+ synthesizes it, but may find an inaccessible (internal) copy in a reference assembly
// (e.g. Lib.Harmony) first, causing CS0122. Defining it here gives the compiler an accessible version.
#pragma warning disable CS0436 // type conflicts with imported type — intentional
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute { }
}
#pragma warning restore CS0436
