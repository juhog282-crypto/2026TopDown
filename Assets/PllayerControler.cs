
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Sprite[] spriteUP;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;
    public float frameTime = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        if (currentSprites != null && currentSprites.Length > 0)
        {
            sr.sprite = currentSprites[0];
        }
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                    ChangeSprites(spriteRight);
                else
                    ChangeSprites(spriteLeft);
            }
            else
            {
                if (input.y > 0)
                    ChangeSprites(spriteUP);
                else
                    ChangeSprites(spriteDown);
            }
        }
    }

    private void Update()
    {
        // 움직이지 않을 때는 애니메이션을 멈추고 0번째 정지 프레임으로 둡니다.
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            if (currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[frameIndex];
            }
            return;
        }

        // 움직이고 있을 때만 발걸음을 옮기는 애니메이션 타이머 작동
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;
            if (currentSprites != null && frameIndex >= currentSprites.Length)
                frameIndex = 0;

            if (currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[frameIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        // 이미 같은 방향을 보고 있다면 교체 처리를 하지 않고 나갑니다.
        if (currentSprites == newSprites)
            return;

        if (newSprites != null && newSprites.Length > 0)
        {
            currentSprites = newSprites;
            frameIndex = 0;
            timer = 0f;
            sr.sprite = currentSprites[frameIndex];
        }
    }
}