using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGeneration : MonoBehaviour
{
    public GameObject broadleafTree;
    public GameObject coniferTree;
    public GameObject palmTree;
    private TreeInstance treeInstance;

    public void GetTrees(Terrain _terrain)
    {
        //Delete existing trees
        List<TreeInstance> treeInstances = new List<TreeInstance>(0);
        _terrain.terrainData.treeInstances = treeInstances.ToArray();
        
        GameObject _collider;
        Vector3 colliderPos;
        float terrainHeight;
        float treeDensity;
        float treeRandom;
        


        // Trees created, however can walk through them, colliders don't work on prefab
        for (float x = 0; x < _terrain.terrainData.size.x; x++)
        {
            for (float z = 0; z < _terrain.terrainData.size.z; z++)
            {
                //Terrain terrain = GetComponent<Terrain>();

                terrainHeight = Mathf.PerlinNoise(x * 0.05f, z * 0.05f);
                int r = Mathf.FloorToInt(Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 1000);
                treeDensity = Random.Range(0, 100);
                if (r < 200 && treeDensity < 25)
                {

                    terrainHeight = _terrain.terrainData.GetHeight((int)(x * _terrain.terrainData.heightmapWidth / _terrain.terrainData.size.x),
                                                          (int)(z * _terrain.terrainData.heightmapHeight / _terrain.terrainData.size.z));
                    
                    


                    if (terrainHeight > 5 && terrainHeight < 20)
                    {
                        treeInstance = new TreeInstance();

                        treeRandom = Random.value;
                        treeInstance.position = new Vector3((x + treeRandom) / _terrain.terrainData.size.x, 0, (z + treeRandom) / _terrain.terrainData.size.z);

                        if (terrainHeight < 8)
                        {
                            treeInstance.prototypeIndex = 0;
                        }
                        else if (terrainHeight < 15)
                        {
                            treeInstance.prototypeIndex = 1;
                        }
                        else
                        {
                            treeInstance.prototypeIndex = 2;
                        }
                            
                        treeInstance.widthScale = 0.3f;
                        treeInstance.heightScale = 0.3f;
                        treeInstance.color = Color.white;
                        treeInstance.lightmapColor = Color.white;

                        _terrain.AddTreeInstance(treeInstance);
                        _terrain.Flush();


                        colliderPos = new Vector3((x + treeRandom), _terrain.SampleHeight(new Vector3(x, 0, z)), (z + treeRandom));
                        _collider = new GameObject();
                        _collider.gameObject.AddComponent<CapsuleCollider>();
                        _collider.transform.position = colliderPos;
                        _collider.GetComponent<CapsuleCollider>().height = 40;
                        _collider.GetComponent<CapsuleCollider>().radius = 0.2f;
                    }
                }
            }
        }



    }
}