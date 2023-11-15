using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static SimplexNoise;

// -----------------------------------------------------------------------------------------------------

public class SimplexNoise
{
    // -----------------------------------------------------------------------------------------------------

    // Simplex Noise
    private static Grad[] grad3 = new Grad[12]
        {
            new Grad(1, 1, 0), new Grad(-1, 1, 0), new Grad(1, -1, 0), new Grad(-1, -1, 0),
            new Grad(1, 0, 1), new Grad(-1, 0, 1), new Grad(1, 0, -1), new Grad(-1, 0, -1),
            new Grad(0, 1, 1), new Grad(0, -1, 1), new Grad(0, 1, -1), new Grad(0, -1, -1)
        };

    // -----------------------------------------------------------------------------------------------------

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

    // -----------------------------------------------------------------------------------------------------

    // To remove the need for index wrapping, double the permutation table length
    private static short[] perm = new short[512];
    private static short[] permMod12 = new short[512];

    private double F2 = 0.5 * (Mathf.Sqrt(3.0f) - 1.0);
    private double G2 = (3.0 - Mathf.Sqrt(3.0f)) / 6.0;
    private double G2Times2;

    // Very nice and simple skew factor for 3D
    private double F3 = 1.0f / 3.0f;
    private double G3 = 1.0f / 6.0f;
    private double G3Times2;
    private double G3Times3;

    // -----------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Setup()
    {
        // Simplex Setup
        for (int i = 0; i < 512; i++)
        {
            perm[i] = p[i & 255];
            permMod12[i] = (short)(perm[i] % 12);
        }

        G2Times2 = 2.0 * G2;

        G3Times2 = 2.0 * G3;
        G3Times3 = 3.0 * G3;
    }

    // -----------------------------------------------------------------------------------------------------

    //The fastest simplex noise in the west 
    //https://gist.github.com/boj/1759876

    // 2D simplex noise 
    // Functions inlined and variables precomputed for maximum performance (8.5 times faster than the fastest in the west)
    public double SimplexNoise2D(double x, double y)
    {
        // Noise contributions from the three corners
        double n0;
        double n1;
        double n2;

        // Skew the input space to determine which simplex cell we're in
        double s = (x + y) * F2; // Hairy factor for 2D

        // Semi-Inlined FastFloor for to save 0.75ms 
        double floorInX = x + s;
        double floorInY = y + s;

        int xi = (int)floorInX;
        int yi = (int)floorInY;

        int i = floorInX < xi ? xi - 1 : xi;
        int j = floorInY < yi ? yi - 1 : yi;

        //int i = FastFloor(xin + s);
        //int j = FastFloor(yin + s);

        double t = (i + j) * G2;
        double X0 = i - t; // Unskew the cell origin back to (x,y) space
        double Y0 = j - t;
        double x0 = x - X0; // The x,y distances from the cell origin
        double y0 = y - Y0;

        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
        // Offsets for second (middle) corner of simplex in (i,j) coords
        int i1; 
        int j1;

        // lower triangle, XY order: (0,0)->(1,0)->(1,1)
        // upper triangle, YX order: (0,0)->(0,1)->(1,1)
        // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
        // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
        // c = (3-Sqrt(3))/6
        switch (x0 > y0)
        {
            case true:
                i1 = 1; // lower triangle, XY order: (0,0)->(1,0)->(1,1)
                j1 = 0; // upper triangle, YX order: (0,0)->(0,1)->(1,1)
                break;

            case false:
                i1 = 0; // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
                j1 = 1; // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
                break;
        }

        // Offsets for middle corner in (x,y) unskewed coords
        double x1 = x0 - i1 + G2; 
        double y1 = y0 - j1 + G2;

        // Offsets for last corner in (x,y) unskewed coords
        double x2 = x0 - 1.0 + G2Times2;
        double y2 = y0 - 1.0 + G2Times2;

        // Work out the hashed gradient indices of the three simplex corners
        int ii = i & 255;
        int jj = j & 255;

        int gi0 = perm[ii + perm[jj]] % 12;
        int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
        int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;

        // Calculate the contribution from the three corners
        double t0 = 0.5 - x0 * x0 - y0 * y0;
        double t1 = 0.5 - x1 * x1 - y1 * y1;
        double t2 = 0.5 - x2 * x2 - y2 * y2;

        switch (t0 < 0)
        {
            case true:
                n0 = 0.0;
                break;

            case false:
                t0 *= t0;
                //n0 = t0 * t0 * Dot(grad3[gi0], x0, y0);  // (x,y) of grad3 used for 2D gradient
                // Inlined Dot for mega performance
                n0 = t0 * t0 * (grad3[gi0].x * x0 + grad3[gi0].y * y0);  // (x,y) of grad3 used for 2D gradient
                break;
        }

        switch (t1 < 0)
        {
            case true:
                n1 = 0.0;
                break;

            case false:
                t1 *= t1;
                n1 = t1 * t1 * (grad3[gi1].x * x1 + grad3[gi1].y * y1);
                //n1 = t1 * t1 * Dot(grad3[gi1], x1, y1);
                break;
        }


        switch (t2 < 0)
        {
            case true:
                n2 = 0.0;
                break;

            case false:
                t2 *= t2;
                n2 = t2 * t2 * (grad3[gi2].x * x2 + grad3[gi2].y * y2);
                //n2 = t2 * t2 * Dot(grad3[gi2], x2, y2);
                break;
        }

        // Add contributions from each corner to get the final noise value.
        // The result is scaled to return values in the interval [-1,1].
        return 70.0 * (n0 + n1 + n2);
    }

