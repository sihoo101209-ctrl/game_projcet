using TMPro;
using UnityEngine;

/// <summary>
/// 타이틀 화면 우하단의 "ver. A / ver. B" 표시.
/// 잘못된 버전으로 빌드했을 때 테스트 도중에라도 발견할 수 있는 유일한 안전장치.
/// Start 씬의 TMP 텍스트에 붙이고 필드를 연결한다.
/// </summary>
public class VersionLabel : MonoBehaviour
{
    public TMP_Text label;

    void Start()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null) return;
        label.text = GameLogger.Instance != null
            ? "ver. " + GameLogger.Instance.mapVersion
            : "ver. ?";
    }
}
