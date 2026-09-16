using UnityEngine;

/// <summary>
/// 문 하나. 잠금(전투 중) · 개방 · 일방통행을 모두 처리한다.
///
/// 통과 가능 조건:
///   1) 인접 방의 전투 잠금이 없고
///   2) 일방통행이면 플레이어가 현재 from 방에 있을 때만
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

    public void Init(int from, int to, bool oneWayDoor)
    {
        fromId = from;
        toId = to;
        oneWay = oneWayDoor;
        sr = GetComponent<SpriteRenderer>();
        solid = GetComponent<Collider2D>();
        Apply();
    }

    public void SetCombatLocked(bool locked)
    {
        combatLocks = Mathf.Max(0, combatLocks + (locked ? 1 : -1));
        Apply();
    }

    bool Passable =>
        combatLocks == 0 &&
        (!oneWay || RoomController.CurrentRoomId == fromId);

    void Update() => Apply();   // 일방통행은 플레이어 위치에 따라 매 프레임 달라질 수 있음

    void Apply()
    {
        bool open = Passable;
        if (solid != null) solid.enabled = !open;
        if (sr != null) sr.color = open ? OpenColor : LockedColor;
    }
}
