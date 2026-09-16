using UnityEngine;

/// <summary>
/// 방 하나(방 16x10 + 벽 = 18x12)가 정확히 화면에 들어오는 고정 카메라.
/// 플레이어 추적 금지 (시야 차이가 실험 변수가 됨) — 방 진입 시 즉시 스냅만 한다.
///
/// 화면 비율이 3:2 가 아니면 남는 부분을 검은 띠로 가려서(레터박스) 옆 방이 비치지 않게 한다.
/// 1280x720 창에서는 좌우에 약 100px 씩 검은 띠가 생기는 게 정상.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    const float ViewWidth = 18f;    // 방 16 + 벽 1+1
    const float ViewHeight = 12f;   // 방 10 + 벽 1+1

    Camera cam;
    int lastW, lastH;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = ViewHeight * 0.5f;   // 코드로 고정
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);

        // 메인 카메라 뷰포트 바깥을 검게 칠하는 배경 카메라 (아무것도 그리지 않고 지우기만)
        var back = new GameObject("LetterboxBackdrop").AddComponent<Camera>();
        back.transform.SetParent(transform, false);
        back.depth = cam.depth - 1f;
        back.clearFlags = CameraClearFlags.SolidColor;
        back.backgroundColor = Color.black;
        back.cullingMask = 0;
        back.orthographic = true;

        ApplyViewport();
    }

    void LateUpdate()
    {
        if (Screen.width != lastW || Screen.height != lastH) ApplyViewport();
    }

    void ApplyViewport()
    {
        lastW = Screen.width;
        lastH = Screen.height;
        if (lastW <= 0 || lastH <= 0) return;

        float target = ViewWidth / ViewHeight;
        float screen = (float)lastW / lastH;
        if (screen > target)
        {
            float w = target / screen;
            cam.rect = new Rect((1f - w) * 0.5f, 0f, w, 1f);
        }
        else
        {
            float h = screen / target;
            cam.rect = new Rect(0f, (1f - h) * 0.5f, 1f, h);
        }
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
