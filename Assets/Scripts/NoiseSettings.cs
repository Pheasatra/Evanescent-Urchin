using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

[System.Serializable]
public class NoiseSettings
{
    public string name;

    [Space(10)]

    [Header("Base values")]
    public float scale;
    public float amplitude;
    public float frequency;
    public float waveSpeed;

    [Space(10)]

    [Header("Layering")]
    [Tooltip("How many layers of noise do you want to ovelap?")]
    public int octaves;

    [Space(10)]

    [Tooltip("Multiplies amplitude by octave count, with persistance of 2 heights become larger and larger for example")]
    public float persistance;

    [Tooltip("Multiplies frequency by octave count")]
    public float lacunarity;

    [Tooltip("Multiplies waveScale by octave count")]
    public float waveSubspeed;

    [Header("Seeds")]
    public bool manualSeeds = false;

    [Space(10)]

    public int xSeed = 0;
    public int ySeed = 0;
    public int zSeed = 0;

    [Space(10)]

    public Vector3[] octaveOffsets;

    // A cache of the seed range so we only grap it from the terrain manager once
    private int seedRange;

    // -----------------------------------------------------------------------------------------------------

    /// <summary>  </summary>
    public void UpdateSeeds()
    {
        switch (manualSeeds)
        {
            // Randomly generate seed
            case false:
                xSeed = UnityEngine.Random.Range(-seedRange, seedRange);
                ySeed = UnityEngine.Random.Range(-seedRange, seedRange);
                zSeed = UnityEngine.Random.Range(-seedRange, seedRange);
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary>  </summary>
    public void UpdateOctaveOffsets()
    {
        octaveOffsets = new Vector3[octaves];

        // For all octaves
        for (int x = 0; x < octaves; x++)
        {
            float offsetX = UnityEngine.Random.Range(-seedRange, seedRange);
            float offsetY = UnityEngine.Random.Range(-seedRange, seedRange);
            float offsetZ = UnityEngine.Random.Range(-seedRange, seedRange);

            octaveOffsets[x] = new Vector3(offsetX, offsetY, offsetZ);
        }

    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Sets up all of our dependant settings </summary>
    public void Setup()
    {
        seedRange = TerrainManager.terrainManager.seedRange;

        UpdateSeeds();
        UpdateOctaveOffsets();
    }

    // -----------------------------------------------------------------------------------------------------

    // Using a property we can update the octave offsets on set
    public int Octaves
    {
        get
        {
            return octaves;
        }

        set
        {
            octaves = value;
            UpdateOctaveOffsets();
        }
    }

    // -----------------------------------------------------------------------------------------------------

    public float Scale
    {
        get
        {
            return scale;
        }

        set
        {
            // Prevent scale from ever being 0
            scale = Math.Max(scale, 0.0001f);
            scale = value;
        }
    }
}