using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUIManager : MonoBehaviour
{
    public void OnClickReturnToMenu()
    {
        Time.timeScale = 1f; // ∏ÿ√Ë¥¯ Ω√∞£ ¥ŸΩ√ »Â∏£∞‘ «‘
        SceneManager.LoadScene("MainMenu"); 
    }
}