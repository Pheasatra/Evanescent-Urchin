using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

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

    //public List<Block> blockVariants = new List<Block>();

    [Header("Seeds")]
    public int seedRange = 32767;

    [Space(10)]

    public bool manualSeeds = false;
    public int worldSeed = 0;

    [Space(10)]

    public int xSeed = 0;
    public int ySeed = 0;
    public int zSeed = 0;

    [Header("Noise")]
    [Tooltip("How many layers of noise do you want to ovelap?")]
    public int octaves;
    public int noiseUpdatesPerSecond = 165;

    [Space(10)]

    public float scale = 0.075f;
    public float amplitude = 1.0f;
    public float frequency = 1.0f;
    public float waveSpeed = 1.0f;

    [Space(10)]

    // Our octaves sub waves, we don't do subscale as this offsets everything
    public float persistance = 1.0f;
    public float lacunarity = 1.0f;
    public float waveScale = 1.0f;

    public Vector3[] octaveOffsets;

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

        octaveOffsets = new Vector3[octaves];

        for (int x = 0; x < octaves; x++)
        {
            float offsetX = UnityEngine.Random.Range(-seedRange, seedRange);
            float offsetY = UnityEngine.Random.Range(-seedRange, seedRange);
            float offsetZ = UnityEngine.Random.Range(-seedRange, seedRange);

            octaveOffsets[x] = new Vector3(offsetX, offsetY, offsetZ);
        }

        switch (manualSeeds)
        {
            // Randomly generate seed
            case false:
                int dateTicks = (int)DateTime.Now.Ticks;

                worldSeed = UnityEngine.Random.Range(-dateTicks, dateTicks);

                xSeed = UnityEngine.Random.Range(-seedRange, seedRange);
                ySeed = UnityEngine.Random.Range(-seedRange, seedRange);
                zSeed = UnityEngine.Random.Range(-seedRange, seedRange);
                break;
        }

        // Set the primary world seed that will set the x, y, z seeds
        UnityEngine.Random.InitState(worldSeed);
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        scale = Math.Max(scale, 0.0001f);

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
    
    /// <summary> Generates Simplex noise in octaves </summary>
    public float OctaveSimplex2D(float x, float y)
    {
        float output = 0;

        float currentAmplitude = amplitude;
        float currentFrequency = frequency;
        float currentWaveSpeed = waveSpeed;

        //Loop through all octaves.
        for (int i = 0; i < octaves; i++)
        {
            float waveOffset = Time.timeSinceLevelLoad * currentWaveSpeed;
            //float frequencyScale = scale * currentFrequency;

            // !!!! OPTIMISE removes repeated calculations
            float xCoord = (x + xSeed) / scale * currentFrequency + octaveOffsets[i].x + waveOffset;
            float yCoord = (y + ySeed) / scale * currentFrequency + octaveOffsets[i].y + waveOffset;

            // !!! Fix the tiling issue

            output += OpenSimplex2.Noise2_UnskewedBase(worldSeed, xCoord, yCoord) / scale * currentFrequency * currentAmplitude;  // FASTEST NON-SIMD
            //output += fastNoiseLite.GetNoise(xCoord, yCoord) / scale * currentFrequency * currentAmplitude;
            //output += terrainManager.fastNoise2.GenSingle2D(xCoord, yCoord, worldSeed) / scale * currentFrequency * currentAmplitude;
            //output += SimplexNoiseAll.Noise.Generate(xCoord, yCoord) / scale * currentFrequency * currentAmplitude;

            currentAmplitude *= persistance;
            currentFrequency *= lacunarity;
            currentWaveSpeed /= waveScale;
        }

        return output;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Generates Simplex noise in octaves </summary>
    public float OctaveSimplex3D(float x, float y, float z)
    {
        float output = 0;

        float currentAmplitude = amplitude;
        float currentFrequency = frequency;
        float currentWaveSpeed = waveSpeed;

        //Loop through all octaves.
        for (int i = 0; i < octaves; i++)
        {
            float waveOffset = Time.timeSinceLevelLoad * currentWaveSpeed;

            // !!!! OPTIMISE removes repeated calculations
            float xCoord = (x + xSeed) / scale * currentFrequency + octaveOffsets[i].x + waveOffset;
            float yCoord = (y + ySeed) / scale * currentFrequency + octaveOffsets[i].y + waveOffset;
            float zCoord = (z + zSeed) / scale * currentFrequency + octaveOffsets[i].z + waveOffset;

            //output += simplexNoise.SimplexNoise3D(xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;
            //output += OpenSimplex2.Noise3_ImproveXZ(worldSeed, xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;
            //output += fastNoiseLite.GetNoise(xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;
            output += fastNoise2.GenSingle3D(xCoord, yCoord, zCoord, worldSeed) / scale * currentFrequency * currentAmplitude;
            //output += SimplexNoiseAll.Noise.Generate(xCoord, yCoord, zCoord) / scale * currentFrequency * currentAmplitude;

            currentAmplitude *= persistance;
            currentFrequency *= lacunarity;
            currentWaveSpeed /= waveScale;
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
        chunk.yChunk = chunkKey.y;

        chunk.chunkSize = chunkSize;
        chunk.noiseSize = chunkSize + 1;
        chunk.chunkUnitSize = unitSize;

        // Set the noise memory size to be (chunk size + 1)^2 to account for the vertices on the very edges
        chunk.noiseMemory = new float[(chunkSize + 1) * (chunkSize + 1)];

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