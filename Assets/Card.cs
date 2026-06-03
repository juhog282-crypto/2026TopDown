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

    private bool isFlipped = false;
    private bool isMatched = false;

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
        // 마우스 왼쪽 버튼 클릭 순간을 직접 감지
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isFlipped || isMatched) return;

            // 스크린 마우스 좌표 -> 게임 월드 좌표 변환
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            // 마우스 클릭 위치가 내 카드 콜라이더 영역 안인지 검사
            if (cardCollider != null && cardCollider.OverlapPoint(clickPos))
            {
                TriggerCardClick();
            }
        }
    }

    private void TriggerCardClick()
    {
        // ⭐ 에러 원인 해결: CardGame 대신 CardGameManger를 찾도록 이름을 맞췄습니다!
        CardGameManger manager = Object.FindFirstObjectByType<CardGameManger>();
        if (manager != null)
        {
            manager.CardSelected(this);
        }
    }

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