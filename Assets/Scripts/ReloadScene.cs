using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{   

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }
}
