using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    [System.Serializable]
    public class SplatHeights
    {
        // Store index of texture
        public int textureIndex;
        // Store starting height of texture
        public int startingHeight;
        // Texture overlap
        public int overlap;
    }

    // Array to store alpha values of texture
    public SplatHeights[] splatMap;

    // Normalise vector used to normalise splat map
    float[] normalise(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
        return v;
    }

    // Map function
    // Takes a value then wants to know that original values range
    // Between the min and max
    // Then you give it the new range that it will go through
    public float Map(float value, float minValue, float maxValue, float minRange, float maxRange)
    {
        return (value - minValue) * (maxRange - minRange) / (maxValue - minValue) + minRange;
    }

    // Start is called before the first frame update

    public void SplatMap(Terrain _terrain)
    {
        TerrainData _terrainData = _terrain.terrainData;

        // 3D float array for the alpha width and height and the number of layers add to the terrain
        float[,,] splatmapData = new float[_terrainData.alphamapWidth, _terrainData.alphamapHeight, _terrainData.alphamapLayers];

        // Loop through the height and width of terrain per vertex at a time
        for (int y = 0; y < _terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < _terrainData.alphamapWidth; x++)
            {
                // Find height at any particular point, multiplication and division of heightmap and alphamap heights to allow for a different scale
                float terrainHeight = _terrainData.GetHeight((y * _terrainData.heightmapHeight / _terrainData.alphamapHeight), (x * _terrainData.heightmapWidth / _terrainData.alphamapWidth));
                // Splat array, for all the different alpha values used in each texture
                float[] splatMap = new float[this.splatMap.Length];

                // Clamp noise
                float thisNoise = Map(Mathf.PerlinNoise(x * 0.05f, y * 0.05f), 0, 1, 0.5f, 1);
                //thisNoise = 1.0f;
                // Loop through all the height values
                for (int i = 0; i < this.splatMap.Length; i++)
                {
                    // This is the start height of the texture we are working with
                    // This equals the splatheights starting height minus the splatheights ovelap
                    // This will allow the texture come down a little bit on the map overlapping the texture above and below
                    float thisHeightStart = (this.splatMap[i].startingHeight - this.splatMap[i].overlap) * thisNoise;

                    // This is the starting position of the next texture starting at 0
                    float thisHeightEnd = 1000;
                    // This will only be used if there is another texture
                    // If we are working with textures that are below the top most texture then calculate height start
                    if (i != this.splatMap.Length - 1)
                    {
                        // The overlap is added this time so that a band overlap is created above and below the height set before for the texture
                        thisHeightEnd = (this.splatMap[i + 1].startingHeight + this.splatMap[i].overlap) * thisNoise;
                    }

                    // If it's the last texture and it's at the correct height then set it one
                    if (i == this.splatMap.Length - 1 && terrainHeight >= thisHeightStart)
                    {
                        splatMap[i] = 1;
                    }
                    // Else make sure it is between the texture and the next tuxture in the list
                    else if (terrainHeight >= thisHeightStart && terrainHeight <= thisHeightEnd)
                    {
                        splatMap[i] = 1;
                    }
                    else
                    {
                        splatMap[i] = 0;
                    }
                }

                // Normalise splat valus
                splatMap = normalise(splatMap);

                // Loop through all the splat values in the terrainHeight float array
                for (int j = 0; j < this.splatMap.Length; j++)
                {
                    // Set splatmapData at x and y for height and at position j will have all the splat values for each texture
                    splatmapData[x, y, j] = splatMap[j];
                }
            }
        }
        // Apply splatmapdata to terrain
        _terrainData.SetAlphamaps(0, 0, splatmapData);
    }


    public void PartialSplatMap(TerrainData _terrainData, int startX, int startY, int mapWidth, int mapHeight)
    {


        // 3D float array for the alpha width and height and the number of layers add to the terrain
        float[,,] splatmapData = new float[mapWidth, mapHeight, _terrainData.alphamapLayers];

        // Loop through the height and width of terrain per vertex at a time
        for (int y = startY; y < startY + mapHeight; y++)
        {
            for (int x = startX; x < startX + mapWidth; x++)
            {
                // Find height at any particular point, multiplication and division of heightmap and alphamap heights to allow for a different scale
                float terrainHeight = _terrainData.GetHeight((y * _terrainData.heightmapHeight / _terrainData.alphamapHeight), (x * _terrainData.heightmapWidth / _terrainData.alphamapWidth));
                // Splat array, for all the different alpha values used in each texture
                float[] splatMap = new float[this.splatMap.Length];

                // Clamp noise
                float thisNoise = Map(Mathf.PerlinNoise((x - startX) * 0.05f, (y - startY) * 0.05f), 0, 1, 0.5f, 1);
                // thisNoise = 1.0f;
                // Loop through all the height values
                for (int i = 0; i < this.splatMap.Length; i++)
                {
                    // This is the start height of the texture we are working with
                    // This equals the splatheights starting height minus the splatheights ovelap
                    // This will allow the texture come down a little bit on the map overlapping the texture above and below
                    float thisHeightStart = (this.splatMap[i].startingHeight - this.splatMap[i].overlap) * thisNoise;

                    // This is the starting position of the next texture starting at 0
                    float thisHeightEnd = 1000;
                    // This will only be used if there is another texture
                    // If we are working with textures that are below the top most texture then calculate height start
                    if (i != this.splatMap.Length - 1)
                    {
                        // The overlap is added this time so that a band overlap is created above and below the height set before for the texture
                        thisHeightEnd = (this.splatMap[i + 1].startingHeight + this.splatMap[i].overlap) * thisNoise;
                    }

                    // If it's the last texture and it's at the correct height then set it one
                    if (i == this.splatMap.Length - 1 && terrainHeight >= thisHeightStart)
                    {
                        splatMap[i] = 1;
                    }
                    // Else make sure it is between the texture and the next tuxture in the list
                    else if (terrainHeight >= thisHeightStart && terrainHeight <= thisHeightEnd)
                    {
                        splatMap[i] = 1;
                    }
                    else
                    {
                        splatMap[i] = 0;
                    }
                }

                // Normalise splat valus
                splatMap = normalise(splatMap);

                // Loop through all the splat values in the terrainHeight float array
                for (int j = 0; j < this.splatMap.Length; j++)
                {
                    // Set splatmapData at x and y for height and at position j will have all the splat values for each texture
                    splatmapData[x - startX, y - startY, j] = splatMap[j];
                }
            }
        }
        // Apply splatmapdata to terrain
        _terrainData.SetAlphamaps(startY, startX, splatmapData);
    }

}