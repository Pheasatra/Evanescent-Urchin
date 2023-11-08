using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
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

    [Space(10)]

    public int totalFaces;
    public int totalTriangles;

    [Space(10)]

    public int chunkSize = 0;
    public float chunkUnitSize = 0;

    public int lodDistance = 0;

    public float[] noiseMemory; 

    [HideInInspector] public List<Vector3> vertices;
    [HideInInspector] public List<Vector2> uvs;
    [HideInInspector] public List<Color> colours;
    [HideInInspector] public List<int> triangles;

    // !!! Map each vector3 to a vertex on a cube, so for example top left, bottom forward, this lets use reuse some of these and also allows us to better understand this code
    public static VertexOffsets Top = new VertexOffsets(new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, -0.5f), new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f));
    public static VertexOffsets Bottom = new VertexOffsets(new Vector3(0.5f, 0.0f, -0.5f), new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, 0.5f));

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

        GenerateMesh();
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        GenerateMemory();
        UpdateMesh();
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
        //GenerateMesh();
    }    
    
    // -----------------------------------------------------------------------------------------------------
    
    // Generate the noise used to contruct the mesh
    void GenerateMemory()
    {
        Vector2Int index2D;

        // For the length of simplex noise
        for (int x = 0; x < noiseMemory.Length; x++)
        {
            // Use the 2d version, its far faster
            // !!!! https://github.com/WardBenjamin/SimplexNoise/blob/master/SimplexNoise/Noise.cs

            index2D = Index1Dto2D(x, chunkSize);
            noiseMemory[x] = terrainManager.OctaveSimplex3D(index2D.x + transform.position.x * chunkUnitSize, index2D.y + transform.position.y * chunkUnitSize, 0);
            /*
            switch (noise > terrainManager.visiblityLimit)
            {
                case false:
                    continue;
            }*/
        }
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Takes a flattened 2D array and scales it up or down depending on the new size variables, this duplicates the variables inside too so they fit snug into the new array </summary>
    public int[] ScaleArray(int[] originalArray, int originalSize, int newSize)
    {
        int[] scaledArray = new int[newSize * newSize];

        // Get the ratio between each array, we round it to int for good measure just in case the sizes are not perfectly divisible
        float sizeRatio = (float)originalSize / newSize;

        // For all elements in the new array
        for (int x = 0; x < newSize; x++)
        {
            for (int y = 0; y < newSize; y++)
            {
                // Calculate the corresponding position in the original array
                int originalX = (int)(x * sizeRatio);
                int originalY = (int)(y * sizeRatio);

                // Copy the scaled value
                scaledArray[Index2Dto1D(x, y, newSize)] = originalArray[Index2Dto1D(originalX, originalY, originalSize)];
            }
        }

        return scaledArray;
    }

    // -----------------------------------------------------------------------------------------------------

    void UpdateMesh()
    {
        int vIndex = 0;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // Ok so a little complex, we are making every vertex share noise values by making each face 'tile' by 1 instead of two, we can plus one the noiseMemory since it has that + 1 to it's size from spawning
                // These constant numbers in Index2Dto1D represent the position of each vertex, by adding on a axis positions of 1 we cam make the vertex jump to the next block if you know what I mean
                // Look at the wireframe at try visualising how this works to wrap your head around it
                vertices[vIndex + 0] = new Vector3(vertices[vIndex + 0].x, noiseMemory[Index2Dto1D(0 + x, 0 + y, chunkSize + 1)], vertices[vIndex + 0].z);
                vertices[vIndex + 1] = new Vector3(vertices[vIndex + 1].x, noiseMemory[Index2Dto1D(1 + x, 0 + y, chunkSize + 1)], vertices[vIndex + 1].z);
                vertices[vIndex + 2] = new Vector3(vertices[vIndex + 2].x, noiseMemory[Index2Dto1D(0 + x, 1 + y, chunkSize + 1)], vertices[vIndex + 2].z);
                vertices[vIndex + 3] = new Vector3(vertices[vIndex + 3].x, noiseMemory[Index2Dto1D(1 + x, 1 + y, chunkSize + 1)], vertices[vIndex + 3].z);

                //colours[vIndex + 0] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                //colours[vIndex + 1] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                //colours[vIndex + 2] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                //colours[vIndex + 3] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));

                vIndex += 4;
            }
        }
        
        // !!! Test if that artifact is still there using sine
        
        // For all vertices
        for (int x = 0; x < vertices.Count; x++)
        {
            vertices[x] = new Vector3(vertices[x].x, Mathf.Sin(vertices[x].x + vertices[x].z + Time.timeSinceLevelLoad), vertices[x].z);
        }
        

        mesh.SetVertices(vertices);
        //mesh.SetColors(colours);

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Takes in a world position and rounds it into chunk space, then it finds the closest noise point and returns it </summary>
    public float GetNoiseLocal(Vector2 position)
    {
        // Nullify chunk stretching effects, then round to int
        Vector2Int roundPosition = Vector2Int.RoundToInt(position / chunkUnitSize);
        return noiseMemory[Index2Dto1D(roundPosition.x, roundPosition.y, chunkSize)];
    }

    // -----------------------------------------------------------------------------------------------------

    // Generates the chunk mesh
    void GenerateMesh()
    {
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
                startPosition = new Vector3Int(x, 0, y);

                BuildFace(Top, startPosition, chunkUnitSize);
                //BuildFace(Bottom, startPosition, chunkUnitSize);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(colours);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0); // submeshing!

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
        meshFilter.mesh.MarkDynamic();
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Builds a face based on a position and an offset for each vertex which aim it in a direction. 
    /// There are also two optional stretching variables 'faceStretch' which is good for greedy mesh faces, and 'chunkStretch' which stretches everything leading to a larger looking chunk </summary>
    public void BuildFace(VertexOffsets offset, Vector3Int startPosition, float chunkStretch = 1)
    {
        vertices.Add((startPosition + offset.vertex1) * chunkStretch);
        vertices.Add((startPosition + offset.vertex2) * chunkStretch);
        vertices.Add((startPosition + offset.vertex3) * chunkStretch);
        vertices.Add((startPosition + offset.vertex4) * chunkStretch);
        /*
        colours.Add(new Color(startPosition.x, startPosition.y, 0) / chunkSize);
        colours.Add(new Color(startPosition.x, startPosition.y, 0) / chunkSize);
        colours.Add(new Color(startPosition.x, startPosition.y, 0) / chunkSize);
        colours.Add(new Color(startPosition.x, startPosition.y, 0) / chunkSize);
        */
        /*
        int index = Index2Dto1D(startPosition.x, startPosition.y);
        
        float uvX = noiseMemory[index].uvPosition.x * textureRatio;
        float uvY = noiseMemory[index].uvPosition.y * textureRatio;
        
        float uvX = 1 * textureRatio;
        float uvY = 4 * textureRatio;

        uvs.Add(new Vector2(uvX, uvY + textureRatio));
        uvs.Add(new Vector2(uvX + textureRatio, uvY + textureRatio));
        uvs.Add(new Vector2(uvX + textureRatio, uvY));
        uvs.Add(new Vector2(uvX, uvY));
        */
        // Temporary, maps one texture to each quad
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));

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
        totalFaces = 0;
        totalTriangles = 0;

        xChunk = 0;
        yChunk = 0;

        //Array.Clear(noiseMemory, 0, noiseMemory.Length);
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Takes in a 3D index and flattens them into the index for a 1D array </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Index3Dto1D(int x, int y, int z, int array1DSize)
    {
        return (z * array1DSize * array1DSize) + (y * array1DSize) + x;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Takes in a 2D index and flattens them into the index for a 1D array </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Index2Dto1D(int x, int y, int array1DSize)
    {
        return (y * array1DSize) + x;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Take in a 1D index for a flattened 1D array and expands them into a 2D index </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int Index1Dto2D(int index, int array1DSize)
    {
        int y = index / array1DSize;
        int x = index % array1DSize;

        return new Vector2Int(x, y);
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Take in a 1D index for a flattened 1D array and expands them into a 3D index  </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3Int Index1Dto3D(int index, int array1DSize)
    {
        int z = index / (array1DSize * array1DSize);
        index -= z * array1DSize * array1DSize;
        int y = index / array1DSize;
        int x = index % array1DSize;

        return new Vector3Int(x, y, z);
    }

    // -----------------------------------------------------------------------------------------------------

    // Manually take out the trash (some things do not get garbage collected)
    private void OnDestroy()
    {
        Destroy(mesh);
        Destroy(meshFilter);
    }

    // -----------------------------------------------------------------------------------------------------

    // Debug assistance
    void OnDrawGizmos()
    {
        // debug
        for (int x = 0; x < debug.Count; x++)
        {
            Handles.color = Color.green;
            Handles.DrawWireCube(debug[x] + transform.position, new Vector3(chunkUnitSize, chunkUnitSize, chunkUnitSize) / 4);
        }
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
    public Vector3 vertex4;

    // -----------------------------------------------------------------------------------------------------

    public VertexOffsets(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 vertex4)
    {
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        this.vertex3 = vertex3;
        this.vertex4 = vertex4;
    }
}