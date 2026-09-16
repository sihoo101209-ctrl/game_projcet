using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ESC 메뉴: 계속하기 / 맵 보기 / 조작법 / 그만하기(확인 후 종료).
/// 열려 있는 동안 Time.timeScale = 0 — 반드시. 안 그러면 메뉴 시간이 클리어 타임에 섞인다.
/// 맵 화면을 열 때마다 RecordMapView() 호출 (ESC 자체가 아니라 맵 항목을 눌렀을 때만).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    GameObject root, mainPanel, mapPanel, controlsPanel, quitPanel;
    MapScreen mapScreen;

    void Start()
    {
        IsOpen = false;      // 씬 재진입 대비
        Build();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (DeathScreen.IsShowing) return;
        if (GameUI.Instance != null && GameUI.Instance.ClearShown) return;

        if (IsOpen) Close();
        else Open();
    }

    void Open()
    {
        IsOpen = true;
        Time.timeScale = 0f;
        root.SetActive(true);
        ShowOnly(mainPanel);
    }

    void Close()
    {
        IsOpen = false;
        root.SetActive(false);
        if (!DeathScreen.IsShowing) Time.timeScale = 1f;
    }

    void ShowOnly(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        mapPanel.SetActive(panel == mapPanel);
        controlsPanel.SetActive(panel == controlsPanel);
        quitPanel.SetActive(panel == quitPanel);
    }

    void Build()
    {
        var overlay = GameAssets.NewPanel("PauseOverlay", GameUI.Instance.CanvasRoot, new Color(0f, 0f, 0f, 0.75f));
        var rt = overlay.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        root = overlay.gameObject;

        // ── 메인 메뉴 ──
        mainPanel = GameAssets.NewUI("Main", rt).gameObject;
        Stretch(mainPanel);
        string[] labels = { "계속하기", "맵 보기", "조작법", "그만하기" };
        UnityEngine.Events.UnityAction[] actions =
        {
            Close,
            OpenMap,
            () => ShowOnly(controlsPanel),
            () => ShowOnly(quitPanel),
        };
        for (int i = 0; i < labels.Length; i++)
        {
            var b = GameAssets.NewButton("Btn_" + labels[i], mainPanel.transform, labels[i], 24, actions[i], new Vector2(260f, 54f));
            GameAssets.Place((RectTransform)b.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 110f - i * 72f), new Vector2(260f, 54f));
        }

        // ── 맵 화면 (별도 화면 — 게임 화면 미니맵 금지) ──
        mapPanel = GameAssets.NewUI("Map", rt).gameObject;
        Stretch(mapPanel);
        var mapTitle = GameAssets.NewText("MapTitle", mapPanel.transform, "맵", 30, Color.white);
        GameAssets.Place(mapTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(200f, 40f));
        var mapContent = GameAssets.NewUI("MapContent", mapPanel.transform);
        GameAssets.Place(mapContent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 500f));
        mapScreen = mapContent.gameObject.AddComponent<MapScreen>();
        AddBackButton(mapPanel);

        // ── 조작법 ──
        controlsPanel = GameAssets.NewUI("Controls", rt).gameObject;
        Stretch(controlsPanel);
        var ctrl = GameAssets.NewText("CtrlText", controlsPanel.transform,
            "WASD  이동\n마우스 클릭  공격\n\n적을 모두 처치하면 문이 열립니다", 26, Color.white);
        GameAssets.Place(ctrl.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(600f, 200f));
        AddBackButton(controlsPanel);

        // ── 그만하기 확인 ──
        quitPanel = GameAssets.NewUI("Quit", rt).gameObject;
        Stretch(quitPanel);
        var q = GameAssets.NewText("QuitText", quitPanel.transform, "정말 그만할래?", 30, Color.white);
        GameAssets.Place(q.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(400f, 50f));
        var yes = GameAssets.NewButton("YesBtn", quitPanel.transform, "예, 그만하기", 22, OnQuitConfirmed, new Vector2(240f, 50f));
        GameAssets.Place((RectTransform)yes.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(240f, 50f));
        var no = GameAssets.NewButton("NoBtn", quitPanel.transform, "아니오", 22, () => ShowOnly(mainPanel), new Vector2(240f, 50f));
        GameAssets.Place((RectTransform)no.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -64f), new Vector2(240f, 50f));

        root.SetActive(false);
    }

    void OpenMap()
    {
        // 맵 항목을 눌렀을 때만 카운트 — 길 잃음을 행동으로 측정하는 핵심 지표
        if (GameLogger.Instance != null) GameLogger.Instance.RecordMapView();
        mapScreen.Rebuild();
        ShowOnly(mapPanel);
    }

    void OnQuitConfirmed()
    {
        if (GameLogger.Instance != null) GameLogger.Instance.FinishGaveUp();
        GameUI.QuitApp();
    }

    void AddBackButton(GameObject panel)
    {
        var b = GameAssets.NewButton("BackBtn", panel.transform, "뒤로", 20, () => ShowOnly(mainPanel), new Vector2(160f, 44f));
        GameAssets.Place((RectTransform)b.transform, new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(160f, 44f));
    }

    static void Stretch(GameObject go)
    {
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
