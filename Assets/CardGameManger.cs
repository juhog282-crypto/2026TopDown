using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CardGameManger : MonoBehaviour
{
    [Header("카드 설정")]
    public GameObject cardPrefab;    
    public Transform cardParent;     
    public List<Sprite> cardSprites; 
    public Sprite cardBackSprite;    

    [Header("배치 및 규칙")]
    public int rows = 4;             
    public int cols = 6;             
    public float spacing = 1.5f;     
    public int maxWrongCount = 3;            
    public float penaltySurvivalTime = 30f; 

    [Header("시간 및 UI")]
    public float previewDuration = 3f;     
    public float cardMatchLimitTime = 30f; 
    public TextMeshProUGUI wrongCountText; 
    public TextMeshProUGUI timerText;      
    public GameObject victoryCanvas; 

    private int currentRound = 1; 
    private int matchedPairsCount = 0; 
    private int wrongCount = 0;
    
    private List<GameObject> spawnedCards = new List<GameObject>();
    private Card firstSelectedCard = null;
    private Card secondSelectedCard = null;
    
    // 💡 몬스터 정지 제어 변수 (true일 때 몬스터 물리 엔진 정지)
    public bool isProcessing = false; 
    private bool isGamePlaying = false;    
    [HideInInspector] public bool isPenaltyMode = false;

    private void Start()
    {
        currentRound = 1;
        StartNewGame();
    }

    public void StartNewGame()
    {
        isProcessing = true; // 시작 시 정지 상태
        isPenaltyMode = false;
        wrongCount = 0;
        matchedPairsCount = 0; 
        isGamePlaying = false;
        firstSelectedCard = null;
        secondSelectedCard = null;

        UpdateWrongCountUI();
        ClearAllCards();
        SpawnCardGrid();

        StartCoroutine(PreviewAndStartTimerCoroutine());
    }

    public void SpawnCardGrid()
    {
        int totalCards = rows * cols;
        List<int> cardIDs = new List<int>();
        for (int i = 0; i < totalCards / 2; i++) { cardIDs.Add(i); cardIDs.Add(i); }
        
        for (int i = 0; i < cardIDs.Count; i++) { int temp = cardIDs[i]; int randomIndex = Random.Range(i, cardIDs.Count); cardIDs[i] = cardIDs[randomIndex]; cardIDs[randomIndex] = temp; }

        float startX = -(cols - 1) * spacing / 2f;
        float startY = (rows - 1) * spacing / 2f;

        for (int i = 0; i < cardIDs.Count; i++)
        {
            float posX = startX + ((i % cols) * spacing);
            float posY = startY - ((i / cols) * spacing);
            GameObject newCardObj = Instantiate(cardPrefab, new Vector3(posX, posY, 0), Quaternion.identity, cardParent);
            spawnedCards.Add(newCardObj);
            
            Card card = newCardObj.GetComponent<Card>();
            card.backSprite = cardBackSprite;
            card.SetupCard(cardIDs[i], cardSprites[cardIDs[i] % cardSprites.Count]);
            card.ShowFront(); 
        }
    }

    private IEnumerator PreviewAndStartTimerCoroutine()
    {
        // 1. 프리뷰 3초 (정지 유지)
        yield return new WaitForSecondsRealtime(previewDuration);

        foreach (GameObject cardObj in spawnedCards) if (cardObj != null) cardObj.GetComponent<Card>().ShowBack();
        
        // 2. 카드 맞추기 30초 (이 동안 isProcessing = true 유지하여 정지)
        isGamePlaying = true;
        float mTimeLeft = cardMatchLimitTime;
        while (mTimeLeft > 0 && isGamePlaying)
        {
            if (timerText != null) timerText.text = $"라운드 {currentRound}\n남은 시간: {Mathf.CeilToInt(mTimeLeft)}초";
            mTimeLeft -= Time.deltaTime; 
            yield return null;
        }
        
        // 30초 종료 후 패널티 선택창으로 (정지 유지)
        if (mTimeLeft <= 0 && isGamePlaying) OpenPenaltyChoiceScreen();
    }

    public void CardSelected(Card clickedCard)
    {
        if (!isGamePlaying || isPenaltyMode) return;
        if (firstSelectedCard == null) { firstSelectedCard = clickedCard; firstSelectedCard.FlipCard(); }
        else if (secondSelectedCard == null && clickedCard != firstSelectedCard)
        {
            secondSelectedCard = clickedCard;
            secondSelectedCard.FlipCard();
            StartCoroutine(CheckMatchCoroutine());
        }
    }

    private IEnumerator CheckMatchCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (firstSelectedCard.cardID == secondSelectedCard.cardID)
        {
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();
            matchedPairsCount++;
            if (matchedPairsCount >= (rows * cols) / 2) 
            { 
                isProcessing = false; // 승리 시 정지 해제
                if(victoryCanvas) victoryCanvas.SetActive(true); 
            }
        }
        else
        {
            firstSelectedCard.FlipCard();
            secondSelectedCard.FlipCard();
            wrongCount++;
            UpdateWrongCountUI(); 
            if (wrongCount >= maxWrongCount) OpenPenaltyChoiceScreen();
        }
        firstSelectedCard = null;
        secondSelectedCard = null;
    }

    private void OpenPenaltyChoiceScreen()
    {
        isGamePlaying = false;
        // 선택창이 떠 있는 동안 isProcessing = true 유지
        var penaltyChoice = FindFirstObjectByType<PenaltyChoiceManager>();
        if (penaltyChoice != null) penaltyChoice.ShowPenaltyScreen();
        else StartPenaltyMode();
    }

    public void StartPenaltyMode()
    {
        isPenaltyMode = true;
        isProcessing = false; // 💡 여기서부터 몬스터가 움직이기 시작함!
        ClearAllCards();
        StartCoroutine(PenaltySurvivalCoroutine());
    }

    private IEnumerator PenaltySurvivalCoroutine()
    {
        float timeLeft = penaltySurvivalTime; 
        while (timeLeft > 0 && isPenaltyMode)
        {
            if (timerText != null) timerText.text = $"⚠️ 도망치세요!\n생존 시간: {Mathf.CeilToInt(timeLeft)}초";
            timeLeft -= Time.deltaTime; 
            yield return null;
        }
        if (isPenaltyMode) { currentRound++; StartNewGame(); }
    }

    public void UpdateWrongCountUI() { if (wrongCountText != null) wrongCountText.text = $"틀린 횟수: {wrongCount} / {maxWrongCount}"; }
    public void ClearAllCards() { foreach (GameObject card in spawnedCards) if (card != null) Destroy(card); spawnedCards.Clear(); }
}