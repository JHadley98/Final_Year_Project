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
    private int detailResolution;
    private int resolutionPerPatch = 8;
    
    public GameObject water;
    GameObject grass;
    private Texture2D grassSprite;
    public GameObject terrainPrefab;

    public void CreateWater(Terrain _terrain)
    {
        // Instantiate the water object, setting it's location to equal the terrains x, and z size, while settings it Y to equal 6 for the height of the water object to appear in the game
        Instantiate(water, new Vector3(_terrain.terrainData.size.x / 2 +_terrain.transform.position.x, 6, _terrain.terrainData.size.z / 2 +_terrain.transform.position.z), Quaternion.identity);
    }

    public void CreateGrass(Terrain _terrain)
    {
        
        //GameObject grass = (GameObject)Instantiate(terrainPrefab);
        //_terrain = grass.GetComponent<Terrain>();

        // Dry colour = R: 205, G: 188, B: 16, A: 255, convert to decimal divide by 255 as ranges needs to be 0 - 1 instead of range 0 - 255
        // Dry colour in decimals then equals new colour (R: 0.8f, G: 0.73f, B: 0.06, A: 1)
        // Healthy colour = R: 67, G: 249, B: 42, A: 255, convert to decimal divide by 255 as ranges needs to be 0 - 1 instead of range 0 - 255
        // Healthy colour in decimal then equals new colour (R: 0.26, B:
        DetailPrototype detailPrototype = GrassPrototypeForTexture(grassSprite, 2f, new Color(0.8f, 0.73f, 0.06f, 1), new Color(0.67f, 2.49f, 0.42f, 1f));

        

        //TerrainData terrainData = new UnityEngine.TerrainData();
        //_terrain.terrainData = terrainData;

        //_terrain.terrainData.detailPrototypes = new DetailPrototype[1] { detailPrototype };
        _terrain.terrainData.wavingGrassSpeed = 0.5f;
        _terrain.terrainData.wavingGrassAmount = 0.5f;
        _terrain.terrainData.wavingGrassStrength = 0.5f;
        _terrain.terrainData.wavingGrassTint = new Color(.45f, .45f, .45f, 1);

        _terrain.GetComponent<TerrainCollider>().terrainData = _terrain.terrainData;

        _terrain.terrainData.SetDetailResolution(64, 8);
        int size = 64;
        int[,] details = new int[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                details[x, y] = 15;
            }
        }

        _terrain.terrainData.SetDetailLayer(0, 0, 0, details);
    }

    public static DetailPrototype GrassPrototypeForTexture(Texture2D texture2D, float scale, Color dryColour, Color healthyColour)
    {
        DetailPrototype detail = new DetailPrototype();
        detail.usePrototypeMesh = false;
        detail.prototypeTexture = texture2D;
        detail.dryColor = dryColour;
        detail.healthyColor = healthyColour;
        detail.noiseSpread = 5f;
        detail.minWidth = 1 * scale;
        detail.minWidth = 2 * scale;
        detail.minHeight = .3f * scale;
        detail.maxHeight = .5f * scale;
        detail.renderMode = DetailRenderMode.GrassBillboard;
        return detail;
    }

    public void CreateTrees(Terrain _terrain)
    {
        // Delete existing trees
        List<TreeInstance> treeInstances = new List<TreeInstance>(0);
        _terrain.terrainData.treeInstances = treeInstances.ToArray();

        GameObject _collider;
        Vector3 colliderPos;

        // Loop through the terrain size on both the x and z axis
        for (float x = 0; x < _terrain.terrainData.size.x; x++)
        {
            for (float z = 0; z < _terrain.terrainData.size.z; z++)
            {
                terrainHeight = Mathf.PerlinNoise(x * 0.05f, z * 0.05f);
                int randomGrouping = Mathf.FloorToInt(Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 1000);
                treeDensity = UnityEngine.Random.Range(0, 100);

                if (randomGrouping < 200 && treeDensity < 25)
                {
                    // Set terrain height
                    terrainHeight = _terrain.terrainData.GetHeight((int)(x * _terrain.terrainData.heightmapWidth / _terrain.terrainData.size.x),
                                                                   (int)(z * _terrain.terrainData.heightmapHeight / _terrain.terrainData.size.z));
                    
                    // If terrain is greater than 5 or less than 20 then place tree between them heights
                    if (terrainHeight > 5 && terrainHeight < 20)
                    {
                        // Create trees
                        treeInstance = new TreeInstance();

                        // Create random position for trees to be generated
                        treeRandomPosition = UnityEngine.Random.value;
                        
                        // Set tree position to be always be on terrain
                        treeInstance.position = new Vector3((x + treeRandomPosition) / _terrain.terrainData.size.x, 0, (z + treeRandomPosition) / _terrain.terrainData.size.z);

                        // If terrain height is less than 8 and has a tree density less than 50 create tree prototype index 0 (Palm tree)
                        if (terrainHeight < 8 && treeDensity < 50)
                        {
                            treeInstance.prototypeIndex = 0;
                        }
                        // Else if terrain height is less than 15 and has a tree density less than 10 create tree prototype index 1 (Broad tree) between heights 8 and 15
                        else if (terrainHeight < 15 && treeDensity < 10)
                        {
                            treeInstance.prototypeIndex = 1;
                        }
                        // Else generate tree index 2 (Conifer tree) between heights 15 and 20
                        else
                        {
                            treeInstance.prototypeIndex = 2;
                        }

                        // Set tree scale for width and height
                        treeInstance.widthScale = 0.3f;
                        treeInstance.heightScale = 0.3f;

                        // Add tree instance to terrain
                        _terrain.AddTreeInstance(treeInstance);
                        // Apply changes done in terrain so it takes effect
                        _terrain.Flush();

                        // Set collider position to be equal to 
                        colliderPos = new Vector3((x + treeRandomPosition) + _terrain.transform.position.x, _terrain.SampleHeight(new Vector3(x, 0, z)), (z + treeRandomPosition) + _terrain.transform.position.z);
                        _collider = new GameObject();
                        _collider.gameObject.AddComponent<CapsuleCollider>();
                        _collider.transform.position = colliderPos;                 // Set the collider position to be equal to the collider position placing the capsule collider at the trees position
                        _collider.GetComponent<CapsuleCollider>().height = 5;       // Set caspule collider height applied to trees equal to 5
                        _collider.GetComponent<CapsuleCollider>().radius = 0.2f;    // Set capsule collider radius applied to trees equal to 0.2
                    }
                }
            }
        }
    }
}
 