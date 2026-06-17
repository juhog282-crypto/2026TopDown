using UnityEngine;

public class HelpWindowManager : MonoBehaviour
{
    public GameObject helpPanel; // 설명창 패널을 연결할 변수

    // 설명창 열기/닫기 토글 함수
    public void ToggleHelpWindow()
    {
        // 현재 상태를 반전시킵니다 (꺼져있으면 켜고, 켜져있으면 끕니다)
        bool isActive = helpPanel.activeSelf;
        helpPanel.SetActive(!isActive);
    }
}