using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    [Header("References")]
    public TerrainManager terrainManager;
    public Mesh mesh;
    public MeshFilter meshFilter;

    [Header("Variables")]
    [Tooltip("If 1.0f was the textureSheet size, then this is the percentage that a single sprite on our texture takes up")]
    public float textureRatio = 0.125f;

    [Space(10)]

    public int xChunk = 0;
    public int yChunk = 0;
    public int zChunk = 0;

    [Space(10)]

    public int totalFaces;
    public int totalTriangles;

    [Space(10)]

    public int chunkSize = 0;
    public float chunkUnitSize = 0;

    [HideInInspector] public Block[] blockMemory; // convert to a single array to avoid cache misses, especially with greedy meshing

    [HideInInspector] public List<Vector3> vertices;
    [HideInInspector] public List<Vector2> uvs;
    [HideInInspector] public List<int> triangles;

    // !!! Map each vector3 to a vertex on a cube, so for example top left, bottom forward, this lets use reuse some of these and also allows us to better understand this code
    public static VertexOffsets Left = new VertexOffsets(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f));
    public static VertexOffsets Right = new VertexOffsets(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f));
    public static VertexOffsets Top = new VertexOffsets(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));
    public static VertexOffsets Bottom = new VertexOffsets(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f));
    public static VertexOffsets Forward = new VertexOffsets(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));
    public static VertexOffsets Back = new VertexOffsets(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f));

    private Vector3Int startPoint;
    private Vector3Int endPoint;
    private bool passedChecks;
    private Vector3Int faceOffset;

    [HideInInspector] public List<Vector3> debug;
    [HideInInspector] public List<Vector3> debug2;

    // Start is called before the first frame update
    void Start()
    {
        terrainManager = TerrainManager.terrainManager;        
        
        // Mesh setup 
        mesh = new Mesh();
        mesh.MarkDynamic();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {

    }

    // -----------------------------------------------------------------------------------------------------

    // Can be upgrade to wait for frames instead of time
    public void DelayedReload(int framesToWait)
    {
        float frameTime = 1.0f / Application.targetFrameRate;
        Invoke(nameof(Reload), frameTime * framesToWait);
    }

    // -----------------------------------------------------------------------------------------------------

    // For refreshing things that change between chunks
    public void Reload()
    {
        GenerateMemory();
        GenerateMesh();
    }    
    
    // -----------------------------------------------------------------------------------------------------
    
    // Generate the noise used to contruct the mesh
    void GenerateMemory()
    {
        // Get the raw noise data in 1D form
        //float[] simplexNoiseRaw = terrainManager.simplexNoise.SimplexNoise3D(new Vector3(xChunk, yChunk, zChunk) * chunkSize, chunkSize);
        float noise;
        Vector3Int index3D;

        // For the length of simplex noise
        for (int x = 0; x < blockMemory.Length; x++)
        {
            // Use the 2d version, its far faster
            // !!!! https://github.com/WardBenjamin/SimplexNoise/blob/master/SimplexNoise/Noise.cs

            index3D = Index3D(x);
            noise = terrainManager.simplexNoise.SimplexNoise3D(index3D.x, index3D.y, index3D.z);

            switch (noise > terrainManager.visiblityLimit)
            {
                case false:
                    continue;
            }

            blockMemory[x] = terrainManager.blockVariants[UnityEngine.Random.Range(1, 3)];
        }
    }

    // -----------------------------------------------------------------------------------------------------

    // Generates the chunk mesh
    void GenerateMesh()
    {
        //yield return new WaitForSeconds(0.00f);

        mesh.Clear();

        // Create lists for all the vertives, uvs and triangles in case they already exist
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();

        Vector3Int startPosition;

        // For every block in the chunk
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    switch (blockMemory[Index1D(x, y, z)].blockType)
                    {
                        // If this block is not visible
                        case 0:
                            continue;
                    }

                    startPosition = new Vector3Int(x, y, z);

                    // Is the neighbouring block is going to larger than the chunk size
                    switch (x + 1 > chunkSize - 1)
                    {
                        // Look into this chunk
                        case false:
                            // Check if the neighbour block is air or a block
                            switch (blockMemory[Index1D(x + 1, y, z)].blockType)
                            {
                                case 0:
                                    BuildFace(Left, startPosition, 1, chunkUnitSize);
                                    break;
                            }
                            break;
                    }

                    switch (x - 1 < 0)
                    {
                        case false:
                            switch (blockMemory[Index1D(x - 1, y, z)].blockType)
                            {
                                case 0:
                                    BuildFace(Right, startPosition, 1, chunkUnitSize);
                                    break;
                            }
                            break;
                    }

                    // Y axis
                    switch (y + 1 > chunkSize - 1)
                    {
                        case false:
                            switch (blockMemory[Index1D(x, y + 1, z)].blockType)
                            {
                                case 0:
                                    BuildFace(Top, startPosition, 1, chunkUnitSize);
                                    break;
                            }
                            break;
                    }

                    switch (y - 1 < 0)
                    {
                        case false:
                            switch (blockMemory[Index1D(x, y - 1, z)].blockType)
                            {
                                case 0:
                                    BuildFace(Bottom, startPosition, 1, chunkUnitSize);
                                    break;
                            }
                            break;
                    }

                    // Z axis
                    switch (z + 1 > chunkSize - 1)
                    {
                        case false:
                            switch (blockMemory[Index1D(x, y, z + 1)].blockType)
                            {
                                case 0:
                                    BuildFace(Forward, startPosition, 1, chunkUnitSize);
                                    break;
                            }
                            break;
                    }

                    switch (z - 1 < 0)
                    {
                        case false:
                            switch (blockMemory[Index1D(x, y, z - 1)].blockType)
                            {
                                case 0:
                                    BuildFace(Back, startPosition, 1, chunkUnitSize);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        //mesh.Optimize();
        //mesh.OptimizeIndexBuffers();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
        meshFilter.mesh.MarkDynamic();
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Builds a face based on a position and an offset for each vertex which aim it in a direction. 
    /// There are also two optional stretching variables 'faceStretch' which is good for greedy mesh faces, and 'chunkStretch' which stretches everything leading to a larger looking chunk </summary>
    public void BuildFace(VertexOffsets offset, Vector3Int startPosition, float faceStretch = 1, float chunkStretch = 1)
    {
        vertices.Add((startPosition + offset.vertex1 * faceStretch) * chunkStretch);
        vertices.Add((startPosition + offset.vertex2 * faceStretch) * chunkStretch);
        vertices.Add((startPosition + offset.vertex3 * faceStretch) * chunkStretch);
        vertices.Add((startPosition + offset.vertex4 * faceStretch) * chunkStretch);

        int index = Index1D(startPosition.x, startPosition.y, startPosition.z);

        float uvX = blockMemory[index].uvPosition.x * textureRatio;
        float uvY = blockMemory[index].uvPosition.y * textureRatio;

        uvs.Add(new Vector2(uvX, uvY + textureRatio));
        uvs.Add(new Vector2(uvX + textureRatio, uvY + textureRatio));
        uvs.Add(new Vector2(uvX + textureRatio, uvY));
        uvs.Add(new Vector2(uvX, uvY));

        int vertexCount = vertices.Count;

        triangles.Add(1 + vertexCount - 4);
        triangles.Add(2 + vertexCount - 4);
        triangles.Add(3 + vertexCount - 4);
        triangles.Add(0 + vertexCount - 4);
        triangles.Add(2 + vertexCount - 4);
        triangles.Add(1 + vertexCount - 4);

        totalFaces += 1;
        totalTriangles += 2;
    }

    // -----------------------------------------------------------------------------------------------------

    public void SaveAndClearChunk()
    {
        //TrySave();

        // does nothing
        /*
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();*/

        totalFaces = 0;
        totalTriangles = 0;

        xChunk = 0;
        yChunk = 0;
        zChunk = 0;

        Array.Clear(blockMemory, 0, blockMemory.Length);

        //filePath = "NULL";
        //zipPath = "NULL";
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Take in the three 3D coordinates and flattens them into the index for a 1D array </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Index1D(int x, int y, int z)
    {
        return (z * chunkSize * chunkSize) + (y * chunkSize) + x;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Take in the 1D coordinates for a flattened 1D array and expands them into an array of 3d coordinates </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3Int Index3D(int index)
    {
        int z = index / (chunkSize * chunkSize);
        index -= z * chunkSize * chunkSize;
        int y = index / chunkSize;
        int x = index % chunkSize;

        return new Vector3Int(x, y, z);
    }

    // -----------------------------------------------------------------------------------------------------

    // Manually take out the trash (some things do not get garbage collected)
    private void OnDestroy()
    {
        Destroy(mesh);
        Destroy(meshFilter);
    }
}

// -----------------------------------------------------------------------------------------------------

[System.Serializable]
public struct VertexOffsets
{
    [Tooltip("Top Left")]
    public Vector3 vertex1;
    public Vector3 vertex2;
    public Vector3 vertex3;
    public Vector3 vertex4;// !!!PLACEHOLDERS

    // -----------------------------------------------------------------------------------------------------

    public VertexOffsets(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 vertex4)
    {
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        this.vertex3 = vertex3;
        this.vertex4 = vertex4;
    }
}