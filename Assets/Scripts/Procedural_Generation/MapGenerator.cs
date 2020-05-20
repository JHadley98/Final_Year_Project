using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Height map and noise class

public class MapGenerator : MonoBehaviour
{
    public Terrain _terrain; // terrain to modify

    void Start()
    {
        int sizeToUpdate = (int)_terrain.terrainData.size.x / 2;
        float[,] heights = _terrain.terrainData.GetHeights(0, 0, sizeToUpdate, sizeToUpdate);

        // we set each sample of the terrain in the size to the desired height
        for (int i = 0; i < sizeToUpdate; i++)
        {
            for (int j = 0; j < sizeToUpdate; j++)
            {
                heights[i, j] = Random.value / 300;
            }

        }
        // set the new height
        _terrain.terrainData.SetHeights(0, 0, heights);
    }
}
