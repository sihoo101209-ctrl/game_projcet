using UnityEngine;

/// <summary>
/// 방마다 하나씩 붙인다. 플레이어가 들어오면 GameLogger 에 방 번호를 알린다.
/// 방 전체를 덮는 빈 GameObject 에 BoxCollider2D 를 달고 이 스크립트를 붙이면 된다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomTrigger : MonoBehaviour
{
    [Header("이 방의 번호 (1~8, 막다른 방은 101 / 102 처럼)")]
    public int roomNumber = 1;

    [Header("막다른 방이면 체크")]
    public bool isDeadEnd = false;

    void Reset()
    {
        // 스크립트를 붙이는 순간 자동으로 트리거로 설정
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameLogger.Instance == null) return;

        GameLogger.Instance.EnterRoom(roomNumber, isDeadEnd);
    }
}
