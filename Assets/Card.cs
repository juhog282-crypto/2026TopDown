using UnityEngine;
using UnityEngine.InputSystem;

public class Card : MonoBehaviour
{
    public int cardID;
    public Sprite frontSprite;
    public Sprite backSprite;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D cardCollider;
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
    
    
}

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isFlipped || isMatched) return;
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            if (cardCollider != null && cardCollider.OverlapPoint(new Vector2(worldPos.x, worldPos.y)))
                TriggerCardClick();
        }
    }

    private void TriggerCardClick()
    {
        CardGameManger manager = FindFirstObjectByType<CardGameManger>();
        if (manager != null) manager.CardSelected(this);
    }

    public void FlipCard()
    {
        isFlipped = !isFlipped;
        if (isFlipped) ShowFront(); else ShowBack();
    }

    public void ShowFront() { if (spriteRenderer != null) spriteRenderer.sprite = frontSprite; }
    public void ShowBack() { if (spriteRenderer != null) spriteRenderer.sprite = backSprite; }
    public void SetMatched() { isMatched = true; }
}