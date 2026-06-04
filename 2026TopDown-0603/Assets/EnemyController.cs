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
        }
    }

    private void Start()
    {
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        
        if (Time.timeScale == 0f) return;

      
        if (targetPlayer == null) return;

     
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
          
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);
        }
    }
}