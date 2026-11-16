using UnityEngine;
using UnityEngine.SceneManagement;

// 점수(생존 시간), 게임 오버, 재시작 관리
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static bool isOver;      // 다른 스크립트가 게임 오버 여부 확인용
    float score;

    void Awake()
    {
        instance = this;
        isOver = false;             // 씬 다시 로드될 때 초기화
    }

    void Update()
    {
        if (!isOver) score += Time.deltaTime;
    }

    public void GameOver()
    {
        isOver = true;
    }

    void OnGUI()
    {
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = 30;
        s.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 20, 400, 50), "Score " + (int)score, s);

        if (isOver)
        {
            // 씬이 Build Settings에 등록돼 있어야 재시작 가능
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60), "Restart"))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
