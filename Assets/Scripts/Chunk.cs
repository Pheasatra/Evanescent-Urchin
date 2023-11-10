using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

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

    public int chunkSize = 0;
    public float chunkUnitSize = 0;

    public int noiseSize = 0;

    public int lodDistance = 0;

    public float[] noiseMemory; 

    [HideInInspector] public Vector3[] vertices;
    [HideInInspector] public Vector2[] uvs;
    [HideInInspector] public Color[] colours;
    [HideInInspector] public int[] triangles;

    // !!! Map each vector3 to a vertex on a cube, so for example top left, bottom forward, this lets use reuse some of these and also allows us to better understand this code
    public static VertexOffsets Top = new VertexOffsets(new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, -0.5f), new Vector3(-0.5f, 0.0f, 0.5f), new Vector3(0.5f, 0.0f, 0.5f));
    public static VertexOffsets Bottom = new VertexOffsets(new Vector3(0.5f, 0.0f, -0.5f), new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, 0.5f));

    [HideInInspector] public List<Vector3> debug;
    [HideInInspector] public List<Vector3> debug2;

    public int verticeIndex = 0;
    public int triangleIndex = 0;   // Note that each 'triangle' is actually a winding value, meaning triangle count / 3 = true triangle count

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

        vertices = new Vector3[chunkSize * chunkSize * 4];  // 4 vertices per face
        uvs = new Vector2[vertices.Length];                 
        colours = new Color[vertices.Length];
        triangles = new int[chunkSize * chunkSize * 6];     // 6 triangle corners per face (for winding)

        RegenerateMesh();
        //InvokeRepeating(nameof(NoiseUpdate), 0.0f, 1.0f / terrainManager.noiseUpdatesPerSecond);
    }

    // -----------------------------------------------------------------------------------------------------
    /*
    public void NoiseUpdate()
    {
        GenerateMemory();
    }*/

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        GenerateMemory();
        UpdateMesh();
    }

    // -----------------------------------------------------------------------------------------------------

    // For refreshing things that change between chunks
    public void Reload()
    {
        //RegenerateMesh();
    }

    // -----------------------------------------------------------------------------------------------------

    // Generate the noise used to contruct the mesh
    public void GenerateMemory()
    {
        int xIndex;
        int yIndex;

        //Vector3 positionOffset = new Vector3(transform.position.x * chunkUnitSize, 0.0f, transform.position.z * chunkUnitSize);
        Vector3 positionOffset = new Vector3(xChunk * chunkSize * chunkUnitSize, 0.0f, yChunk * chunkSize * chunkUnitSize);

        // For the length of simplex noise
        for (int i = 0; i < noiseMemory.Length; i++)
        {
            // 1D to 2D index conversion
            yIndex = i / noiseSize;
            xIndex = i % noiseSize;

            //noiseMemory[i] = terrainManager.OctaveSimplex3D(xIndex + positionOffset.x, yIndex + positionOffset.z, 0);
            //noiseMemory[i] = terrainManager.OctaveSimplex2D(xIndex + positionOffset.x, yIndex + positionOffset.z);

            //noiseMemory[i] = Mathf.Sin(xIndex + transform.position.x + Time.timeSinceLevelLoad * 2);
            noiseMemory[i] = (xIndex + positionOffset.x + yIndex + positionOffset.z) / 4;
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

    public void UpdateMesh()
    {
        verticeIndex = 0;

        int verticesPlus0;
        int verticesPlus1;
        int verticesPlus2;  
        int verticesPlus3;

        // For all tiles
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // Ok so a little complex, we are making every vertex share noise values by making each face 'tile' by 1 instead of two, we can plus one the noiseMemory since it has that + 1 to it's size from spawning
                // These constant numbers in Index2Dto1D represent the position of each vertex, by adding on a axis positions of 1 we cam make the vertex jump to the next block if you know what I mean
                // Look at the wireframe at try visualising how this works to wrap your head around it

                // Precompute vertices, in case we use it twice
                verticesPlus0 = verticeIndex + 0;
                verticesPlus1 = verticeIndex + 1;
                verticesPlus2 = verticeIndex + 2;
                verticesPlus3 = verticeIndex + 3;

                // We simply change the y position to our noise value
                // Note: this version unpackeds Index2Dto1D as it is far more performant
                vertices[verticesPlus0].y = noiseMemory[((0 + y) * noiseSize) + 0 + x];
                vertices[verticesPlus1].y = noiseMemory[((0 + y) * noiseSize) + 1 + x];
                vertices[verticesPlus2].y = noiseMemory[((1 + y) * noiseSize) + 0 + x];
                vertices[verticesPlus3].y = noiseMemory[((1 + y) * noiseSize) + 1 + x];

                //colours[verticesPlus0] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                //colours[verticesPlus1] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                //colours[verticesPlus2] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                //colours[verticesPlus3] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));

                verticeIndex += 4;
            }
        }

        mesh.SetVertices(vertices);
        //mesh.SetColors(colours);

        //mesh.RecalculateNormals();
        //mesh.RecalculateTangents(); // Relates to normals maps, we can get away with not using it here even though we do use them
    }

    // -----------------------------------------------------------------------------------------------------

    // Generates the chunk mesh
    public void RegenerateMesh()
    {
        verticeIndex = 0;
        triangleIndex = 0;

        mesh.Clear();

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
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Builds a face based on a position and an offset for each vertex which aim it in a direction. 
    /// There are also two optional stretching variables 'faceStretch' which is good for greedy mesh faces, and 'chunkStretch' which stretches everything leading to a larger looking chunk </summary>
    public void BuildFace(VertexOffsets offset, Vector3Int startPosition, float chunkStretch = 1)
    {
        // Precompute vertices 
        int verticesPlus0 = verticeIndex + 0;
        int verticesPlus1 = verticeIndex + 1;
        int verticesPlus2 = verticeIndex + 2;
        int verticesPlus3 = verticeIndex + 3;

        vertices[verticesPlus0] = (startPosition + offset.vertex1) * chunkStretch;
        vertices[verticesPlus1] = (startPosition + offset.vertex2) * chunkStretch;
        vertices[verticesPlus2] = (startPosition + offset.vertex3) * chunkStretch;
        vertices[verticesPlus3] = (startPosition + offset.vertex4) * chunkStretch;

        colours[verticesPlus0] = new Color(startPosition.x, startPosition.y, startPosition.z) / chunkSize;
        colours[verticesPlus1] = new Color(startPosition.x, startPosition.y, startPosition.z) / chunkSize;
        colours[verticesPlus2] = new Color(startPosition.x, startPosition.y, startPosition.z) / chunkSize;
        colours[verticesPlus3] = new Color(startPosition.x, startPosition.y, startPosition.z) / chunkSize;
        
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
        uvs[verticesPlus0] = new Vector2(0, 0);
        uvs[verticesPlus1] = new Vector2(0, 1);
        uvs[verticesPlus2] = new Vector2(1, 0);
        uvs[verticesPlus3] = new Vector2(1, 1);

        // Triangle 1 and 2 for this quad
        // We take the winding value and add the vertices count minus the 4 vertices for each face, the following comments are the winding order
        triangles[triangleIndex + 0] = verticesPlus1;    // 1
        triangles[triangleIndex + 1] = verticesPlus2;    // 2
        triangles[triangleIndex + 2] = verticesPlus3;    // 3
        triangles[triangleIndex + 3] = verticesPlus0;    // 0
        triangles[triangleIndex + 4] = verticesPlus2;    // 2
        triangles[triangleIndex + 5] = verticesPlus1;    // 1

        // Add our vertice and winding counts for this face
        verticeIndex += 4;
        triangleIndex += 6;
    }

    // -----------------------------------------------------------------------------------------------------

    public void SaveAndClearChunk()
    {
        xChunk = 0;
        yChunk = 0;
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
    /*
    // Debug assistance
    void OnDrawGizmos()
    {
        // debug
        for (int x = 0; x < debug.Count; x++)
        {
            Handles.color = Color.green;
            Handles.DrawWireCube(debug[x] + transform.position, new Vector3(chunkUnitSize, chunkUnitSize, chunkUnitSize) / 4);
        }
    }*/
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