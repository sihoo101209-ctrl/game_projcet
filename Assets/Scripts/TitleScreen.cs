using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면을 코드로 생성한다 (에디터 UI 작업 불필요, TMP 불필요).
/// Start 씬에는 GameLogger 와 이 스크립트가 붙은 빈 오브젝트만 있으면 된다.
///
/// 레이아웃은 CLAUDE.md 의 타이틀 화면 사양 그대로:
///   제목 / 기기 번호 / 참가자 번호 / 시작 / 조작 안내(ESC 안내 필수) / 우하단 ver. A|B
/// 기기 번호는 한 번 입력하면 그 실행 내내 유지된다 (PlayerPrefs).
/// 난이도·인트로·스토리 없음.
/// </summary>
public class TitleScreen : MonoBehaviour
{
    public string gameSceneName = "Game";

    const string DeviceKey = "device_id";

    InputField deviceField;
    InputField participantField;
    Text warningText;

    void Awake()
    {
        GameAssets.EnsureCamera();
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        Build();
    }

    void Start()
    {
        int saved = PlayerPrefs.GetInt(DeviceKey, 0);
        if (saved > 0) deviceField.text = saved.ToString();
        participantField.text = "";
        participantField.Select();
        participantField.ActivateInputField();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnStartButton();
        // Tab 으로 두 입력칸 이동
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var next = deviceField.isFocused ? participantField : deviceField;
            next.Select();
            next.ActivateInputField();
        }
    }

    void Build()
    {
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        var root = (RectTransform)canvasGO.transform;

        var bg = GameAssets.NewPanel("BG", root, new Color(0.07f, 0.07f, 0.09f));
        bg.rectTransform.anchorMin = Vector2.zero; bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero; bg.rectTransform.offsetMax = Vector2.zero;
        var center = new Vector2(0.5f, 0.5f);

        var title = GameAssets.NewText("Title", root, "게임 프로젝트", 44, Color.white);
        GameAssets.Place(title.rectTransform, center, new Vector2(0f, 220f), new Vector2(600f, 60f));
        var sub = GameAssets.NewText("Sub", root, "레벨 배치 테스트", 26, new Color(0.75f, 0.75f, 0.8f));
        GameAssets.Place(sub.rectTransform, center, new Vector2(0f, 170f), new Vector2(600f, 40f));

        deviceField = MakeRow(root, "기기 번호", 70f);
        participantField = MakeRow(root, "참가자 번호", 10f);

        var start = GameAssets.NewButton("StartBtn", root, "시작", 28, OnStartButton, new Vector2(240f, 56f));
        GameAssets.Place((RectTransform)start.transform, center, new Vector2(0f, -70f), new Vector2(240f, 56f));

        warningText = GameAssets.NewText("Warning", root, "", 20, new Color(1f, 0.5f, 0.4f));
        GameAssets.Place(warningText.rectTransform, center, new Vector2(0f, -120f), new Vector2(600f, 30f));

        // ESC 안내는 필수 — 화면에 그만하기 버튼이 없으므로 이 줄이 없으면 종료 방법을 알 수 없다
        var guide = GameAssets.NewText("Guide", root,
            "WASD 이동 · 마우스 클릭 공격\n방의 적을 모두 처치하면 문이 열립니다\nESC 를 누르면 메뉴가 열립니다",
            20, new Color(0.7f, 0.7f, 0.75f));
        guide.lineSpacing = 1.3f;
        GameAssets.Place(guide.rectTransform, center, new Vector2(0f, -220f), new Vector2(700f, 100f));

        // 우하단 버전 표시 — 잘못된 버전으로 빌드했을 때 발견할 수 있는 유일한 안전장치
        string ver = GameLogger.Instance != null ? GameLogger.Instance.mapVersion : "?";
        var version = GameAssets.NewText("Version", root, "ver. " + ver, 16, new Color(0.5f, 0.5f, 0.55f), TextAnchor.LowerRight);
        GameAssets.Place(version.rectTransform, new Vector2(1f, 0f), new Vector2(-16f, 12f), new Vector2(200f, 24f));
    }

    InputField MakeRow(RectTransform root, string label, float y)
    {
        var center = new Vector2(0.5f, 0.5f);
        var lbl = GameAssets.NewText("Label_" + label, root, label, 24, Color.white, TextAnchor.MiddleRight);
        GameAssets.Place(lbl.rectTransform, center, new Vector2(-90f, y), new Vector2(200f, 40f));

        var box = GameAssets.NewPanel("Input_" + label, root, new Color(0.92f, 0.92f, 0.95f));
        GameAssets.Place(box.rectTransform, center, new Vector2(100f, y), new Vector2(160f, 40f));

        var txt = GameAssets.NewText("Text", box.transform, "", 24, Color.black, TextAnchor.MiddleLeft);
        txt.supportRichText = false;
        StretchWithPadding(txt.rectTransform, 10f);
        var ph = GameAssets.NewText("Placeholder", box.transform, "숫자", 22, new Color(0.5f, 0.5f, 0.5f), TextAnchor.MiddleLeft);
        ph.fontStyle = FontStyle.Italic;
        StretchWithPadding(ph.rectTransform, 10f);

        var input = box.gameObject.AddComponent<InputField>();
        input.targetGraphic = box;
        input.textComponent = txt;
        input.placeholder = ph;
        input.contentType = InputField.ContentType.IntegerNumber;
        input.characterLimit = 4;
        return input;
    }

    static void StretchWithPadding(RectTransform rt, float pad)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, 0f); rt.offsetMax = new Vector2(-pad, 0f);
    }

    /// <summary>시작 버튼 / Enter. SessionStartUI 와 동일한 검증 규칙.</summary>
    public void OnStartButton()
    {
        if (!int.TryParse(deviceField.text, out int device) || device <= 0)
        {
            Warn("기기 번호를 입력해줘");
            return;
        }
        if (!int.TryParse(participantField.text, out int participant) || participant <= 0)
        {
            Warn("참가자 번호를 입력해줘");
            return;
        }
        if (GameLogger.Instance == null)
        {
            Warn("GameLogger 가 씬에 없음 (Start 씬 확인)");
            Debug.LogError("[로그] GameLogger 가 씬에 없음");
            return;
        }

        PlayerPrefs.SetInt(DeviceKey, device);
        PlayerPrefs.Save();

        GameLogger.Instance.BeginSession(device, participant);
        SceneManager.LoadScene(gameSceneName);
    }

    void Warn(string message)
    {
        warningText.text = message;
        Debug.LogWarning($"[로그] {message}");
    }
}
