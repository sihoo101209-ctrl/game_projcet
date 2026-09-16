using UnityEngine;

/// <summary>
/// 방 하나가 통째로 화면에 들어오는 고정 카메라.
/// 플레이어 추적 금지 (시야 차이가 실험 변수가 됨) — 방 진입 시 즉시 스냅만 한다.
///
/// 세로 12유닛(방 10 + 벽 1+1)을 코드로 고정. 1280x720 창에서는 가로 21.3유닛이 보여
/// 옆 방 바닥이 좌우로 살짝(약 0.7유닛) 비치는데, 창 크기가 고정이라 모든 참가자에게 동일하다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    const float ViewHeight = 12f;   // 방 10 + 벽 1+1

    void Awake()
    {
        Instance = this;
        var cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = ViewHeight * 0.5f;   // 코드로 고정
        cam.rect = new Rect(0f, 0f, 1f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
    }

    public static void SnapTo(Vector2 center)
    {
        if (Instance == null)
        {
            var main = Camera.main;
            if (main == null) return;
            Instance = main.GetComponent<CameraController>();
            if (Instance == null) Instance = main.gameObject.AddComponent<CameraController>();
        }
        Instance.transform.position = new Vector3(center.x, center.y, -10f);
    }
}
