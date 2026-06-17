using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform targetPlayer;
    public float moveSpeed = 3f;
    private Rigidbody2D rb;

    private void Awake() { rb = GetComponent<Rigidbody2D>(); }

    private void FixedUpdate()
    {
        CardGameManger manager = Object.FindFirstObjectByType<CardGameManger>();
        
        // 💡 매니저가 카드 게임 중이면 물리 엔진을 끔 (완벽 정지)
        if (manager != null && manager.isProcessing)
        {
            if (rb != null) 
            {
                rb.simulated = false; 
                rb.linearVelocity = Vector2.zero;
            }
            return; 
        }

        // 그 외에는 정상 작동
        if (rb != null) rb.simulated = true;
        
        if (targetPlayer == null) return;
        Vector2 direction = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
}