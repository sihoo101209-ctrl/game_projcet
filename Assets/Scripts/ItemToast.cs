using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템 획득 알림. 상단 1/3 지점, 1.5초 후 페이드아웃.
/// 문 개방 알림과 겹치지 않는다 (문은 색으로만 알림).
/// </summary>
public class ItemToast : MonoBehaviour
{
    const float ShowSeconds = 1.5f;
    const float FadeSeconds = 0.4f;

    Text text;
    float bornAt;

    public static void Show(string message)
    {
        if (GameUI.Instance == null) return;

        var t = GameAssets.NewText("ItemToast", GameUI.Instance.CanvasRoot, message, 30, Color.white);
        // 720 기준 상단 1/3 지점 (위에서 240)
        GameAssets.Place(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -240f), new Vector2(700f, 44f));
        var toast = t.gameObject.AddComponent<ItemToast>();
        toast.text = t;
        toast.bornAt = Time.time;
    }

    void Update()
    {
        float t = Time.time - bornAt;
        if (t <= ShowSeconds) return;

        float a = 1f - Mathf.Clamp01((t - ShowSeconds) / FadeSeconds);
        var c = text.color; c.a = a; text.color = c;
        if (a <= 0f) Destroy(gameObject);
    }
}
