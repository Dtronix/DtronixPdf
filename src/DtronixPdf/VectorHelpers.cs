using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace DtronixPdf;

internal static class VectorHelpers
{
    /// <summary>
    /// Gets the width of the vector.
    /// </summary>
    /// <param name="vector">Order needs to be MinX, MinY, MaxX, MaxY</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetWidth(this in Vector128<float> vector)
    {
        return vector[2] - vector[0];
    }

    /// <summary>
    /// Gets the height of the vector.
    /// </summary>
    /// <param name="vector">Order needs to be MinX, MinY, MaxX, MaxY</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetHeight(this in Vector128<float> vector)
    {
        return vector[3] - vector[1];
    }
}
