using UnityEngine;

// -----------------------------------------------------------------------------------------------------

[System.Serializable]
public struct Visualise
{
    public Vector2 position;
    public Color colour;

    // -----------------------------------------------------------------------------------------------------

    public Visualise(Vector2 position, Color colour)
    {
        this.position = position;
        this.colour = colour;
    }
}