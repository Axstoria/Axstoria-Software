using UnityEngine;
using UnityEngine.SceneManagement;

public class FastSwitch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SceneManager.LoadScene("BuilderScene"); 
    }

}
