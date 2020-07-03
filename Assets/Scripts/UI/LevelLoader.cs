using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void Start()
    {
        SceneManager.LoadScene("GameWorld");
    }
}