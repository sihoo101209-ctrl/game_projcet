using UnityEngine;

// 센서 기울기로 플레이어를 좌우 이동
public class PlayerMove : MonoBehaviour
{
    public float speed = 8f;        // 최대 이동 속도
    public float limitX = 8f;       // 화면 밖으로 못 나가게 하는 x 한계
    public float deadZone = 0.1f;   // 이 값보다 작게 기울이면 무시 (떨림 방지)

    void Update()
    {
        if (GameManager.isOver) return;          // 게임 오버면 정지

        float t = SerialReceiver.tilt;
        if (Mathf.Abs(t) < deadZone) t = 0f;

        Vector3 pos = transform.position;
        pos.x += t * speed * Time.deltaTime;     // 방향 반대면 -t * speed 로
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;
    }
}
