using UnityEngine;

public class EnemyTraceController : MonoBehaviour
{
    [Header("추적 설정")]
    public Transform targetPlayer;
    public float moveSpeed = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            // 탑다운 2D 물리 이동 시 미끄러짐을 방지하고 벽에 더 깔끔하게 부딪히게 설정
            rb.gravityScale = 0f; 
        }
    }

    private void Start()
    {
        // 자동으로 플레이어를 찾는 안전 장치
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
        }
    }

    
    private void FixedUpdate()
    {
        
        if (Time.timeScale == 0f)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (targetPlayer == null) return;

        // 플레이어 방향 계산
        Vector2 direction = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

        // 물리 컴포넌트가 있다면 완벽하게 벽에 막히는 물리 속도로 이동
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.fixedDeltaTime);
        }
    }
}