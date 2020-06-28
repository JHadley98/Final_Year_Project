using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class is used to generate the terrain assets as followed:
/// Three different tree types across different heights throughout the terrain
/// Grass assets above the grass textures
/// Water objects that will be above sand sections of the terrain making mini beaches/lakes
/// </summary>
public class TerrainAssetGenerator : MonoBehaviour
{
    private TreeInstance treeInstance;

    private float terrainHeight;
    private float treeDensity;
    private float treeRandomPosition;

    public GameObject water;
    public Texture2D grassSprite;

    public void CreateWater(Terrain _terrain)
    {
        // Instantiate the water object, setting it's location to equal the terrains x, and z size, while settings it Y to equal 6 for the height of the water object to appear in the game
        Instantiate(water, new Vector3(_terrain.terrainData.size.x / 2 + _terrain.transform.position.x, 6, _terrain.terrainData.size.z / 2 + _terrain.transform.position.z), Quaternion.identity);
    }

    public IEnumerator CreateGrass(Terrain _terrain, int terrainCount)
    {
        // Dry colour = R: 205, G: 188, B: 16, A: 255, convert to decimal divide by 255 as ranges needs to be 0 - 1 instead of range 0 - 255
        // Dry colour in decimals then equals new colour (R: 0.8f, G: 0.73f, B: 0.06, A: 1)
        // Healthy colour = R: 67, G: 249, B: 42, A: 255, convert to decimal divide by 255 as ranges needs to be 0 - 1 instead of range 0 - 255
        // Healthy colour in decimal then equals new colour (R: 0.26, G:0.97, 0.16, A: 1)
        DetailPrototype detailPrototype = GrassPrototypeForTexture(grassSprite, new Color(0.8f, 0.73f, 0.06f, 1), new Color(0.26f, 0.97f, 0.16f, 1f));

        _terrain.terrainData.detailPrototypes = new DetailPrototype[1] { detailPrototype }; // Create new detail prototype passing in the GrassProtoTypeTexture
        _terrain.terrainData.wavingGrassSpeed = 0.5f;                                       // Set waving grass speed
        _terrain.terrainData.wavingGrassAmount = 0.5f;                                      // Set waving grass amount
        _terrain.terrainData.wavingGrassStrength = 0.5f;                                    // Set waving grass strength
        _terrain.terrainData.wavingGrassTint = new Color(0.45f, 0.45f, 0.45f, 1);           // Set waving grass tint

        // Set details to equal the terrain detail width and hieght
        int[,] details = new int[_terrain.terrainData.detailWidth, _terrain.terrainData.detailHeight];
        // Loop the through detail width and height
        for (int x = 0; x < _terrain.terrainData.detailWidth; x++)
        {
            for (int z = 0; z < _terrain.terrainData.detailHeight; z++)
            {
                // Set terrain height
                terrainHeight = _terrain.terrainData.GetHeight((int)(z * _terrain.terrainData.heightmapHeight / _terrain.terrainData.detailHeight),
                                                               (int)(x * _terrain.terrainData.heightmapWidth / _terrain.terrainData.detailWidth));

                // Set terrain height values between a set range in which the grass will be generated onto
                if (terrainHeight > 8 && terrainHeight < 17)
                {
                    // Set detail frequency for the grass
                    details[x, z] = 16;
                }
                // Else don't paint grass on to terrain
                else
                {
                    details[x, z] = 0;
                }
            }
            // yield used every 50 x's to maintain performance of player
            if (((float)x / 50 == x / 50) && terrainCount >= 9)
            {
                yield return null;
            }
        }

        if (terrainCount >= 9)
        {
            yield return null;
        }
        _terrain.terrainData.SetDetailLayer(0, 0, 0, details);  // Set detail layer
        _terrain.terrainData.RefreshPrototypes();               // Refresh all available prototypes in the terrain object
    }

    /// <summary>
    /// Detail prototype function used to set the values for the grass to be created
    /// </summary>
    public static DetailPrototype GrassPrototypeForTexture(Texture2D texture2D, Color dryColour, Color healthyColour)
    {
        DetailPrototype detail = new DetailPrototype();
        detail.usePrototypeMesh = false;
        detail.prototypeTexture = texture2D;
        detail.dryColor = dryColour;
        detail.healthyColor = healthyColour;
        detail.noiseSpread = 0.1f;
        detail.minWidth = 0.5f;
        detail.maxWidth = 1;
        detail.minHeight = 0.1f;
        detail.maxHeight = 0.2f;
        detail.renderMode = DetailRenderMode.GrassBillboard;
        return detail;
    }

