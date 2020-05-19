using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    private Terrain _terrain;
    public Material _terrainMaterial;

    // Class reference
    public TerrainTexture _texture;

    int chunksVisibleInViewDst;

    // Start is called before the first frame update
    void Start()
    {
        _terrain = GetComponent<Terrain>();

        // At the start of the game apply the material to the terrain material asset and update mesh heights for the terrain.
        // TODO: Create min and max height replacing 0, 100
        _texture.CreateTextureOptions(_terrainMaterial, 0, 100);


        //TODO meed calculation here
        // Set maxViewDst to be detailLevels length - 1
        //float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        // Set chunksize
        //meshWorldSize = _meshSettings.meshWorldSize;
        // Set number of chunk visible in view distance
        //chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
        chunksVisibleInViewDst = 5;

        // Update or create number of chunks visible
        UpdateVisibleChunks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateVisibleChunks()
    {

    }
}