    // -----------------------------------------------------------------------------------------------------

    public double SimplexNoise3D(float x, float y, float z)
    {
        // Noise contributions from the four corners
        double n0;
        double n1;
        double n2;
        double n3;

        // Skew the input space to determine which simplex cell we're in
        double s = (x + y + z) * F3; 

        // Semi-Inlined FastFloor for to save 0.75ms 
        double floorInX = x + s;
        double floorInY = y + s;
        double floorInZ = z + s;

        int xi = (int)floorInX;
        int yi = (int)floorInY;
        int zi = (int)floorInZ;

        int i = floorInX < xi ? xi - 1 : xi;
        int j = floorInY < yi ? yi - 1 : yi;
        int k = floorInZ < zi ? zi - 1 : zi;

        //int i = FastFloor(x + s);
        //int j = FastFloor(y + s);
        //int k = FastFloor(z + s);

        double t = (i + j + k) * G3;

        // Unskew the cell origin back to (x,y,z) space
        double X0 = i - t;
        double Y0 = j - t;
        double Z0 = k - t;

        // The x, y, z distances from the cell origin
        double x0 = x - X0;
        double y0 = y - Y0;
        double z0 = z - Z0;

        // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
        // Determine which simplex we are in.

        // Offsets for second corner of simplex in (i,j,k) coords
        int i1;
        int j1;
        int k1;

        // Offsets for third corner of simplex in (i,j,k) coords
        int i2;
        int j2;
        int k2;

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
        // c = 1 / 6.

        // Offsets for second corner in (x,y,z) coords
        double x1 = x0 - i1 + G3;
        double y1 = y0 - j1 + G3;
        double z1 = z0 - k1 + G3;

        // Offsets for third corner in (x,y,z) coords
        double x2 = x0 - i2 + G3Times2; 
        double y2 = y0 - j2 + G3Times2;
        double z2 = z0 - k2 + G3Times2;

        // Offsets for last corner in (x,y,z) coords
        double x3 = x0 - 1.0f + G3Times3; 
        double y3 = y0 - 1.0f + G3Times3;
        double z3 = z0 - 1.0f + G3Times3;

        // Work out the hashed gradient indices of the four simplex corners
        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;

        int gi0 = permMod12[ii + perm[jj + perm[kk]]];
        int gi1 = permMod12[ii + i1 + perm[jj + j1 + perm[kk + k1]]];
        int gi2 = permMod12[ii + i2 + perm[jj + j2 + perm[kk + k2]]];
        int gi3 = permMod12[ii + 1 + perm[jj + 1 + perm[kk + 1]]];

        // Calculate the contribution from the four corners
        double t0 = 0.5f - x0 * x0 - y0 * y0 - z0 * z0;
        double t1 = 0.5f - x1 * x1 - y1 * y1 - z1 * z1;
        double t2 = 0.5f - x2 * x2 - y2 * y2 - z2 * z2;
        double t3 = 0.5f - x3 * x3 - y3 * y3 - z3 * z3;

        switch (t0 < 0)
        {
            case true:
                n0 = 0.0f;
                break;

            case false:
                t0 *= t0;
                // Inlined Dot for mega performance

                n0 = t0 * t0 * (grad3[gi0].x * x0 + grad3[gi0].y * y0 + grad3[gi0].z * z0);
                //n0 = t0 * t0 * Dot(grad3[gi0], x0, y0, z0);
                break;
        }

        switch (t1 < 0)
        {
            case true:
                n1 = 0.0f;
                break;

            case false:
                t1 *= t1;
                n1 = t1 * t1 * (grad3[gi1].x * x1 + grad3[gi1].y * y1 + grad3[gi1].z * z1);
                //n1 = t1 * t1 * Dot(grad3[gi1], x1, y1, z1);
                break;
        }

        switch (t2 < 0)
        {
            case true:
                n2 = 0.0f;
                break;

            case false:
                t2 *= t2;
                n2 = t2 * t2 * (grad3[gi2].x * x2 + grad3[gi2].y * y2 + grad3[gi2].z * z2);
                //n2 = t2 * t2 * Dot(grad3[gi2], x2, y2, z2);
                break;
        }

        switch (t3 < 0)
        {
            case true:
                n3 = 0.0f;
                break;

            case false:
                t3 *= t3;
                n3 = t3 * t3 * (grad3[gi3].x * x3 + grad3[gi3].y * y3 + grad3[gi3].z * z3);
                //n3 = t3 * t3 * Dot(grad3[gi3], x3, y3, z3);
                break;
        }

        // Add contributions from each corner to get the final noise value.
        // The result is scaled to stay just inside [-1, 1]
        return 32.0 * (n0 + n1 + n2 + n3);
    }

    // -----------------------------------------------------------------------------------------------------

    // This method is a *lot* faster than using (int)Math.floor(x)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastFloor(double x)
    {
        int xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    // -----------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Grad grad, double x, double y, double z)
    {
        return grad.x * x + grad.y * y + grad.z * z;
    }

    // -----------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Grad grad, double x, double y)
    {
        return grad.x * x + grad.y * y;
    }

    // -----------------------------------------------------------------------------------------------------

    public struct Grad
    {
        public float x, y, z;

        public Grad(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}