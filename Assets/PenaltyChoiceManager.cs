using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class PenaltyChoiceManager : MonoBehaviour
{
    public GameObject penaltyPanel; 

    [Header("3개의 패널티 카드 이미지")]
    public Image card1Image;
    public Image card2Image;
    public Image card3Image;

    [Header("각 카드별 패널티 설명 글자")]
    public TextMeshProUGUI card1Text;
    public TextMeshProUGUI card2Text;
    public TextMeshProUGUI card3Text;

    [Header("상단 안내 타이틀")]
    public TextMeshProUGUI titleText; 

    [Header("⭐ 적(Enemy) 원본 프리팹 혹은 오브젝트 연결")]
    public GameObject enemyPrefab; 

    private CardGameManger cardManager;

    void Start()
    {
        // 씬 안에 있는 카드 게임 매니저를 자동으로 찾습니다.
        cardManager = FindObjectOfType<CardGameManger>();
        
        // 시작할 때는 패널티 판넬을 숨겨둡니다.
        if (penaltyPanel != null) penaltyPanel.SetActive(false);
        
        // 카드 이미지들에 마우스 클릭 이벤트 자동 부여
        AddClickEvents();
    }

   
    public void ShowPenaltyScreen()
    {
        if (penaltyPanel != null) penaltyPanel.SetActive(true);
        if (titleText != null) titleText.text = "카드 맞추기 실패!\n감당할 수 있는 패널티 카드를 선택하세요!";
        
        // 1번 카드: 리턴(보상) 요소 추가 완료! ⭐
        if (card1Text != null) card1Text.text = "1번 카드\n\n위기 탈출\n[ 적 1마리 즉시 제거 ]\n대신 버티기 시간 +10초";
        
        if (card2Text != null) card2Text.text = "2번 카드\n\n추가 적\n[ 몬스터 1마리 소환 ]";
        if (card3Text != null) card3Text.text = "3번 카드\n\n다음 매칭 시간 UP\n[ 대신 적 2배 복제 ]";
    }

    // 이미지에 마우스 클릭 감지 컴포넌트를 붙여주는 세팅
    void AddClickEvents()
    {
        if(card1Image != null) AddEventToImage(card1Image, SelectCard1);
        if(card2Image != null) AddEventToImage(card2Image, SelectCard2);
        if(card3Image != null) AddEventToImage(card3Image, SelectCard3);
    }

    void AddEventToImage(Image img, UnityEngine.Events.UnityAction action)
    {
        img.raycastTarget = true; 
        EventTrigger trigger = img.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = img.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { action.Invoke(); });
        trigger.triggers.Add(entry);
    }

    
    void SelectCard1()
    {
        if (cardManager != null)
        {
            // 버티기 시간 40초로 세팅 (기본 30초 + 10초)
            cardManager.penaltySurvivalTime = 40f; 

            // 필드의 모든 적들을 찾아옴
            GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");

            // 적이 1마리 이상 살아있다면 첫 번째 적을 제거/숨김
            if (currentEnemies.Length > 0)
            {
                // 소환된 복사본(Clone)인 경우 완전 파괴
                if (currentEnemies[0].name.Contains("(Clone)"))
                {
                    Destroy(currentEnemies[0]);
                    Debug.Log("1번 카드 발동: 복사된 적 1마리를 제거했습니다!");
                }
                else
                {
                    // 원본 오브젝트인 경우 에러 방지를 위해 화면에서 숨김 처리
                    currentEnemies[0].SetActive(false);
                    Debug.Log("1번 카드 발동: 원본 적 오브젝트를 숨겼습니다!");
                }
            }

            // 패널티 버티기 모드 시작
            cardManager.StartPenaltyMode(); 
        }
        ClosePanel();
    }

    
    void SelectCard2()
    {
        if (cardManager != null)
        {
            cardManager.penaltySurvivalTime = 30f; 
            SpawnExtraEnemy(1); // 1마리 무작위 소환
            cardManager.StartPenaltyMode();
        }
        ClosePanel();
    }

    
    void SelectCard3()
    {
        if (cardManager != null)
        {
            cardManager.penaltySurvivalTime = 30f;
            cardManager.previewDuration += 3f; // 보상: 다음 라운드 3초 더 미리보기

            // 현재 맵에 살아있는 적의 마리수를 체크
            GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            int enemyCount = currentEnemies.Length;
            if (enemyCount == 0) enemyCount = 1; // 적이 하나도 없다면 최소 1마리 기준

            SpawnExtraEnemy(enemyCount); // 존재하는 수만큼 그대로 추가 소환 (2배 복제)
            cardManager.StartPenaltyMode();
        }
        ClosePanel();
    }

   
    void SpawnExtraEnemy(int count)
    {
        if (enemyPrefab != null)
        {
            for (int i = 0; i < count; i++)
            {
                // 화면 안 무작위 위치 계산
                Vector3 randomPos = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0f);
                GameObject newEnemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
                newEnemy.SetActive(true); 

                // 컴포넌트 안전장치 복구 (물리 및 스크립트 재활성화)
                Collider2D collider = newEnemy.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true; 

                Rigidbody2D rb = newEnemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = true;        
                    rb.linearVelocity = Vector2.zero; 
                    rb.gravityScale = 0f;       
                }

                MonoBehaviour[] scripts = newEnemy.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in scripts)
                {
                    if (script != null && script != this) script.enabled = true; 
                }
            }
        }
    }

    // 패널티 창 닫기
    void ClosePanel()
    {
        if (penaltyPanel != null) penaltyPanel.SetActive(false);
    }
}