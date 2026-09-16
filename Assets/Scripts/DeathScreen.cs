using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 사망 화면. 다시 시도 / 포기하기.
/// 포기하기는 **확인 없이** 즉시 기록 후 종료 (ESC 메뉴와 다르게 처리 — 이미 실패한 상태라
/// 붙잡으면 데이터가 왜곡됨). 누적 사망 10회 초과 시 안내 한 줄 추가.
/// </summary>
public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }
    public static bool IsShowing { get; private set; }

    GameObject root;
    Text suggestLine;

    void Awake()
    {
        Instance = this;
        IsShowing = false;   // 씬을 다시 불러왔을 때(웹 데모 재시작) 이전 상태가 남지 않게
        Build();
    }

    void Build()
    {
        var overlay = GameAssets.NewPanel("DeathOverlay", GameUI.Instance.CanvasRoot, new Color(0f, 0f, 0f, 0.8f));
        var rt = overlay.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        root = overlay.gameObject;

        var title = GameAssets.NewText("Title", rt, "죽었습니다", 48, new Color(0.95f, 0.3f, 0.3f));
        GameAssets.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(400f, 70f));

        var retry = GameAssets.NewButton("RetryBtn", rt, "다시 시도", 24, OnRetry, new Vector2(240f, 54f));
        GameAssets.Place((RectTransform)retry.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(240f, 54f));

        var giveUp = GameAssets.NewButton("GiveUpBtn", rt, "포기하기", 24, OnGiveUp, new Vector2(240f, 54f));
        GameAssets.Place((RectTransform)giveUp.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(240f, 54f));

        suggestLine = GameAssets.NewText("Suggest", rt, "계속 어려우면 포기하셔도 됩니다", 18, new Color(0.7f, 0.7f, 0.7f));
        GameAssets.Place(suggestLine.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(500f, 30f));

        root.SetActive(false);
    }

    public static void Show()
    {
        if (Instance == null) return;
        IsShowing = true;
        Time.timeScale = 0f;
        Instance.suggestLine.gameObject.SetActive(
            GameLogger.Instance != null && GameLogger.Instance.ShouldSuggestQuit);
        Instance.root.SetActive(true);
    }

    void OnRetry()
    {
        IsShowing = false;
        root.SetActive(false);
        if (PlayerController.Instance != null) PlayerController.Instance.Respawn();
        Time.timeScale = 1f;
    }

    void OnGiveUp()
    {
        // 확인 없음 — 즉시 기록 후 종료
        if (GameLogger.Instance != null) GameLogger.Instance.FinishGaveUp();
        GameUI.QuitApp();
    }
}
