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

    [Header(" 규칙 설정 (시간)")]
    public float previewDuration = 3f;     
    public float cardMatchLimitTime = 20f; 

    [Header("화면 글자 UI 연결")]
    public TextMesh wrongCountText; 
    public TextMesh timerText;      

    private List<GameObject> spawnedCards = new List<GameObject>();
    private Card firstSelectedCard = null;
    private Card secondSelectedCard = null;
    private bool isProcessing = false;
    private bool isGamePlaying = false;    

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        
        Time.timeScale = 0f;

        wrongCount = 0;
        isProcessing = true; 
        isGamePlaying = false;
        firstSelectedCard = null;
        secondSelectedCard = null;

        UpdateWrongCountUI();
        if (timerText != null) timerText.gameObject.SetActive(true); 

        ClearAllCards();
        SpawnCardGrid();

        
        StartCoroutine(PreviewAndStartTimerCoroutine());
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

    
    private IEnumerator PreviewAndStartTimerCoroutine()
    {
        foreach (GameObject cardObj in spawnedCards)
        {
            if (cardObj != null) cardObj.GetComponent<Card>().ShowFront();
        }

       
        float pTimeLeft = previewDuration;
        while (pTimeLeft > 0)
        {
            if (timerText != null)
            {
                timerText.text = $"카드를 기억하세요!\n시작까지 {Mathf.CeilToInt(pTimeLeft)}초";
            }
            pTimeLeft -= Time.unscaledDeltaTime;
            yield return null;
        }

        
        foreach (GameObject cardObj in spawnedCards)
        {
            if (cardObj != null) cardObj.GetComponent<Card>().ShowBack();
        }
        
        isProcessing = false;
        isGamePlaying = true;

        
        float mTimeLeft = cardMatchLimitTime;
        while (mTimeLeft > 0 && isGamePlaying)
        {
            if (timerText != null)
            {
                timerText.text = $"카드 맞추기 시간!\n남은 시간: {Mathf.CeilToInt(mTimeLeft)}초";
            }
            mTimeLeft -= Time.unscaledDeltaTime;
            yield return null;
        }

       
        if (mTimeLeft <= 0 && isGamePlaying)
        {
            Debug.Log("시간 초과! 벌칙 단계로 진입합니다.");
            StartCoroutine(GameOverAndRestartCoroutine());
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

        yield return new WaitForSecondsRealtime(0.5f);

        if (firstSelectedCard.cardID == secondSelectedCard.cardID)
        {
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();
            
            //성공 로직
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
        isGamePlaying = false; 
        ClearAllCards();

        if (timerText != null) timerText.gameObject.SetActive(true);

        Time.timeScale = 1f;

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