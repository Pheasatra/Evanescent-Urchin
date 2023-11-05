using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------------------------------------------

public class ChunkPool : MonoBehaviour
{
    public List<Chunk> pool;

    //--------------------------------------------------------------------------------------------------

    public void Add(Chunk chunk)
    {
        chunk.gameObject.SetActive(false);

        pool.Add(chunk);
    }

    //--------------------------------------------------------------------------------------------------

    public Chunk Take()
    {
        Chunk chunk = pool[0];
        pool.RemoveAt(0);

        chunk.gameObject.SetActive(true);

        return chunk;
    }
}