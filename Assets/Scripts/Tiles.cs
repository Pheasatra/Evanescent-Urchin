using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

[System.Serializable]
public struct TileType
{
    public string name;

    [Space(10)]

    [Tooltip("From the previous range to the current, sets the max height this tile will appear")]
    public float heightRange;
    public Color colour;
}

// -----------------------------------------------------------------------------------------------------

public struct Tile
{
    public byte tildId;
    public float height;
}