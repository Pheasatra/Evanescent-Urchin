using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager terrainManager { get; private set; }

    [Header("References")]
    public GameObject chunkPrefab;
    public SimplexNoise simplexNoise = new SimplexNoise();
    public FastNoiseLite fastNoiseLite = new FastNoiseLite();
    public FastNoise fastNoise2;

    // Where all our inactive chunks are stored
    public ChunkPool chunkPool;

    public Color[] colours;

    public TileType[] tileTypes;

    public Vector3 windDirection;

    [Header("Seeds")]
    public int seedRange = 32767;

    [Space(10)]

    public bool manualSeed = false;
    public int worldSeed = 0;

    [Header("Noise")]
    public NoiseSettings[] noiseSettings;

    [Space(10)]

    public float visiblityLimit = 0.5f;

    [Header("Chunks")]
    public float chunkUnitSize = 16;
    public int baseChunkSize = 16;
    public int renderDistance = 4;

    [Space(10)]

    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    [Space(10)]

    public Vector3Int cameraChunkIndex;
    private Vector3Int oldCameraChunkIndex;

    [HideInInspector]
    public int currentRenderDistance;

    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        terrainManager = this;
    }

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        simplexNoise.Setup();
        fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        fastNoise2 = new FastNoise("FractalFBm");
        fastNoise2.Set("Source", new FastNoise("Simplex"));
        fastNoise2.Set("Gain", 0.3f);
        fastNoise2.Set("Lacunarity", 0.6f);

        switch (manualSeed)
        {
            // Randomly generate seed
            case false:
                int dateTicks = (int)DateTime.Now.Ticks;

                worldSeed = UnityEngine.Random.Range(-dateTicks, dateTicks);
                break;
        }

        // Set the primary world seed that will set the x, y, z seeds
        UnityEngine.Random.InitState(worldSeed);

        // Update all of our settings
        for (int x = 0; x < noiseSettings.Length; x++)
        {
            noiseSettings[x].Setup();
        }
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        // Update all of our settings realtime so we can play with variables
        for (int x = 0; x < noiseSettings.Length; x++)
        {
            noiseSettings[x].UpdateSubNoise();
        }

        oldCameraChunkIndex = cameraChunkIndex;
        cameraChunkIndex = FindChunkIndex(Camera.main.transform.position);

        // If the camera position has changed or the render distance has changed then update our chunks
        switch (cameraChunkIndex != oldCameraChunkIndex || renderDistance != currentRenderDistance)
        {
            // Only update chunks if the camera chunk index has changed
            case true:
                currentRenderDistance = renderDistance;

                UpdateChunks();
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Using a chunk key find any chunks at this position </summary>
    public void UpdateChunks()
    {
        // Set our things here so we can reuse them
        Vector3Int rawChunkKey;
        Vector3Int chunkKey;

        float distance;

        Chunk chunk;

        // For all positions in our render distance
        for (int x = -currentRenderDistance; x < currentRenderDistance; x++)
        {
            for (int z = -currentRenderDistance; z < currentRenderDistance; z++)
            {
                // Compare the (x, y, z).magnitude to the current render distance, this is the same as distance < current render distance (Think of magnitude as the length of a line)
                // This allows us to avoid using Vector2Int.Distance and is much more elegant
                rawChunkKey = new Vector3Int(x, 0, z);
                distance = rawChunkKey.magnitude;

                switch (distance < currentRenderDistance)
                {
                    // If distance between the chunk and target is larger than renderDistance then skip to the next chunk key
                    case false:
                        continue;
                }

                // Combine our camera chunk position with the chunk position
                chunkKey = new Vector3Int(cameraChunkIndex.x, 0, cameraChunkIndex.z) + rawChunkKey;

                chunk = GetChunk(chunkKey);

                switch (chunk)
                {
                    // If this chunk already exists then skip this
                    case not null:
                        continue;
                }

                SpawnChunk(chunkKey, baseChunkSize, chunkUnitSize);
            }
        }

        // Convert the chunk dictionaries keys into a list so the iterator does not vaporise itself
        List<Vector3Int> keys = new List<Vector3Int>(chunks.Keys);

        // Make all of the elements in the dictionary CheckForUnload
        for (int x = 0; x < keys.Count; x++)
        {
            // Get the distance between the camera position
            distance = Vector3.Distance(new Vector3Int(cameraChunkIndex.x, 0, cameraChunkIndex.z), keys[x]);

            switch (distance < currentRenderDistance)
            {
                // If distance is smaller than currentRenderDistance
                case true:
                    chunk = GetChunk(keys[x]);
                    chunk.lodDistance = Mathf.RoundToInt(distance / 2.0f);
                    continue;
            }

            // Pool this chunk by inputing its dictionary key
            PoolChunk(keys[x]);
        }
    }

    // -----------------------------------------------------------------------------------------------------
    
    /// <summary> Generates 2D noise with octaves </summary>
    public float OctaveNoise(float x, float y, float time, NoiseSettings noiseSettings)
    {
        float output = 0;

        float positionX = x + noiseSettings.xSeed;
        float positionY = y + noiseSettings.ySeed;

        float xCoord;
        float yCoord;

        // For all octaves.
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            // Value that moves an axis in a direction at a set speed
            float waveOffsetX = time * (noiseSettings.waveSpeeds[i] * windDirection.x);
            float waveOffsetY = time * (noiseSettings.waveSpeeds[i] * windDirection.z);

            xCoord = (positionX + noiseSettings.octaveOffsets[i].x) / noiseSettings.scale * noiseSettings.frequencies[i] + waveOffsetX;
            yCoord = (positionY + noiseSettings.octaveOffsets[i].y) / noiseSettings.scale * noiseSettings.frequencies[i] + waveOffsetY;

            //output += GerstnerNoise.GerstnerNoise2D(xCoord, yCoord, waveOffset, currentFrequency, currentAmplitude);  // BROKEN

            output += (float)simplexNoise.SimplexNoise2D(xCoord, yCoord) * noiseSettings.amplitudes[i];/*
            output += (float)simplexNoise.SimplexNoise3D(xCoord, yCoord, 0) * noiseSettings.amplitudes[i];
            output += OpenSimplex2.Noise2_UnskewedBase(worldSeed, xCoord, yCoord) * noiseSettings.amplitudes[i];
            output += fastNoiseLite.GetNoise(xCoord, yCoord) * noiseSettings.amplitudes[i];
            output += terrainManager.fastNoise2.GenSingle2D(xCoord, yCoord, worldSeed) * noiseSettings.amplitudes[i];
            output += SimplexNoiseAll.Noise.Generate(xCoord, yCoord) * noiseSettings.amplitudes[i]; */
        }

        return output;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Generates 3D noise with octaves </summary>
    public float OctaveNoise(float x, float y, float z, float time, NoiseSettings noiseSettings)
    {
        float output = 0;

        float currentAmplitude = noiseSettings.amplitude;
        float currentFrequency = noiseSettings.frequency;
        float currentWaveSpeed = noiseSettings.waveSpeed;

        Vector3 waveDirection;

        // For all octaves.
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            float waveOffset = time * currentWaveSpeed;
            waveDirection = windDirection * waveOffset;

            // !!!! OPTIMISE, precalculate repeated operations
            float xCoord = (x + noiseSettings.xSeed) / noiseSettings.scale * currentFrequency + noiseSettings.octaveOffsets[i].x + waveDirection.x;
            float yCoord = (y + noiseSettings.ySeed) / noiseSettings.scale * currentFrequency + noiseSettings.octaveOffsets[i].y + waveDirection.y;
            float zCoord = (z + noiseSettings.zSeed) / noiseSettings.scale * currentFrequency + noiseSettings.octaveOffsets[i].z + waveDirection.z;

            //output += simplexNoise.SimplexNoise3D(xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;
            //output += OpenSimplex2.Noise3_ImproveXZ(worldSeed, xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;
            //output += fastNoiseLite.GetNoise(xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;
            output += fastNoise2.GenSingle3D(xCoord, yCoord, zCoord, worldSeed) / noiseSettings.scale * currentFrequency * currentAmplitude;
            //output += SimplexNoiseAll.Noise.Generate(xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;

            currentAmplitude *= noiseSettings.persistance;
            currentFrequency *= noiseSettings.lacunarity;
            currentWaveSpeed /= noiseSettings.waveSubspeed;
        }

        return output;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Using a chunk key find any chunks at this position </summary>
    public Chunk GetChunk(Vector3Int chunkKey)
    {
        chunks.TryGetValue(chunkKey, out Chunk containerChunk);
        return containerChunk;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Converts a world position into a chunk index(key) as a Vector3int </summary>
    public Vector3Int FindChunkIndex(Vector3 worldPosition)
    {
        return Vector3Int.RoundToInt(worldPosition / baseChunkSize);
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Takes a chunk out of the pool or if none exist creates a new one </summary>
    public void SpawnChunk(Vector3Int chunkKey, int chunkSize, float unitSize)
    {
        Chunk chunk;

        switch (chunkPool.pool.Count)
        {
            // Draw a chunk out of the chunk pool
            default:
                chunk = chunkPool.Take();
                break;

            // The pool is empty so create a new chunk
            case 0:
                chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0)).GetComponent<Chunk>();
                break;
        }

        chunk.transform.position = chunkSize * unitSize * (Vector3)chunkKey;
        chunk.transform.SetParent(transform);

        chunk.transform.name = "Chunk (" + chunkKey + ")";

        chunk.xChunk = chunkKey.x;
        chunk.zChunk = chunkKey.z;

        chunk.chunkSize = chunkSize;
        chunk.noiseSize = chunkSize + 1;
        chunk.chunkUnitSize = unitSize;

        chunk. terrainManager = this;

        chunks.Add(chunkKey, chunk);

        chunk.Reload();
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Save then wipe the chunk and put into storage so we can use it later </summary>
    public void PoolChunk(Vector3Int chunkKey)
    {
        chunks.TryGetValue(chunkKey, out Chunk chunk);

        chunk.SaveAndClearChunk();

        chunk.transform.name = "Chunk (Null)";

        // Add to the pool
        chunkPool.Add(chunk);

        // Remove from the dictionary
        chunks.Remove(chunkKey);
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Completly destroys a chunk, this is not used typically </summary>
    public void DestroyChunk(Vector3Int chunkKey)
    {
        switch (chunks.TryGetValue(chunkKey, out Chunk chunk))
        {
            case true:
                Destroy(chunk.gameObject);
                chunks.Remove(chunkKey);
                break;
        }
    }
}