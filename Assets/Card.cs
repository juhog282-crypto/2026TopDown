using UnityEngine;
using UnityEngine.InputSystem;

public class Card : MonoBehaviour
{
    [Header("카드 정보")]
    public int cardID;

    [Header("스프라이트 설정")]
    public Sprite frontSprite;
    public Sprite backSprite;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D cardCollider;

    // 외부에서 매니저가 유연하게 통제할 수 있도록 변수를 public/내부 상태로 안전화
    public bool isFlipped = false;
    public bool isMatched = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardCollider = GetComponent<BoxCollider2D>();
    }

    public void SetupCard(int id, Sprite front)
    {
        cardID = id;
        frontSprite = front;
        isFlipped = false;
        isMatched = false;

        ShowBack();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isFlipped || isMatched) return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            if (cardCollider != null && cardCollider.OverlapPoint(clickPos))
            {
                TriggerCardClick();
            }
        }
    }

    private void TriggerCardClick()
    {
        CardGameManger manager = Object.FindFirstObjectByType<CardGameManger>();
        if (manager != null)
        {
            // 매니저에게 클릭 알림을 보냅니다. 뒤집는 연출은 매니저 판정 안에서 대행합니다.
            manager.CardSelected(this);
        }
    }

    // 매니저에서 짝 검사 결과에 맞춰 앞/뒷면 유연하게 반전할 수 있도록 수정한 핵심 로직
    public void FlipCard()
    {
        isFlipped = !isFlipped;

        if (isFlipped)
            ShowFront();
        else
            ShowBack();
    }

    public void ShowFront()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = frontSprite;
    }

    public void ShowBack()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = backSprite;
    }

    public void SetMatched()
    {
        isMatched = true;
    }
}