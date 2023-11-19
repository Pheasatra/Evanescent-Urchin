using System.Collections;
using System.Runtime.CompilerServices;

// -----------------------------------------------------------------------------------------------------

public static class Check
{
    // -----------------------------------------------------------------------------------------------------

    /// <summary> Check if a value is between a min and max </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InRange(float value, float min, float max)
    {
        return value >= min && value < max;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Check if a value is smaller than a max value </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLower(float value, float max)
    {
        return value < max;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Check if a value is larger than a min value </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHigher(float value, float min)
    {
        return value > min;
    }
}
