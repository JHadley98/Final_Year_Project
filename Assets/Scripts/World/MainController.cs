using UnityEngine;

public class MainController : MonoBehaviour
{
    private Terrain _terrain;

    public GameObject _player;

    private Camera _camera;

    public MapGenerator _mapGenerator;

    public float minCameraHeight;
    public float startCameraHeight;
    private float startBuildTime = 0f;

    private Terrain prevTerrain;

    public bool terrainsNeeded;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _terrain = GetTerrain(_camera.transform.position);
        prevTerrain = _terrain;
        terrainsNeeded = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Get terrains position to equal the camera position
        _terrain = GetTerrain(_camera.transform.position);

        //Check if terrain has changed from previous one
        if (_terrain != prevTerrain)
        {
            terrainsNeeded = true;
        }
        // If terrain is needed build new terrain, 1 at time with a 2 second delay between each generation of terrain
        if (terrainsNeeded == true && Time.time - startBuildTime > 2f)
        {
            startBuildTime = Time.time;
            _mapGenerator.CreateEndlessTerrain(_terrain.GetComponent<Terrain>());
        }

        // Positions in getheight are not reversed as looking at camera but need to be scaled as x and z positions are on a different scale to the height map
        float terrainHeight = _terrain.terrainData.GetHeight((int)(_camera.transform.position.x - _terrain.GetPosition().x) * _terrain.terrainData.heightmapWidth / (int)_terrain.terrainData.size.x,
                                                          (int)(_camera.transform.position.z - _terrain.GetPosition().z) * _terrain.terrainData.heightmapHeight / (int)_terrain.terrainData.size.z);

        // Check camera height
        if (_camera.transform.position.y < terrainHeight + minCameraHeight)
        {
            //Move up
            ChangeCameraHeight();
        }
        else if (_camera.transform.localPosition.y > startCameraHeight)
        {
            // Move down
            ChangeCameraHeight();
        }
    }

    public void ChangeCameraHeight()
    {
        // positions in getheight are not reversed as looking at camera but need to be scaled as x and z positions are on a different scale to the height map
        float terrainHeight = _terrain.terrainData.GetHeight((int)(_camera.transform.position.x - _terrain.GetPosition().x) * _terrain.terrainData.heightmapWidth / (int)_terrain.terrainData.size.x,
                                                             (int)(_camera.transform.position.z - _terrain.GetPosition().z) * _terrain.terrainData.heightmapHeight / (int)_terrain.terrainData.size.z);
        // Move camera's relative position
        float cameraNewY;
        //dont let camera go below water
        if (terrainHeight < 7)
        {
            cameraNewY = 7 + minCameraHeight - _player.transform.position.y;
        }
        else
        {
            cameraNewY = terrainHeight + minCameraHeight - _player.transform.position.y;
        }


        // If camera position is lower than player then keep camera position in line with player
        if (cameraNewY < startCameraHeight)
        {
            cameraNewY = startCameraHeight;
        }

        // Cap the amount of change per frame to 0.01 this stops dramatic changes to the camera angle
        if (cameraNewY > _camera.transform.localPosition.y + 0.01f)
        {
            cameraNewY = _camera.transform.localPosition.y + 0.01f;
        }
        else if (cameraNewY < _camera.transform.localPosition.y - 0.01f)
        {
            cameraNewY = _camera.transform.localPosition.y - 0.01f;
        }

        _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, cameraNewY, _camera.transform.localPosition.z);

        //Move camera's angle
        // Distance on Y axis from camera to player 
        float differenceInY = cameraNewY - startCameraHeight;
        // ~Distance in X, Z plane from camera to player
        float distanceToPlayer = Mathf.Sqrt((_camera.transform.localPosition.x * _camera.transform.localPosition.x) + (_camera.transform.localPosition.z * _camera.transform.localPosition.z));
        // Angle calculated using inverse tan and converting from radians to degrees
        float cameraDownAngle = Mathf.Atan(differenceInY / distanceToPlayer) * (180 / Mathf.PI);
        // Apply angle to camera rotation
        _camera.transform.localEulerAngles = new Vector3(cameraDownAngle, 0, 0);
    }

    //Code sourced from https://stackoverflow.com/questions/52345522/unity-get-the-actual-current-terrain
    // This returns the terrain for any given position, needed for when the player moves from terrain to another
    public Terrain GetTerrain(Vector3 playerPos)
    {
        Terrain[] _terrains = Terrain.activeTerrains;
        //Get the closest one to the player
        Vector3 center = new Vector3(_terrains[0].transform.position.x + _terrains[0].terrainData.size.x / 2, playerPos.y, _terrains[0].transform.position.z + _terrains[0].terrainData.size.z / 2);
        float lowestDistance = (center - playerPos).sqrMagnitude;
        int terrainIndex = 0;

        for (int i = 0; i < _terrains.Length; i++)
        {
            center = new Vector3(_terrains[i].transform.position.x + _terrains[i].terrainData.size.x / 2, playerPos.y, _terrains[i].transform.position.z + _terrains[i].terrainData.size.z / 2);

            //Find the distance and check if it is lower than the last one then store it
            float dist = (center - playerPos).sqrMagnitude;
            if (dist < lowestDistance)
            {
                lowestDistance = dist;
                terrainIndex = i;
            }
        }

        return _terrains[terrainIndex];
    }
}