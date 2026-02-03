using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main");
        }
    }
    public void goToMaps()
    {
        SceneManager.LoadScene("Maps");
    }

    public void tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void map1()
    {
        SceneManager.LoadScene("playground");
    }

    public void map2()
    {
        SceneManager.LoadScene("Docks");
    }

    public void map3()
    {
        SceneManager.LoadScene("Old Town");
    }
}