using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 레벨 배치 A/B 테스트용 로그 수집기.
/// 씬이 바뀌어도 파괴되지 않고, 세션이 끝나면 CSV 두 개에 덧붙여 저장한다.
///   summary.csv : 참가자 1명 = 1행
///   deaths.csv  : 사망 1회  = 1행
///
/// 중요: 총 플레이시간은 "시작 버튼"이 아니라 "방 1 최초 진입" 시점부터 측정한다.
/// </summary>
public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    [Header("빌드할 때 A 또는 B 로 지정")]
    public string mapVersion = "A";

    [Header("타이틀 화면에서 입력됨 (여기는 테스트용)")]
    public int deviceId = 1;
    public int participantId = 0;

    const string SummaryFile = "summary.csv";
    const string DeathFile = "deaths.csv";

    const string SummaryHeader =
        "기기번호,번호,버전,총플레이시간(초),클리어시간(초),시작방체류시간(초)," +
        "사망횟수,되돌아간횟수,맵확인횟수,포기여부,포기시점,되돌림복도진입,평균FPS,최저FPS";
    const string DeathHeader =
        "기기번호,번호,버전,몇번째죽음,x,y,방번호";

    // 세션 상태
    bool sessionActive;
    bool finished;

    // 타이머 (방 1 진입부터)
    float sessionStartTime;     // 시작 버튼을 누른 시각 (시작방 체류시간 계산용)
    float playStartTime;        // 방 1 최초 진입 시각
    bool timerStarted;
    float tutorialSeconds;

    // 지표
    int deathCount;
    int currentRoom;
    int revisitCount;
    int mapViewCount;
    bool enteredDeadEnd;

    readonly HashSet<int> visited = new HashSet<int>();
    readonly List<string> pendingDeaths = new List<string>();

    // FPS 샘플링 (1초 단위)
    readonly List<float> fpsSamples = new List<float>();
    float fpsTimer;
    int fpsFrames;

    /// <summary>
    /// 에디터에서는 persistentDataPath, 빌드에서는 exe 옆의 logs 폴더.
    /// 학교 노트북에서 AppData 숨김 폴더를 뒤지지 않아도 되게 하기 위함.
    /// </summary>
    public string SaveFolder
    {
        get
        {
#if UNITY_EDITOR
            return Application.persistentDataPath;
#else
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "logs"));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
#endif
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        if (!sessionActive || !timerStarted) return;

        fpsFrames++;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 1f)
        {
            fpsSamples.Add(fpsFrames / fpsTimer);
            fpsFrames = 0;
            fpsTimer = 0f;
        }
    }

    // ────────────────── 세션 ──────────────────

    /// <summary>타이틀 화면의 시작 버튼에서 호출. 타이머는 아직 시작되지 않는다.</summary>
    public void BeginSession(int device, int participant)
    {
        deviceId = device;
        participantId = participant;

        sessionStartTime = Time.time;
        sessionActive = true;
        finished = false;
        timerStarted = false;
        tutorialSeconds = 0f;

        deathCount = 0;
        revisitCount = 0;
        mapViewCount = 0;
        currentRoom = 0;
        enteredDeadEnd = false;
        visited.Clear();
        pendingDeaths.Clear();
        fpsSamples.Clear();
        fpsTimer = 0f;
        fpsFrames = 0;

        Debug.Log($"[로그] 세션 시작 — 기기 {device}, 참가자 {participant}, 버전 {mapVersion}");
    }

    /// <summary>방 1 최초 진입 시 호출. 여기서부터 총 플레이시간이 측정된다.</summary>
    public void StartTimerAtRoom1()
    {
        if (!sessionActive || timerStarted) return;   // 되돌아와도 재시작 금지

        playStartTime = Time.time;
        tutorialSeconds = playStartTime - sessionStartTime;
        timerStarted = true;

        Debug.Log($"[로그] 타이머 시작 — 시작방 체류 {tutorialSeconds:F1}초");
    }

    // ────────────────── 게임 중 ──────────────────

    public void EnterRoom(int roomNumber, bool isDeadEnd = false)
    {
        if (!sessionActive) return;

        currentRoom = roomNumber;
        if (isDeadEnd) enteredDeadEnd = true;
        if (roomNumber == 1) StartTimerAtRoom1();

        // HashSet.Add 는 이미 있으면 false → 재방문
        if (!visited.Add(roomNumber)) revisitCount++;
    }

    public void RecordDeath(Vector2 position)
    {
        if (!sessionActive) return;

        deathCount++;
        pendingDeaths.Add(string.Join(",",
            deviceId,
            participantId,
            mapVersion,
            deathCount,
            position.x.ToString("F2", CultureInfo.InvariantCulture),
            position.y.ToString("F2", CultureInfo.InvariantCulture),
            currentRoom));
    }

    /// <summary>ESC 메뉴에서 맵 화면을 열 때마다 호출. 길 잃음을 행동으로 측정하는 지표.</summary>
    public void RecordMapView()
    {
        if (!sessionActive) return;
        mapViewCount++;
    }

    /// <summary>사망 화면에 "포기하셔도 됩니다" 안내를 띄울지 판단.</summary>
    public bool ShouldSuggestQuit => deathCount > 10;

    public int DeathCount => deathCount;
    public int CurrentRoom => currentRoom;

    public void FinishCleared() => EndSession(true);
    public void FinishGaveUp() => EndSession(false);

    // ────────────────── 저장 ──────────────────

    void EndSession(bool clearedBoss)
    {
        if (!sessionActive || finished) return;
        finished = true;
        sessionActive = false;

        float elapsed = timerStarted ? Time.time - playStartTime : 0f;
        string total = elapsed.ToString("F1", CultureInfo.InvariantCulture);

        // 포기자는 클리어시간을 빈칸으로 (0 으로 두면 평균이 망가짐)
        string clearTime = clearedBoss ? total : "";
        string gaveUp = clearedBoss ? "X" : "O";
        string quitRoom = clearedBoss ? "" : currentRoom.ToString();

        float avgFps = 0f, minFps = 0f;
        if (fpsSamples.Count > 0)
        {
            float sum = 0f;
            minFps = float.MaxValue;
            foreach (var f in fpsSamples)
            {
                sum += f;
                if (f < minFps) minFps = f;
            }
            avgFps = sum / fpsSamples.Count;
        }

        AppendRows(SummaryFile, SummaryHeader, new List<string> {
            string.Join(",",
                deviceId,
                participantId,
                mapVersion,
                total,
                clearTime,
                tutorialSeconds.ToString("F1", CultureInfo.InvariantCulture),
                deathCount,
                revisitCount,
                mapViewCount,
                gaveUp,
                quitRoom,
                enteredDeadEnd ? "O" : "X",
                avgFps.ToString("F1", CultureInfo.InvariantCulture),
                minFps.ToString("F1", CultureInfo.InvariantCulture))
        });

        if (pendingDeaths.Count > 0)
            AppendRows(DeathFile, DeathHeader, pendingDeaths);

        Debug.Log($"[로그] 저장 완료 ({(clearedBoss ? "클리어" : "포기")}) → {SaveFolder}");
    }

    void AppendRows(string fileName, string header, List<string> rows)
    {
        string folder = SaveFolder;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, fileName);
        bool isNew = !File.Exists(path);

        var sb = new StringBuilder();
        if (isNew) sb.AppendLine(header);
        foreach (var r in rows) sb.AppendLine(r);

        // BOM 포함 UTF-8 → 엑셀에서 한글이 안 깨진다
        File.AppendAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    /// <summary>창을 강제로 닫아도 포기로 기록되게.</summary>
    void OnApplicationQuit()
    {
        if (sessionActive && !finished) EndSession(false);
    }

#if UNITY_EDITOR
    [ContextMenu("저장 폴더 열기")]
    void OpenSaveFolder() => Application.OpenURL("file://" + SaveFolder);
#endif
}