    public IEnumerator CreateTrees(Terrain _terrain, int terrainCount)
    {
        // Delete existing trees
        List<TreeInstance> treeInstances = new List<TreeInstance>(0);
        _terrain.terrainData.treeInstances = treeInstances.ToArray();

        GameObject _collider;
        Vector3 colliderPos;

        bool treeMade;
        // Loop through the terrain size on both the x and z axis
        for (float x = 0; x < _terrain.terrainData.size.x; x++)
        {
            for (float z = 0; z < _terrain.terrainData.size.z; z++)
            {
                terrainHeight = Mathf.PerlinNoise(x * 0.05f, z * 0.05f);
                int randomGrouping = Mathf.FloorToInt(Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 1000);
                treeDensity = Random.Range(0, 100);

                // Set terrain height
                terrainHeight = _terrain.terrainData.GetHeight((int)(x * _terrain.terrainData.heightmapWidth / _terrain.terrainData.size.x),
                                                               (int)(z * _terrain.terrainData.heightmapHeight / _terrain.terrainData.size.z));

                // If terrain is greater than 5 or less than 20 then place tree between them heights
                if (randomGrouping < 200 && terrainHeight >= 6 && terrainHeight <= 19)
                {
                    treeMade = false;

                    // Set terrain height and tree density, if a tree is between them heights and has that density create 1 of 3 tree types based on terrain height and tree density
                    if (terrainHeight <= 9 && treeDensity <= 60)
                    {
                        treeInstance = new TreeInstance();  // Create trees
                        treeInstance.prototypeIndex = 0;    // Create palm trees
                        // Set tree scale for width and height
                        treeInstance.widthScale = 0.7f;
                        treeInstance.heightScale = 0.7f;
                        treeMade = true;
                    }
                    else if (terrainHeight <= 14 && treeDensity <= 30)
                    {
                        treeInstance = new TreeInstance();  // Create trees
                        treeInstance.prototypeIndex = 1;    // Create broad leaf trees trees
                        // Set tree scale for width and height
                        treeInstance.widthScale = 0.3f;
                        treeInstance.heightScale = 0.3f;
                        treeMade = true;
                    }
                    else if (terrainHeight >= 13 && treeDensity >= 30 && treeDensity <= 60)
                    {
                        treeInstance = new TreeInstance();  // Create trees
                        treeInstance.prototypeIndex = 2;    // Create conifer trees
                        // Set tree scale for width and height
                        treeInstance.widthScale = 0.3f;
                        treeInstance.heightScale = 0.3f;
                        treeMade = true;
                    }

                    // If tree made euqals true, set tree positions, scale and colliders
                    if (treeMade == true)
                    {
                        // Create random position for trees to be generated
                        treeRandomPosition = Random.value;

                        // Set tree position to be always be on terrain
                        treeInstance.position = new Vector3((x + treeRandomPosition) / _terrain.terrainData.size.x, 0, (z + treeRandomPosition) / _terrain.terrainData.size.z);

                        // Add tree instance to terrain
                        _terrain.AddTreeInstance(treeInstance);
                        // Apply changes done in terrain so it takes effect
                        _terrain.Flush();

                        // Set collider position to be equal to terrain height at which the trees are generated
                        colliderPos = new Vector3(x + treeRandomPosition + _terrain.transform.position.x, _terrain.SampleHeight(new Vector3(x + _terrain.transform.position.x, 0, z
                            + _terrain.transform.position.z)), z + treeRandomPosition + _terrain.transform.position.z);
                        _collider = new GameObject();
                        _collider.gameObject.AddComponent<CapsuleCollider>();
                        _collider.transform.position = colliderPos;                 // Set the collider position to be equal to the collider position placing the capsule collider at the trees position
                        _collider.GetComponent<CapsuleCollider>().height = 5;       // Set caspule collider height applied to trees equal to 5
                        _collider.GetComponent<CapsuleCollider>().radius = 0.2f;    // Set capsule collider radius applied to trees equal to 0.2
                    }
                }
            }
            // yield used every 50 x's to maintain performance of player
            if (x / 50 == (int)x / 50 && terrainCount >= 9)
            {
                yield return null;
            }
        }
    }
}
