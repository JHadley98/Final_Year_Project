using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Height map and noise class

public class MapGenerator : MonoBehaviour
{
	public Terrain _terrain;
	// Start is called before the first frame update
	void Start()
    {
		int sampleX;
		int sampleY;
		float[,] heights;

		
		
		sampleX = _terrain.terrainData.heightmapWidth;
		sampleY = _terrain.terrainData.heightmapHeight;
		heights = _terrain.terrainData.GetHeights(0, 0, sampleX, sampleY);

		for (int y = 0; y < sampleY; y++)
		{
			for (int x = 0; x < sampleX; x++)
			{
				heights[x, y] = Random.Range(0, 1);
			}
		}
		
		_terrain.terrainData.SetHeights(0, 0, heights);
	}
	 
    // Update is called once per frame
    void Update()
    {
        
    }

	public void HeightMapSettings()
	{
		
	}
}
