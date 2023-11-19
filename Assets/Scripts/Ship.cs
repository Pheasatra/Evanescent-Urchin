using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.Rendering.DebugUI;

// -----------------------------------------------------------------------------------------------------

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Ship : MonoBehaviour
{
    [Header("References")]
    public ShipManager shipManager;
    public Mesh mesh;
    public MeshFilter meshFilter;

    [Header("Variables")]
    [Tooltip("If 1.0f was the textureSheet size, then this is the percentage that a single sprite on our texture takes up")]
    public float textureRatio = 0.125f;

    [Space(10)]

    public int xSize;
    public int ySize;
    public int zSize;

    [Space(10)]

    public float shipUnitSize = 1.0f;

    [Space(10)]

    public int verticesPerFace = 4;
    public int triangleCornersPerFace = 6;

    [HideInInspector] public Block[,,] blockMemory;

    [HideInInspector] public Vector3[] vertices;
    [HideInInspector] public Vector2[] uvs;
    [HideInInspector] public int[] triangles;

    // !!! Map each vector3 to a vertex on a cube, so for example top left, bottom forward, this lets use reuse some of these and also allows us to better understand this code
    public static VertexOffsets Left = new VertexOffsets(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f));
    public static VertexOffsets Right = new VertexOffsets(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f));
    public static VertexOffsets Top = new VertexOffsets(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));
    public static VertexOffsets Bottom = new VertexOffsets(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f));
    public static VertexOffsets Forward = new VertexOffsets(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));
    public static VertexOffsets Back = new VertexOffsets(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f));

    private int verticeIndex = 0;
    private int windingIndex = 0;   // Note that each 'triangle' is actually a winding value, meaning triangle count / 3 = true triangle count

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        shipManager = ShipManager.shipManager;

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

    // For refreshing things that change between chunks
    public void Reload()
    {
        int shipLength = xSize * ySize * zSize;

        // Set the vertice and block count to a full size chunk for speed
        vertices = new Vector3[shipLength * verticesPerFace];  // 4 vertices per face
        uvs = new Vector2[vertices.Length];
        triangles = new int[shipLength * triangleCornersPerFace];     // 6 triangle corners per face (for winding)

        blockMemory = new Block[xSize, ySize, zSize];

        GenerateMesh();
    }

    // -----------------------------------------------------------------------------------------------------

    // Generates the chunk mesh
    public void GenerateMesh()
    {
        //yield return new WaitForSeconds(0.00f);

        verticeIndex = 0;
        windingIndex = 0;

        mesh.Clear();

        Vector3Int startPosition;

        // For every block in the chunk
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    // Inline Index 1D
                    switch (blockMemory[x, y, z].blockType)
                    {
                        // If this block is not visible
                        case 0:
                            continue;
                    }

                    startPosition = new Vector3Int(x, y, z);

                    // Is the neighbouring block is going to larger than the chunk size
                    switch (x + 1 > xSize - 1)
                    {
                        // Look into this chunk
                        case false:
                            // Check if the neighbour block is air or a block
                            switch (blockMemory[x, y, z].blockType)
                            {
                                case 0:
                                    BuildFace(Left, startPosition, 1);
                                    break;
                            }
                            break;
                    }

                    switch (x - 1 < 0)
                    {
                        case false:
                            switch (blockMemory[x, y, z].blockType)
                            {
                                case 0:
                                    BuildFace(Right, startPosition, 1);
                                    break;
                            }
                            break;
                    }

                    // Y axis
                    switch (y + 1 > ySize - 1)
                    {
                        case false:
                            switch (blockMemory[x, y, z].blockType)
                            {
                                case 0:
                                    BuildFace(Top, startPosition, 1);
                                    break;
                            }
                            break;
                    }

                    switch (y - 1 < 0)
                    {
                        case false:
                            switch (blockMemory[x, y, z].blockType)
                            {
                                case 0:
                                    BuildFace(Bottom, startPosition, 1);
                                    break;
                            }
                            break;
                    }

                    // Z axis
                    switch (z + 1 > zSize - 1)
                    {
                        case false:
                            switch (blockMemory[x, y, z].blockType)
                            {
                                case 0:
                                    BuildFace(Forward, startPosition, 1);
                                    break;
                            }
                            break;
                    }

                    switch (z - 1 < 0)
                    {
                        case false:
                            switch (blockMemory[x, y, z].blockType)
                            {
                                case 0:
                                    BuildFace(Back, startPosition, 1);
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
        //mesh.RecalculateTangents();
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Builds a face based on a position and an offset for each vertex which aim it in a direction. 
    /// There are also two optional stretching variables 'faceStretch' which is good for greedy mesh faces, and 'chunkStretch' which stretches everything leading to a larger looking chunk </summary>
    public void BuildFace(VertexOffsets offset, Vector3Int startPosition, float faceStretch = 1)
    {
        // Precompute vertices 
        int verticesPlus0 = verticeIndex + 0;
        int verticesPlus1 = verticeIndex + 1;
        int verticesPlus2 = verticeIndex + 2;
        int verticesPlus3 = verticeIndex + 3;

        vertices[verticesPlus0] = (startPosition + offset.vertex1 * faceStretch) * shipUnitSize;
        vertices[verticesPlus1] = (startPosition + offset.vertex2 * faceStretch) * shipUnitSize;
        vertices[verticesPlus2] = (startPosition + offset.vertex3 * faceStretch) * shipUnitSize;
        vertices[verticesPlus3] = (startPosition + offset.vertex4 * faceStretch) * shipUnitSize;

        float uvX = blockMemory[startPosition.x, startPosition.y, startPosition.z].uvPosition.x * textureRatio;
        float uvY = blockMemory[startPosition.x, startPosition.y, startPosition.z].uvPosition.y * textureRatio;

        uvs[verticesPlus0] = new Vector2(uvX, uvY + textureRatio);
        uvs[verticesPlus1] = new Vector2(uvX + textureRatio, uvY + textureRatio);
        uvs[verticesPlus2] = new Vector2(uvX + textureRatio, uvY);
        uvs[verticesPlus3] = new Vector2(uvX, uvY);

        triangles[windingIndex + 0] = verticesPlus1;    // 1
        triangles[windingIndex + 1] = verticesPlus2;    // 2
        triangles[windingIndex + 2] = verticesPlus3;    // 3
        triangles[windingIndex + 3] = verticesPlus0;    // 0
        triangles[windingIndex + 4] = verticesPlus2;    // 2
        triangles[windingIndex + 5] = verticesPlus1;    // 1

        verticeIndex += 4;
        windingIndex += 6;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary>  </summary>
    public Block GetBlock(int x, int y, int z)
    {
        Block block = new Block();

        switch (x >= 0 && x < xSize)
        {
            // X out of range
            case false:
                return block;
        }

        switch (y >= 0 && y < ySize)
        {
            // Y out of range
            case false:
                return block;
        }

        switch (z >= 0 && z < zSize)
        {
            // Z out of range
            case false:
                return block;
        }

        return blockMemory[x, y, z];
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary>  </summary>
    public void SetBlock(int x, int y, int z, Block newBlock)
    {
        switch (x >= 0 && x < xSize)
        {
            // X out of range
            case false:
                return;
        }

        switch (y >= 0 && y < ySize)
        {
            // Y out of range
            case false:
                return;
        }

        switch (z >= 0 && z < zSize)
        {
            // Z out of range
            case false:
                return;
        }

        blockMemory[x, y, z] = newBlock;
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary>  </summary>
    public void SaveAndClearChunk()
    {
        //TrySave();

        Array.Clear(blockMemory, 0, blockMemory.Length);

        //filePath = "NULL";
        //zipPath = "NULL";
    }

    // -----------------------------------------------------------------------------------------------------

    // Manually take out the trash (some things do not get garbage collected)
    private void OnDestroy()
    {
        Destroy(mesh);
        Destroy(meshFilter);
    }
}