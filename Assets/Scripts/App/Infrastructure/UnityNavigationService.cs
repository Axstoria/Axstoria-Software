using App.Domain;
using UnityEngine.SceneManagement;

namespace App.Infrastructure
{
    public class UnityNavigationService : INavigationService
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}