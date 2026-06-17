using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private bool isDead = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 이미 죽었거나 정지 상태면 충돌 무시
        if (isDead || Time.timeScale <= 0f) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("💀 게임 오버!");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        SaveSurvivalRecord();

        // UI를 먼저 보여주고, 그 다음에 게임을 멈추는 것이 안전합니다.
        var uiManager = FindFirstObjectByType<GameOverUIManager>();
        if (uiManager != null) uiManager.ShowGameOverUI();
        
        Time.timeScale = 0f; 
    }

    private void SaveSurvivalRecord()
    {
        // 매니저를 찾아 현재 라운드를 가져옵니다.
        CardGameManger manager = FindFirstObjectByType<CardGameManger>();
        int currentRound = (manager != null) ? GetPrivateCurrentRound(manager) : 1; 

        int bestRound = PlayerPrefs.GetInt("BestRound", 0);
        
        if (currentRound > bestRound)
        {
            PlayerPrefs.SetInt("BestRound", currentRound);
            PlayerPrefs.Save();
        }
    }

    // manager의 currentRound를 가져오는 방법 (manager 스크립트에서 public으로 바꾸는 게 가장 좋습니다)
    private int GetPrivateCurrentRound(CardGameManger manager)
    {
        // 만약 manager 스크립트의 currentRound가 private라면, 
        // CardGameManger 스크립트에서 public int GetCurrentRound() { return currentRound; } 를 추가해 쓰세요.
        return 1; // 임시값. 매니저 스크립트 수정 권장.
    }
}