using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using TMPro; 

public class GameOverUIManager : MonoBehaviour
{
    public GameObject gameOverPanel; 
    
    
    public TextMeshProUGUI bestRecordText; 

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        gameOverPanel.SetActive(true); 

        int best = PlayerPrefs.GetInt("BestRound", 0);
        bestRecordText.text = "최고 기록: " + best + " 라운드";
    }

  
    public void OnClickRestart()
    {
        Time.timeScale = 1f; 

        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}