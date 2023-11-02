using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class Manager : MonoBehaviour
{
    // Only one of these so we can make it a singleton, also make sure we can't set this
    public static Manager manager { get; private set; }

    [Header("Global control variables")]
    public float screenShakeMultiplyer = 2.0f;
    public float teslaScreenShake = 2.0f;

    public float gameEndDistance = 30.0f;

    public Transform endPosition;
    public float switchHeight;

    [Header("Important variables")]
    //public SmoothCamera2D smoothCamera;

    [Header("References")]
    public GameObject deathExplosion;

    [Header("Seed")]
    public int xSeed = 0;
    public int ySeed = 0;
    public int zSeed = 0;

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

    [Space(10)]

    public SimplexNoise simplexNoise = new SimplexNoise();

    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        manager = this;
    }

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        simplexNoise.Setup();

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
    }
}