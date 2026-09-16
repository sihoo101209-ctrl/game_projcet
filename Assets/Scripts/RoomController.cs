using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 하나의 게임플레이 상태: 진입 → 문 잠금 + 적 스폰 → 전멸 → 문 개방 + 보상.
/// 로그용 RoomTrigger 와 같은 오브젝트(같은 트리거 콜라이더)에 붙는다.
///
/// - 클리어한 방은 재진입해도 적이 나오지 않는다 (실험 타당성 필수 조건)
/// - 사망 후 다시 시도 시 현재 방만 리셋 (ResetForRetry)
/// - 적 배치는 방 번호 시드 고정 → A/B 에서 같은 방은 같은 배치
/// </summary>
public class RoomController : MonoBehaviour
{
    public static readonly Dictionary<int, RoomController> All = new Dictionary<int, RoomController>();
    public static readonly HashSet<int> Cleared = new HashSet<int>();
    public static int CurrentRoomId;

    public static void ResetStatics()
    {
        All.Clear();
        Cleared.Clear();
        CurrentRoomId = 0;
    }

    public RoomData data;
    public Vector2 center;
    /// <summary>사망 시 부활 지점 — 이 방에 들어온 순간의 플레이어 위치.</summary>
    public Vector2 entryPoint;

    readonly List<DoorController> doors = new List<DoorController>();
    readonly List<EnemyBase> alive = new List<EnemyBase>();
    bool inCombat;

    public bool IsCleared => Cleared.Contains(data.id);

    public void Init(RoomData d, Vector2 c)
    {
        data = d;
        center = c;
        entryPoint = c;
        All[d.id] = this;

        // 적도 보스도 없는 방(시작방)은 처음부터 클리어 취급 — 문 항상 열림, 맵에도 표시됨
        if (d.TotalEnemies == 0 || !d.lockDoors)
            Cleared.Add(d.id);
    }

    public void AddDoor(DoorController door) => doors.Add(door);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Enter(other.transform);
    }

    void Enter(Transform player)
    {
        bool sameRoom = CurrentRoomId == data.id;
        CurrentRoomId = data.id;
        CameraController.SnapTo(center);
        if (GameUI.Instance != null) GameUI.Instance.SetRoom(data.id);

        if (IsCleared || (sameRoom && inCombat)) return;
        StartCombat(player.position);
    }

    void StartCombat(Vector2 playerPos)
    {
        entryPoint = playerPos;
        inCombat = true;
        foreach (var d in doors) d.SetCombatLocked(true);
        SpawnAll();
        if (data.HasBoss && GameUI.Instance != null)
        {
            var boss = alive.Find(e => e is BossController);
            if (boss != null) GameUI.Instance.ShowBossBar(data.boss == "final", boss.maxHp);
        }
    }

    void SpawnAll()
    {
        // 방 번호 시드 → A/B 에서 같은 방은 같은 배치 (CLAUDE.md 지시)
        var rng = new System.Random(data.id * 7919);
        var used = new List<Vector2>();
        float rx = MapBuilder.RoomSize.x * 0.5f - 3f;   // 벽·문에서 충분히 떨어진 안쪽 영역
        float ry = MapBuilder.RoomSize.y * 0.5f - 2.5f;

        Vector2 NextPos()
        {
            for (int t = 0; t < 60; t++)
            {
                var p = new Vector2(
                    (float)(rng.NextDouble() * 2.0 - 1.0) * rx,
                    (float)(rng.NextDouble() * 2.0 - 1.0) * ry);
                bool ok = true;
                foreach (var u in used)
                    if (Vector2.Distance(u, p) < 2.2f) { ok = false; break; }
                if (ok) { used.Add(p); return p; }
            }
            return Vector2.zero;
        }

        if (data.enemies != null)
        {
            for (int i = 0; i < data.enemies.melee; i++)
                alive.Add(UnitFactory.CreateEnemy(EnemyKind.Melee, center + NextPos(), this));
            for (int i = 0; i < data.enemies.ranged; i++)
                alive.Add(UnitFactory.CreateEnemy(EnemyKind.Ranged, center + NextPos(), this));
            for (int i = 0; i < data.enemies.tank; i++)
                alive.Add(UnitFactory.CreateEnemy(EnemyKind.Tank, center + NextPos(), this));
        }
        if (data.HasBoss)
            alive.Add(UnitFactory.CreateBoss(data.boss == "final", center, this));
    }

    public void OnEnemyDied(EnemyBase enemy)
    {
        alive.Remove(enemy);
        if (inCombat && alive.Count == 0) ClearRoom();
    }

    void ClearRoom()
    {
        inCombat = false;
        Cleared.Add(data.id);
        foreach (var d in doors) d.SetCombatLocked(false);
        ApplyReward();
    }

    /// <summary>전멸 순간 자동 획득 — 바닥에 놓지 않는다 (놓치면 화력 차이가 배치 효과에 섞임).</summary>
    void ApplyReward()
    {
        if (string.IsNullOrEmpty(data.reward)) return;
        var player = PlayerController.Instance;
        if (player == null) return;

        switch (data.reward)
        {
            case "heal":
                player.Heal(60);
                ItemToast.Show("체력 회복  +60");
                break;
            case "weapon":
                player.UpgradeWeapon();
                ItemToast.Show("무기 강화!  공격 속도 상승");
                break;
        }
    }

    /// <summary>사망 → 다시 시도. 이 방의 적만 전부 리셋하고 다시 스폰한다. 문은 잠긴 상태 유지.</summary>
    public void ResetForRetry()
    {
        foreach (var e in alive)
            if (e != null) Destroy(e.gameObject);
        alive.Clear();

        foreach (var p in FindObjectsOfType<Projectile>())
            Destroy(p.gameObject);

        if (!inCombat) return;   // 이론상 전투 중에만 죽지만 방어적으로
        SpawnAll();
        if (data.HasBoss && GameUI.Instance != null)
        {
            var boss = alive.Find(e => e is BossController);
            if (boss != null) GameUI.Instance.ShowBossBar(data.boss == "final", boss.maxHp);
        }
    }
}
