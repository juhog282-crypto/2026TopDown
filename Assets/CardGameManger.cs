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

    [Header("배치 설정")]
    public int rows = 4;             
    public int cols = 6;             
    public float spacing = 1.5f;     

    [Header("게임 규칙 설정")]
    private int wrongCount = 0;             
    public int maxWrongCount = 3;          
    public float penaltySurvivalTime = 30f; // 벌칙 버티기 기본 시간 (30초)

    [Header(" 규칙 설정 (시간)")]
    public float previewDuration = 3f;     
    public float cardMatchLimitTime = 20f; 

    [Header("화면 글자 UI 연결")]
    public TextMeshProUGUI wrongCountText; 
    public TextMeshProUGUI timerText;      

    [Header("라운드 시스템 및 기록")]
    private int currentRound = 1; 
    private int matchedPairsCount = 0; 

    private List<GameObject> spawnedCards = new List<GameObject>();
    private Card firstSelectedCard = null;
    private Card secondSelectedCard = null;
    private bool isProcessing = false;
    private bool isGamePlaying = false;    
    
    // 현재 벌칙 상태(도망치기)인지 체크하는 변수
    [HideInInspector] public bool isPenaltyMode = false;

    private void Start()
    {
        currentRound = 1;
        StartNewGame();
    }

    public void StartNewGame()
    {
        // 🔒 카드 맞추기 시작 시 플레이어, 몬스터 완전 동결
        Time.timeScale = 0f; 
        isPenaltyMode = false;

        wrongCount = 0;
        matchedPairsCount = 0; 
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
            cardIDs.Add(i); cardIDs.Add(i);
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
        isProcessing = true;
        foreach (GameObject cardObj in spawnedCards)
        {
            if (cardObj != null) cardObj.GetComponent<Card>().ShowFront();
        }

        float pTimeLeft = previewDuration;
        while (pTimeLeft > 0)
        {
            if (timerText != null)
                timerText.text = $"카드를 기억하세요!\n시작까지 {Mathf.CeilToInt(pTimeLeft)}초";
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
                timerText.text = $"라운드 {currentRound}\n남은 시간: {Mathf.CeilToInt(mTimeLeft)}초";
            mTimeLeft -= Time.unscaledDeltaTime; 
            yield return null;
        }

        // ⏱️ 카드 맞추기 시간 초과 시 ➔ 패널티 선택창 오픈!
        if (mTimeLeft <= 0 && isGamePlaying)
        {
            OpenPenaltyChoiceScreen();
        }
    }

    public void CardSelected(Card clickedCard)
    {
        if (isProcessing || !isGamePlaying || isPenaltyMode) return;

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
            
            matchedPairsCount++;
            int totalPairsNeeded = (rows * cols) / 2;

            if (matchedPairsCount >= totalPairsNeeded)
            {
                RoundClearNextStage();
                yield break;
            }
        }
        else
        {
            firstSelectedCard.FlipCard();
            secondSelectedCard.FlipCard();

            wrongCount++;
            UpdateWrongCountUI(); 

            // ❌ 3번 틀려서 카드 맞추기 실패 시 ➔ 패널티 선택창 오픈!
            if (wrongCount >= maxWrongCount)
            {
                OpenPenaltyChoiceScreen();
                yield break;
            }
        }

        firstSelectedCard = null;
        secondSelectedCard = null;
        isProcessing = false;
    }

    public void UpdateWrongCountUI()
    {
        if (wrongCountText != null)
            wrongCountText.text = $"틀린 횟수: {wrongCount} / {maxWrongCount}";
    }

    private void RoundClearNextStage()
    {
        isGamePlaying = false;
        currentRound++; 

        int best = PlayerPrefs.GetInt("BestRound", 1);
        if (currentRound > best)
        {
            PlayerPrefs.SetInt("BestRound", currentRound);
            PlayerPrefs.Save();
        }
        StartNewGame(); 
    }

    // 💡 1. 카드 실패 시 [앞면 패널티 선택 패널]을 띄워주는 함수 (이름 중복 해결!)
    private void OpenPenaltyChoiceScreen()
    {
        isGamePlaying = false;
        ClearAllCards(); // 기존 카드판은 깔끔하게 지우기

        PenaltyChoiceManager penaltyChoice = FindObjectOfType<PenaltyChoiceManager>();
        if (penaltyChoice != null)
        {
            penaltyChoice.ShowPenaltyScreen(); // 앞면 패널티 카드 고르는 창 활성화!
        }
        else
        {
            // 혹시 패널티 매니저를 못 찾을 경우를 대비한 방어 코드 (바로 벌칙 시작)
            StartPenaltyMode();
        }
    }

    // 💡 2. 패널티 카드를 고른 직후, 진짜로 30초 도망치기를 시작하는 함수 (PenaltyChoiceManager가 호출함)
    public void StartPenaltyMode()
    {
        isGamePlaying = false;
        isPenaltyMode = true;
        ClearAllCards();

        // 🔓 플레이어와 몬스터의 봉인을 해제합니다! (시간 정상 작동)
        Time.timeScale = 1f; 

        StartCoroutine(PenaltySurvivalCoroutine());
    }

    private IEnumerator PenaltySurvivalCoroutine()
    {
        float timeLeft = penaltySurvivalTime; 

        while (timeLeft > 0 && isPenaltyMode)
        {
            if (timerText != null)
            {
                timerText.text = $"⚠️ 벌칙! 몬스터를 피하세요!\n남은 생존 시간: {Mathf.CeilToInt(timeLeft)}초";
            }
            timeLeft -= Time.deltaTime; 
            yield return null;
        }

        // 🏃‍♂️ 30초 동안 살아서 버티기 성공 시!
        if (isPenaltyMode)
        {
            Debug.Log("벌칙 완료! 다음 라운드로 진행합니다.");
            RoundClearNextStage(); 
        }
    }

    // 💀 30초 버티기 도중 몬스터와 접촉했을 때 호출될 진짜 게임 오버 함수
    public void TriggerGameOver()
    {
        isGamePlaying = false;
        isPenaltyMode = false;
        ClearAllCards();

        Time.timeScale = 0f; // 세상 정지

        GameOverUIManager uiManager = FindObjectOfType<GameOverUIManager>();
        if (uiManager != null)
        {
            uiManager.ShowGameOverUI();
        }
    }

    public void ClearAllCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null) Destroy(card);
        }
        spawnedCards.Clear();
    }
}