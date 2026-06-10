using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private bool isDead = false;

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (isDead || Time.timeScale == 0f) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
            FindObjectOfType<GameOverUIManager>().ShowGameOverUI();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("💀 게임 오버!");

       
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        
        SaveSurvivalRecord();

        Time.timeScale = 0f; 
    }

    private void SaveSurvivalRecord()
    {
    

        int bestRound = PlayerPrefs.GetInt("BestRound", 0);
        
       
        int currentRound = 1; 

        Debug.Log($"현재 기록: {currentRound} 라운드 / 기존 최고 기록: {bestRound} 라운드");

       
        if (currentRound > bestRound)
        {
            PlayerPrefs.SetInt("BestRound", currentRound);
            PlayerPrefs.Save(); 
            Debug.Log($"🎉 새 기록 {currentRound} 라운드가 저장되었습니다.");
        }
    }
}