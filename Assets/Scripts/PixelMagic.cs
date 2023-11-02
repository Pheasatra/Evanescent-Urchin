using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixelMagic : MonoBehaviour
{
    public Manager manager;

    public Sprite originalSprite;
    public Sprite activeSprite;
    private Image image;

    [Space(10)]

    [Header("Noise")]
    public int octaves;
    public int worldSeed = 0;
    public int seedRange = 32767;

    [Space(10)]

    public float scale = 0.075f;
    public float amplitude = 1.0f;
    public float frequency = 1.0f;

    [Space(10)]
    public float persistance = 1.0f;
    public float lacunarity = 1.0f;

    public Vector3 offset = new Vector3(0.25f, 0.25f, 0.25f);
    public Vector3[] octaveSeeds;

    public float speedMultiplyer = 10;

    [Space(10)]

    public float variation = 0.05f;
    public Color baseColour;

    [Space(10)]

    public float updateRate = 15;

    private float xSeed = 0;
    private float ySeed = 0;
    private float zSeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        octaveSeeds = new Vector3[octaves];

        for (int x = 0; x < octaves; x++)
        {
            float offsetX = Random.Range(-seedRange, seedRange) + offset.x;
            float offsetY = Random.Range(-seedRange, seedRange) + offset.y;
            float offsetZ = Random.Range(-seedRange, seedRange) + offset.z;

            octaveSeeds[x] = new Vector3(offsetX, offsetY, offsetZ);
        }

        worldSeed = Random.Range(-seedRange, seedRange);
        Random.InitState(worldSeed);

        xSeed = Random.Range(-seedRange, seedRange);
        ySeed = Random.Range(-seedRange, seedRange);
        zSeed = Random.Range(-seedRange, seedRange);

        image = GetComponent<Image>();

        // Reset
        activeSprite.texture.SetPixels(originalSprite.texture.GetPixels());
        activeSprite.texture.Apply();

        PixelShader();

        InvokeRepeating("Main", 0.0f, 1.0f / updateRate);
    }

    // Update is called once per frame
    void Main()
    {
        PixelShader();
    }

    //-----------------------------------------------------------------------

    public void PixelShader()
    {
        // Iterate through it's pixels
        for (int x = 0; x < activeSprite.texture.width; x++)
        {
            for (int y = 0; y < activeSprite.texture.height; y++)
            {
                Color currentColour = originalSprite.texture.GetPixel(x, y);

                switch (currentColour.a > 0.1f)
                {
                    case true:
                        float shading = FullSimplex3D(x, y, 0);
                        float varience = Random.Range(-variation, variation);

                        activeSprite.texture.SetPixel(x, y, baseColour + new Color(shading, shading, shading, 0) + new Color(varience, varience, varience, 0)); ;
                        break;

                }
            }
        }

        activeSprite.texture.Apply();

        image.sprite = activeSprite;
    }   
    
    //-----------------------------------------------------------------------

    public float FullSimplex3D(float x, float y, float z)
    {
        float output = 0;

        float currentAmplitude = amplitude;
        float currentFrequency = frequency;

        //Loop through all octaves.
        for (int i = 0; i < octaves; i++)
        {
            float xCoord = (x + xSeed) / scale * currentFrequency + octaveSeeds[i].x;
            float yCoord = (y + ySeed - (Time.time * speedMultiplyer * i)) / scale * currentFrequency + octaveSeeds[i].y;
            float zCoord = (z + zSeed) / scale * currentFrequency + octaveSeeds[i].z;

            output += manager.simplexNoise.SimplexNoise3D(xCoord, yCoord, zCoord) * currentAmplitude;

            currentAmplitude *= persistance;
            currentFrequency *= lacunarity;
        }

        return output;
    }
}
