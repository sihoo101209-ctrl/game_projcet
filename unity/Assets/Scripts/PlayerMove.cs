using UnityEngine;

// 센서 기울기로 플레이어를 좌우 이동 (센서 없을 땐 ←→ / A D 키로도 가능)
public class PlayerMove : MonoBehaviour
{
    public float speed = 8f;        // 최대 이동 속도
    public float limitX = 8f;       // 화면 밖으로 못 나가게 하는 x 한계
    public float deadZone = 0.1f;   // 이 값보다 작게 기울이면 무시 (떨림 방지)
    public bool invert = false;     // 방향이 반대면 체크

    void Update()
    {
        if (GameManager.isOver) return;          // 게임 오버면 정지

        float t = SerialReceiver.tilt;
        if (Mathf.Abs(t) < deadZone) t = 0f;
        if (invert) t = -t;

        float k = KeyInput();                    // 키보드 입력이 있으면 그걸 우선
        if (k != 0f) t = k;

        Vector3 pos = transform.position;
        pos.x += t * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        transform.position = pos;
    }

    // 옛 입력 방식/새 Input System 둘 다 대응
    float KeyInput()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxisRaw("Horizontal");
#elif ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return 0f;
        float v = 0f;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) v -= 1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) v += 1f;
        return v;
#else
        return 0f;
#endif
    }
}
