using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

//-----------------------------------------------------------------------

public class SimplexNoise
{
    // Simplex Noise
    private static Grad[] grad3 = new Grad[12]
        {
            new Grad(1, 1, 0), new Grad(-1, 1, 0), new Grad(1, -1, 0), new Grad(-1, -1, 0),
            new Grad(1, 0, 1), new Grad(-1, 0, 1), new Grad(1, 0, -1), new Grad(-1, 0, -1),
            new Grad(0, 1, 1), new Grad(0, -1, 1), new Grad(0, 1, -1), new Grad(0, -1, -1)
        };

    private static short[] p = new short[256]
        {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

    // To remove the need for index wrapping, double the permutation table length
    private static short[] perm = new short[512];
    private static short[] permMod12 = new short[512];

    private float F3 = 0;
    private float G3 = 0;

    //-----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Setup()
    {
        // Simplex Setup
        for (int i = 0; i < 512; i++)
        {
            perm[i] = p[i & 255];
            permMod12[i] = (short)(perm[i] % 12);
        }

        F3 = 1.0f / 3.0f;
        G3 = 1.0f / 6.0f;
    }

    //-----------------------------------------------------------------------

    // Way too big for this
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float SimplexNoise3D(float x, float y, float z)
    {
        float n0, n1, n2, n3; // Noise contributions from the four corners
                              // Skew the input space to determine which simplex cell we're in
        float s = (x + y + z) * F3; // Very nice and simple skew factor for 3D

        int i = FastFloor(x + s);
        int j = FastFloor(y + s);
        int k = FastFloor(z + s);

        float t = (i + j + k) * G3;
        float X0 = i - t; // Unskew the cell origin back to (x,y,z) space
        float Y0 = j - t;
        float Z0 = k - t;
        float x0 = x - X0; // The x,y,z distances from the cell origin
        float y0 = y - Y0;
        float z0 = z - Z0;

        // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
        // Determine which simplex we are in.
        int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
        int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

        switch (x0 >= y0)
        {
            case true:
                switch (y0 >= z0)
                {
                    // X Y Z order
                    case true:
                        i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                        break;

                    case false:
                        switch (x0 >= z0)
                        {
                            // X Z Y order
                            case true:
                                i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1;
                                break;

                            // Z X Y order
                            case false:
                                i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1;
                                break;
                        }
                        break;
                }
                break;

            case false:
                switch (y0 < z0)
                {
                    // Z Y X order
                    case true:
                        i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1;
                        break;

                    case false:
                        switch (x0 < z0)
                        {
                            // Y Z X order
                            case true:
                                i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1;
                                break;

                            // Y X Z order
                            case false:
                                i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0;
                                break;
                        }
                        break;
                }
                break;
        }

        // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
        // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
        // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
        // c = 1/6.
        float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;
        float x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
        float y2 = y0 - j2 + 2.0f * G3;
        float z2 = z0 - k2 + 2.0f * G3;
        float x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
        float y3 = y0 - 1.0f + 3.0f * G3;
        float z3 = z0 - 1.0f + 3.0f * G3;

        // Work out the hashed gradient indices of the four simplex corners
        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;
        int gi0 = permMod12[ii + perm[jj + perm[kk]]];
        int gi1 = permMod12[ii + i1 + perm[jj + j1 + perm[kk + k1]]];
        int gi2 = permMod12[ii + i2 + perm[jj + j2 + perm[kk + k2]]];
        int gi3 = permMod12[ii + 1 + perm[jj + 1 + perm[kk + 1]]];

        // Calculate the contribution from the four corners
        float t0 = 0.5f - x0 * x0 - y0 * y0 - z0 * z0;
        float t1 = 0.5f - x1 * x1 - y1 * y1 - z1 * z1;
        float t2 = 0.5f - x2 * x2 - y2 * y2 - z2 * z2;
        float t3 = 0.5f - x3 * x3 - y3 * y3 - z3 * z3;

        switch (t0 < 0)
        {
            case true:
                n0 = 0.0f;
                break;

            case false:
                t0 *= t0;
                n0 = t0 * t0 * Dot(grad3[gi0], x0, y0, z0);
                break;
        }

        switch (t1 < 0)
        {
            case true:
                n1 = 0.0f;
                break;

            case false:
                t1 *= t1;
                n1 = t1 * t1 * Dot(grad3[gi1], x1, y1, z1);
                break;
        }

        switch (t2 < 0)
        {
            case true:
                n2 = 0.0f;
                break;

            case false:
                t2 *= t2;
                n2 = t2 * t2 * Dot(grad3[gi2], x2, y2, z2);
                break;
        }

        switch (t3 < 0)
        {
            case true:
                n3 = 0.0f;
                break;

            case false:
                t3 *= t3;
                n3 = t3 * t3 * Dot(grad3[gi3], x3, y3, z3);
                break;
        }

        // Add contributions from each corner to get the final noise value.
        // The result is scaled to stay just inside [-1, 1]
        return 32.0f * (n0 + n1 + n2 + n3);
    }

    //-----------------------------------------------------------------------

    // This method is a *lot* faster than using (int)Math.floor(x)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int FastFloor(float x)
    {
        int xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    //-----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Grad grad, float x, float y, float z)
    {
        return grad.x * x + grad.y * y + grad.z * z;
    }

    //-----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Grad grad, float x, float y)
    {
        return grad.x * x + grad.y * y;
    }

    //-----------------------------------------------------------------------

    public struct Grad
    {
        public float x, y, z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Grad(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}