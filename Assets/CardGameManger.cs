using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardGameManger : MonoBehaviour
{
    [Header("카드 설정")]
    public GameObject cardPrefab;
    public Transform cardParent;
    public List<Sprite> cardSprites;
    public Sprite cardBackSprite;

    [Header("배치 설정")]
    public int rows = 4;
    public int cols = 6;
    public float spacing = 1.5f;

    [Header("게임 규칙 설정")]
    private int wrongCount = 0;
    public int maxWrongCount = 3;
    public float restartDelay = 30f;

    [Header("화면 글자 UI 연결")]
    // ⭐ 패키지가 필요 없는 유니티 기본 TextMesh 기능으로 변경했습니다!
    public TextMesh wrongCountText;
    public TextMesh timerText;

    private List<GameObject> spawnedCards = new List<GameObject>();
    private Card firstSelectedCard = null;
    private Card secondSelectedCard = null;
    private bool isProcessing = false;

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        wrongCount = 0;
        isProcessing = false;
        firstSelectedCard = null;
        secondSelectedCard = null;

        UpdateWrongCountUI();
        if (timerText != null) timerText.gameObject.SetActive(false);

        ClearAllCards();
        SpawnCardGrid();
    }

    private void SpawnCardGrid()
    {
        int totalCards = rows * cols;

        List<int> cardIDs = new List<int>();
        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int temp = cardIDs[i];
            int randomIndex = Random.Range(i, cardIDs.Count);
            cardIDs[i] = cardIDs[randomIndex];
            cardIDs[randomIndex] = temp;
        }

        float startX = -(cols - 1) * spacing / 2f;
        float startY = (rows - 1) * spacing / 2f;

        int cardIndex = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float posX = startX + (c * spacing);
                float posY = startY - (r * spacing);

                GameObject newCardObj = Instantiate(cardPrefab, new Vector3(posX, posY, 0), Quaternion.identity, cardParent);
                spawnedCards.Add(newCardObj);

                Card cardScript = newCardObj.GetComponent<Card>();
                if (cardScript != null)
                {
                    int id = cardIDs[cardIndex];
                    Sprite front = cardSprites[id % cardSprites.Count];

                    cardScript.backSprite = cardBackSprite;
                    cardScript.SetupCard(id, front);
                }

                cardIndex++;
            }
        }
    }

    public void CardSelected(Card clickedCard)
    {
        if (isProcessing) return;

        if (firstSelectedCard == null)
        {
            firstSelectedCard = clickedCard;
            firstSelectedCard.FlipCard();
        }
        else if (secondSelectedCard == null && clickedCard != firstSelectedCard)
        {
            secondSelectedCard = clickedCard;
            secondSelectedCard.FlipCard();

            StartCoroutine(CheckMatchCoroutine());
        }
    }

    private IEnumerator CheckMatchCoroutine()
    {
        isProcessing = true;

        yield return new WaitForSeconds(0.5f);

        if (firstSelectedCard.cardID == secondSelectedCard.cardID)
        {
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();
        }
        else
        {
            firstSelectedCard.FlipCard();
            secondSelectedCard.FlipCard();

            wrongCount++;
            UpdateWrongCountUI();

            if (wrongCount >= maxWrongCount)
            {
                StartCoroutine(GameOverAndRestartCoroutine());
                yield break;
            }
        }

        firstSelectedCard = null;
        secondSelectedCard = null;
        isProcessing = false;
    }

    private void UpdateWrongCountUI()
    {
        if (wrongCountText != null)
        {
            wrongCountText.text = $"틀린 횟수: {wrongCount} / {maxWrongCount}";
        }
    }

    private IEnumerator GameOverAndRestartCoroutine()
    {
        isProcessing = true;
        ClearAllCards();

        if (timerText != null) timerText.gameObject.SetActive(true);

        float timeLeft = restartDelay;

        while (timeLeft > 0)
        {
            if (timerText != null)
            {
                timerText.text = $"게임 오버!\n재시작까지 {Mathf.CeilToInt(timeLeft)}초";
            }
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        StartNewGame();
    }

    private void ClearAllCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        spawnedCards.Clear();
    }
}