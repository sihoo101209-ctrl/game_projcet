using UnityEngine;

/// <summary>
/// 방 하나가 통째로 화면에 들어오는 고정 카메라.
/// 플레이어 추적 금지 (시야 차이가 실험 변수가 됨) — 방 진입 시 즉시 스냅만 한다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        var cam = GetComponent<Camera>();
        cam.orthographic = true;
        // 방 10 + 벽 1+1 = 세로 12유닛이 정확히 화면에 들어오게 (코드로 고정)
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
    }

    public static void SnapTo(Vector2 center)
    {
        if (Instance == null)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Instance = cam.GetComponent<CameraController>();
            if (Instance == null) Instance = cam.gameObject.AddComponent<CameraController>();
        }
        Instance.transform.position = new Vector3(center.x, center.y, -10f);
    }
}
