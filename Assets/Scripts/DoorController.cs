using UnityEngine;

/// <summary>
/// 문 하나. 잠금(전투 중) · 개방 · 일방통행을 모두 처리한다.
///
/// 통과 가능 조건:
///   1) 인접 방의 전투 잠금이 없고
///   2) 일방통행이면 플레이어가 from 방 쪽에 있을 때만 (위치 기준 — 트리거 발동 순서에 흔들리지 않음)
///
/// 플레이어가 문 통로 안에 있는 동안은 절대 콜라이더를 켜지 않는다.
/// (통로 안에서 켜지면 물리가 플레이어를 아무 쪽으로나 튕겨내 방 밖으로 나가는 버그가 난다)
///
/// 적 전용 차단막(EnemyBarrier)은 항상 켜져 있어 적이 자기 방을 벗어나지 못한다. 플레이어와는 충돌 무시.
///
/// 색: 통과 가능 = 초록(반투명), 불가 = 빨강. (필수 — 안 보이면 버그로 오해해 포기함)
/// </summary>
public class DoorController : MonoBehaviour
{
    public int fromId;
    public int toId;
    public bool oneWay;

    static readonly Color LockedColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    static readonly Color OpenColor = new Color(0.15f, 0.8f, 0.3f, 0.45f);

    int combatLocks;              // 인접 방이 전투 중이면 > 0
    SpriteRenderer sr;
    Collider2D solid;
    Collider2D enemyBarrier;
    Vector2 fromCenter;
    Vector2 toCenter;

    public void Init(int from, int to, bool oneWayDoor, Vector2 fromRoomCenter, Vector2 toRoomCenter)
    {
        fromId = from;
        toId = to;
        oneWay = oneWayDoor;
        fromCenter = fromRoomCenter;
        toCenter = toRoomCenter;
        sr = GetComponent<SpriteRenderer>();
        solid = GetComponent<Collider2D>();

        // 부모 스케일을 물려받아 문과 같은 크기. 플레이어만 통과(IgnoreCollision), 적·투사체는 막힘
        var barrier = new GameObject("EnemyBarrier");
        barrier.transform.SetParent(transform, false);
        enemyBarrier = barrier.AddComponent<BoxCollider2D>();

        Apply();
    }

    public void SetCombatLocked(bool locked)
    {
        combatLocks = Mathf.Max(0, combatLocks + (locked ? 1 : -1));
        Apply();
    }

    bool Passable => combatLocks == 0 && (!oneWay || PlayerOnFromSide());

    bool PlayerOnFromSide()
    {
        var p = PlayerController.Instance;
        if (p == null) return true;
        Vector2 pos = p.transform.position;
        return (pos - fromCenter).sqrMagnitude <= (pos - toCenter).sqrMagnitude;
    }

    void Update()
    {
        var p = PlayerController.Instance;
        if (p != null && p.Body != null && enemyBarrier != null
            && !Physics2D.GetIgnoreCollision(enemyBarrier, p.Body))
            Physics2D.IgnoreCollision(enemyBarrier, p.Body, true);

        Apply();
    }

    void Apply()
    {
        bool open = Passable;
        if (solid != null)
        {
            if (open) solid.enabled = false;
            else if (!PlayerOverlaps()) solid.enabled = true;   // 플레이어가 빠져나간 뒤에만 닫힘
        }
        if (sr != null) sr.color = open ? OpenColor : LockedColor;
    }

    /// <summary>플레이어 콜라이더가 문 통로(이 오브젝트의 사각형)와 겹치는가.</summary>
    bool PlayerOverlaps()
    {
        var p = PlayerController.Instance;
        if (p == null || p.Body == null) return false;
        var b = p.Body.bounds;
        Vector3 half = transform.lossyScale * 0.5f;
        Vector3 c = transform.position;
        const float margin = 0.05f;
        return b.max.x > c.x - half.x - margin && b.min.x < c.x + half.x + margin
            && b.max.y > c.y - half.y - margin && b.min.y < c.y + half.y + margin;
    }
}
