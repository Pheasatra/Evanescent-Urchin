using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

//-----------------------------------------------------------------------

public class GerstnerNoise
{
    /// <summary> Takes an x y position and returns the noise value </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GerstnerNoise2D(float x, float y, float time, float steepness, float amplitude)
    {
        float output;
        float dotMultiplier = 1;
        float directionMultiplier = 1;
        float phaseConst = 0.1f;

        // Hmmmmmmmmmm, i'm pretty sure this will always return 1 or 0
        float dotProduct = Dot(x, y, x, y);

        //output = (float)(steepness * amplitude * direction * Math.Cos(dotMultiplyer * dotProduct + phaseConst * time));
        output = (float)(steepness * amplitude * directionMultiplier * Math.Cos(dotMultiplier * dotProduct + phaseConst * time));

        return output;
    }    
    
    //-----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(float xStart, float yStart, float xEnd, float yEnd)
    {
        return xStart * xEnd + yStart * yEnd;
    }
}