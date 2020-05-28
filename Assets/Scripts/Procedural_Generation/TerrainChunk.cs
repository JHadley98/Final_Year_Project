using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    private TerrainData terrainData;

    public Vector2 terrainPos;
    public Vector2 terrainCentre;

    public TerrainChunk(Vector2 terrainPos, TerrainData terrainData)
    {
        this.terrainPos = terrainPos;
        this.terrainData = terrainData;

        terrainCentre = new Vector2(terrainData.size.x / 2, terrainData.size.y / 2);

    }

    public void HeightMapSettings()
    {
        int sampleX;
        int sampleY;
        float[,] heights;

        // Initise terrain
        Terrain _terrain = new Terrain();

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
}