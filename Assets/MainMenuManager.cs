using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수

public class MainMenuManager : MonoBehaviour
{
    // 버튼에 연결할 게임 시작 함수
    public void GameStart()
    {
        Debug.Log("게임 시작!");
        // "GameScene"은 실제 게임이 진행될 씬 이름으로 바꾸세요
        SceneManager.LoadScene("GameScene"); 
    }

    // 버튼에 연결할 게임 종료 함수
    public void GameQuit()
    {
        Debug.Log("게임 종료!");
        Application.Quit(); // 게임 빌드 후 실행 시 종료됨
    }
}