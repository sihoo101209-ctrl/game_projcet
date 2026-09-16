using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 게임 화면 HUD 전체를 코드로 생성한다 (에디터 UI 작업 불필요).
///   좌상단: 방 번호 + HP 바
///   상단 중앙: 보스·준보스 체력 바 (해당 방에서만, 고정)
///   클리어 화면
/// 미니맵은 절대 여기 두지 않는다 — 맵은 ESC 메뉴 안에서만 (실험 핵심 조건).
/// 화면에 그만하기 버튼도 두지 않는다 — 종료는 ESC 메뉴·사망 화면에서만.
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    public RectTransform CanvasRoot { get; private set; }

    Text roomLabel;
    RectTransform hpFill;
    Text hpText;
    const float HpBarWidth = 216f;

    GameObject bossRoot;
    RectTransform bossFill;
    Text bossLabel;
    const float BossBarWidth = 496f;

    GameObject clearRoot;

    void Awake()
    {
        Instance = this;
        BuildCanvas();
        gameObject.AddComponent<PauseMenu>();
        gameObject.AddComponent<DeathScreen>();
    }

    void BuildCanvas()
    {
        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        CanvasRoot = (RectTransform)canvasGO.transform;

        // 좌상단: 방 번호
        roomLabel = GameAssets.NewText("RoomLabel", CanvasRoot, "방 0", 28, Color.white, TextAnchor.UpperLeft);
        GameAssets.Place(roomLabel.rectTransform, new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(200f, 34f));

        // 좌상단: HP 바
        var hpBg = GameAssets.NewPanel("HPBarBG", CanvasRoot, new Color(0.08f, 0.08f, 0.1f, 0.9f));
        GameAssets.Place(hpBg.rectTransform, new Vector2(0f, 1f), new Vector2(16f, -52f), new Vector2(HpBarWidth + 4f, 24f));
        var fillImg = GameAssets.NewPanel("HPFill", hpBg.transform, new Color(0.2f, 0.85f, 0.35f));
        hpFill = fillImg.rectTransform;
        hpFill.anchorMin = new Vector2(0f, 0.5f);
        hpFill.anchorMax = new Vector2(0f, 0.5f);
        hpFill.pivot = new Vector2(0f, 0.5f);
        hpFill.anchoredPosition = new Vector2(2f, 0f);
        hpFill.sizeDelta = new Vector2(HpBarWidth, 20f);
        hpText = GameAssets.NewText("HPText", hpBg.transform, "120 / 120", 15, Color.white);
        GameAssets.Place(hpText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(HpBarWidth, 20f));

        // 상단 중앙: 보스 체력 바 (기본 숨김)
        var bossBg = GameAssets.NewPanel("BossBarBG", CanvasRoot, new Color(0.08f, 0.08f, 0.1f, 0.9f));
        GameAssets.Place(bossBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(BossBarWidth + 4f, 26f));
        bossBg.rectTransform.pivot = new Vector2(0.5f, 1f);
        var bossFillImg = GameAssets.NewPanel("BossFill", bossBg.transform, new Color(0.8f, 0.2f, 0.6f));
        bossFill = bossFillImg.rectTransform;
        bossFill.anchorMin = new Vector2(0f, 0.5f);
        bossFill.anchorMax = new Vector2(0f, 0.5f);
        bossFill.pivot = new Vector2(0f, 0.5f);
        bossFill.anchoredPosition = new Vector2(2f, 0f);
        bossFill.sizeDelta = new Vector2(BossBarWidth, 22f);
        bossLabel = GameAssets.NewText("BossLabel", bossBg.transform, "보스", 16, Color.white);
        GameAssets.Place(bossLabel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(BossBarWidth, 22f));
        bossRoot = bossBg.gameObject;
        bossRoot.SetActive(false);
    }

    // ────────── HUD ──────────

    public void SetRoom(int id) => roomLabel.text = "방 " + id;

    public void SetHP(int hp, int max)
    {
        hpFill.sizeDelta = new Vector2(HpBarWidth * Mathf.Clamp01((float)hp / max), 20f);
        hpText.text = $"{hp} / {max}";
    }

    // ────────── 보스 바 (상단 고정 — 보스전 20초+ 진행도 표시) ──────────

    public void ShowBossBar(bool isFinal, int maxHp)
    {
        bossRoot.SetActive(true);
        bossLabel.text = isFinal ? "보스" : "준보스";
        bossFill.sizeDelta = new Vector2(BossBarWidth, 22f);
    }

    public void UpdateBossBar(int hp, int maxHp) =>
        bossFill.sizeDelta = new Vector2(BossBarWidth * Mathf.Clamp01((float)hp / maxHp), 22f);

    public void HideBossBar() => bossRoot.SetActive(false);

    // ────────── 클리어 화면 ──────────

    public void ShowCleared()
    {
        if (clearRoot != null) return;
        Time.timeScale = 0f;

        var overlay = GameAssets.NewPanel("ClearOverlay", CanvasRoot, new Color(0f, 0f, 0f, 0.75f));
        var rt = overlay.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        clearRoot = overlay.gameObject;

        var title = GameAssets.NewText("Title", rt, "클리어!", 56, Color.white);
        GameAssets.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(400f, 80f));
        var sub = GameAssets.NewText("Sub", rt, "수고했습니다. 기록이 저장되었습니다.", 22, new Color(0.8f, 0.8f, 0.8f));
        GameAssets.Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(600f, 40f));
        var quit = GameAssets.NewButton("QuitBtn", rt, "종료", 24, QuitApp, new Vector2(220f, 52f));
        GameAssets.Place((RectTransform)quit.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(220f, 52f));
    }

    public bool ClearShown => clearRoot != null;

    public static void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
