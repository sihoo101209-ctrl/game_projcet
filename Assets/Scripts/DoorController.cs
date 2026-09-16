using UnityEngine;

/// <summary>
/// 문 하나. 잠금(전투 중) · 개방 · 일방통행을 처리한다.
///
/// 통과 방식은 순간이동: 열린 문에 닿으면 반대편 방의 문 앞으로 바로 옮겨진다 (아이작 방식).
/// 플레이어가 문 통로 안에 머무는 순간이 없으므로, 방에 들어가자마자 문을 잠가도 끼거나 튕기지 않는다.
///
/// 통과 가능 조건: 인접 방 전투 잠금 없음 + (일방통행이면 from 방 쪽에서 닿았을 때만)
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
    Vector2 fromCenter;
    Vector2 toCenter;
    Vector2 axis;                 // 문을 가로지르는 축 (좌우 이웃이면 x, 상하 이웃이면 y)
    bool toIsPositive;            // to 방이 axis 의 + 방향에 있는가
    float halfDepth;              // 문 통로 절반 두께 (= 벽 두께)

    public void Init(int from, int to, bool oneWayDoor, Vector2 fromRoomCenter, Vector2 toRoomCenter)
    {
        fromId = from;
        toId = to;
        oneWay = oneWayDoor;
        fromCenter = fromRoomCenter;
        toCenter = toRoomCenter;
        sr = GetComponent<SpriteRenderer>();
        solid = GetComponent<Collider2D>();

        Vector2 d = toCenter - fromCenter;
        axis = Mathf.Abs(d.x) >= Mathf.Abs(d.y) ? Vector2.right : Vector2.up;
        toIsPositive = Vector2.Dot(d, axis) > 0f;
        halfDepth = (axis.x != 0f ? transform.lossyScale.x : transform.lossyScale.y) * 0.5f;

        // 통과 감지용 트리거 (문과 같은 크기). 물리 차단은 위의 solid 가 담당
        gameObject.AddComponent<BoxCollider2D>().isTrigger = true;

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

    void Update() => Apply();   // 일방통행은 플레이어 위치에 따라 달라짐

    void Apply()
    {
        bool open = Passable;
        if (solid != null) solid.enabled = !open;
        if (sr != null) sr.color = open ? OpenColor : LockedColor;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !Passable) return;
        var p = PlayerController.Instance;
        if (p == null || p.IsDead) return;

        Vector2 door = transform.position;
        float side = Vector2.Dot((Vector2)p.transform.position - door, axis) >= 0f ? 1f : -1f;  // 플레이어가 있는 쪽
        Vector2 target = door - axis * side * (halfDepth + p.Radius + 0.3f);                    // 반대편 방 안쪽
        p.TeleportTo(target);

        int targetId = (side > 0f) == toIsPositive ? fromId : toId;
        if (RoomController.All.TryGetValue(targetId, out var room)) room.Enter(p.transform);
    }
}
