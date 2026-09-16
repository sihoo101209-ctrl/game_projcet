using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 핵심: Resources/map_{version}.json 을 읽어 방·벽·문·바닥 텍스트를 런타임 생성한다.
/// A/B 전환은 GameLogger 의 mapVersion 하나로 끝난다.
///
/// Game 씬에는 Main Camera 와, 이 스크립트가 붙은 빈 오브젝트 하나만 있으면 된다.
/// 프리팹 슬롯을 비워두면 색깔 사각형으로 자동 생성되므로 에디터 수작업 없이도 플레이 가능.
/// </summary>
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder Instance { get; private set; }
    public static MapData CurrentMap { get; private set; }
    public static Vector2 RoomSize { get; private set; }

    [Header("프리팹 (비워두면 자동 생성)")]
    public GameObject playerPrefab;
    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public GameObject tankPrefab;
    public GameObject bossPrefab;
    public GameObject projectilePrefab;

    const float WallThickness = 1f;   // roomSize + 벽 2 = spacing 이 되도록 맵 JSON과 맞춘 값
    const float DoorGap = 3f;         // 문이 뚫리는 폭

    enum Side { Up, Down, Left, Right }

    void Awake()
    {
        Instance = this;
        RoomController.ResetStatics();

        // 에디터에서 Game 씬만 바로 Play 했을 때를 위한 안전장치
        if (GameLogger.Instance == null)
        {
            var g = new GameObject("GameLogger(자동생성)");
            g.AddComponent<GameLogger>().BeginSession(0, 0);
            Debug.LogWarning("[맵] GameLogger 가 없어 테스트 세션 생성 (기기 0 / 참가자 0 — 본 데이터 아님)");
        }

        string version = GameLogger.Instance.mapVersion;
        var map = MapData.Load(version);
        if (map == null) return;
        CurrentMap = map;
        RoomSize = new Vector2(map.roomSize.w, map.roomSize.h);

        var cam = GameAssets.EnsureCamera();
        if (cam.GetComponent<CameraController>() == null)
            cam.gameObject.AddComponent<CameraController>();

        BuildRooms(map);
        BuildDoors(map);
        LogEnemyCount(map);

        if (GameUI.Instance == null)
            new GameObject("GameUI").AddComponent<GameUI>();

        SpawnPlayer(map);
    }

    // ────────────────── 방 ──────────────────

    void BuildRooms(MapData map)
    {
        // 방마다 어느 쪽에 문이 있는지 미리 계산
        var doorSides = new Dictionary<int, HashSet<Side>>();
        foreach (var r in map.rooms) doorSides[r.id] = new HashSet<Side>();

        foreach (var d in map.doors)
        {
            var a = map.GetRoom(d.from);
            var b = map.GetRoom(d.to);
            if (a == null || b == null)
            {
                Debug.LogError($"[맵] 문 {d.from}→{d.to} 가 없는 방을 가리킴");
                continue;
            }
            int dx = b.GridX - a.GridX, dy = b.GridY - a.GridY;
            if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1)
            {
                Debug.LogError($"[맵] 문 {d.from}→{d.to} 는 인접하지 않은 방 사이에 있음");
                continue;
            }
            Side sa = dx == 1 ? Side.Right : dx == -1 ? Side.Left : dy == 1 ? Side.Up : Side.Down;
            Side sb = dx == 1 ? Side.Left : dx == -1 ? Side.Right : dy == 1 ? Side.Down : Side.Up;
            doorSides[a.id].Add(sa);
            doorSides[b.id].Add(sb);
        }

        foreach (var r in map.rooms)
        {
            var center = r.Center(map.spacing);
            var go = new GameObject($"Room_{r.id}");
            go.transform.position = center;

            // 바닥
            GameAssets.NewQuad("Floor", go.transform, Vector2.zero,
                new Vector2(map.roomSize.w, map.roomSize.h), new Color(0.13f, 0.13f, 0.16f), 0);

            // 벽 4면 (doors 배열에 없는 인접 쌍은 자동으로 벽 — 문 없는 면은 통짜 벽)
            foreach (Side s in System.Enum.GetValues(typeof(Side)))
                BuildWall(go.transform, s, doorSides[r.id].Contains(s), map.roomSize.w, map.roomSize.h);

            // 방 트리거 (로그용 RoomTrigger + 게임플레이용 RoomController 가 같이 사용)
            var trig = go.AddComponent<BoxCollider2D>();
            trig.isTrigger = true;
            trig.size = new Vector2(map.roomSize.w + 1f, map.roomSize.h + 1f);

            var rt = go.AddComponent<RoomTrigger>();
            rt.roomNumber = r.id;
            rt.isDeadEnd = r.isDeadEnd;

            var rc = go.AddComponent<RoomController>();
            rc.Init(r, center);

            // 시작방 바닥 안내 문구
            if (r.floorText != null && r.floorText.Length > 0)
                BuildFloorText(go.transform, r.floorText);
        }
    }

    void BuildWall(Transform room, Side side, bool hasDoor, float w, float h)
    {
        bool horizontal = side == Side.Up || side == Side.Down;
        // 가로 벽은 모서리까지 덮고(w+2T), 세로 벽은 그 사이(h)만
        float length = horizontal ? w + WallThickness * 2f : h;
        float offset = horizontal ? h * 0.5f + WallThickness * 0.5f : w * 0.5f + WallThickness * 0.5f;
        Vector2 dir = side == Side.Up ? Vector2.up : side == Side.Down ? Vector2.down
                    : side == Side.Left ? Vector2.left : Vector2.right;
        Vector2 wallCenter = dir * offset;

        void Segment(float segLen, float along)
        {
            Vector2 pos = horizontal
                ? new Vector2(along, wallCenter.y)
                : new Vector2(wallCenter.x, along);
            Vector2 size = horizontal
                ? new Vector2(segLen, WallThickness)
                : new Vector2(WallThickness, segLen);
            var sr = GameAssets.NewQuad($"Wall_{side}", room, pos, size, new Color(0.35f, 0.35f, 0.42f), 2);
            sr.gameObject.AddComponent<BoxCollider2D>();   // 스케일이 곧 크기 (1x1 스프라이트)
        }

        if (!hasDoor)
        {
            Segment(length, 0f);
        }
        else
        {
            float seg = (length - DoorGap) * 0.5f;
            float pos = (DoorGap + seg) * 0.5f;
            Segment(seg, -pos);
            Segment(seg, pos);
        }
    }

    void BuildFloorText(Transform room, string[] lines)
    {
        var go = new GameObject("FloorText");
        go.transform.SetParent(room, false);
        var tm = go.AddComponent<TextMesh>();
        tm.font = GameAssets.UIFont;
        tm.text = string.Join("\n", lines);
        tm.fontSize = 48;
        tm.characterSize = 0.055f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.55f, 0.55f, 0.6f);
        var mr = go.GetComponent<MeshRenderer>();
        mr.material = GameAssets.UIFont.material;
        mr.sortingOrder = 1;
    }

    // ────────────────── 문 ──────────────────

    void BuildDoors(MapData map)
    {
        foreach (var d in map.doors)
        {
            var a = map.GetRoom(d.from);
            var b = map.GetRoom(d.to);
            if (a == null || b == null) continue;
            int dx = b.GridX - a.GridX, dy = b.GridY - a.GridY;
            if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1) continue;

            Vector2 ca = a.Center(map.spacing), cb = b.Center(map.spacing);
            Vector2 mid = (ca + cb) * 0.5f;
            bool horizontalNeighbors = dy == 0;
            // 좌우 이웃 → 세로 벽에 난 문 (x로 얇고 y로 넓음), 상하 이웃 → 반대
            Vector2 size = horizontalNeighbors
                ? new Vector2(WallThickness * 2f, DoorGap)
                : new Vector2(DoorGap, WallThickness * 2f);

            var sr = GameAssets.NewQuad($"Door_{d.from}_{d.to}", null, Vector2.zero, size, Color.red, 3);
            sr.transform.position = mid;
            sr.gameObject.AddComponent<BoxCollider2D>();
            var door = sr.gameObject.AddComponent<DoorController>();
            door.Init(d.from, d.to, d.oneWay);

            if (RoomController.All.TryGetValue(d.from, out var ra)) ra.AddDoor(door);
            if (RoomController.All.TryGetValue(d.to, out var rb)) rb.AddDoor(door);
        }
    }

    // ────────────────── 검증 · 스폰 ──────────────────

    void LogEnemyCount(MapData map)
    {
        int melee = 0, ranged = 0, tank = 0, mini = 0, final = 0;
        foreach (var r in map.rooms)
        {
            if (r.enemies != null) { melee += r.enemies.melee; ranged += r.enemies.ranged; tank += r.enemies.tank; }
            if (r.boss == "mini") mini++;
            if (r.boss == "final") final++;
        }
        const int expected = 30;                    // A/B 모두 이 값이어야 데이터가 유효
        int total = melee + ranged + tank + mini;   // 최종 보스는 별도
        string msg = $"[맵 {map.version}] 총 적 수 = {total} (근접 {melee} + 원거리 {ranged} + 탱커 {tank} + 준보스 {mini}), 최종 보스 {final} 별도";
        if (total == expected) Debug.Log(msg + " ✓");
        else Debug.LogError(msg + $" — {expected}이 아님! 데이터 무효 위험, JSON 확인 필요");
    }

    void SpawnPlayer(MapData map)
    {
        RoomData tut = null;
        foreach (var r in map.rooms)
            if (r.type == "tutorial") { tut = r; break; }
        if (tut == null) tut = map.GetRoom(0);
        if (tut == null) { Debug.LogError("[맵] 시작방(tutorial)이 없음"); return; }

        var center = tut.Center(map.spacing);
        UnitFactory.CreatePlayer(center);
        RoomController.CurrentRoomId = tut.id;
        CameraController.SnapTo(center);
        if (GameUI.Instance != null) GameUI.Instance.SetRoom(tut.id);
    }
}
