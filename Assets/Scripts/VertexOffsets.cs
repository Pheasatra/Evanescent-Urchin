using UnityEngine;

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