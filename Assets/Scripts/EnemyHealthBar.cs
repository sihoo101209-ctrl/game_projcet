using UnityEngine;

/// <summary>
/// 일반 적 머리 위 체력 바. 피격 시에만 표시 → 1초 뒤 서서히 사라짐.
/// 적의 스케일에 영향받지 않도록 부모 없이 따라다닌다.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    const float Width = 1f;
    const float Height = 0.14f;
    const float ShowSeconds = 1f;
    const float FadeSeconds = 0.3f;

    Transform owner;
    SpriteRenderer bg;
    SpriteRenderer fill;
    float shownAt = -999f;

    public static EnemyHealthBar Attach(EnemyBase enemy)
    {
        var go = new GameObject("EnemyHPBar");
        var bar = go.AddComponent<EnemyHealthBar>();
        bar.owner = enemy.transform;
        bar.bg = GameAssets.NewQuad("BG", go.transform, Vector2.zero,
            new Vector2(Width, Height), new Color(0.1f, 0.1f, 0.1f, 0.9f), 7);
        bar.fill = GameAssets.NewQuad("Fill", go.transform, Vector2.zero,
            new Vector2(Width - 0.04f, Height - 0.04f), new Color(0.9f, 0.2f, 0.2f), 8);
        bar.SetAlpha(0f);
        return bar;
    }

    public void Flash(int hp, int maxHp)
    {
        shownAt = Time.time;
        float ratio = Mathf.Clamp01((float)hp / maxHp);
        float w = (Width - 0.04f) * ratio;
        fill.transform.localScale = new Vector3(Mathf.Max(0.001f, w), Height - 0.04f, 1f);
        fill.transform.localPosition = new Vector3(-(Width - 0.04f - w) * 0.5f, 0f, 0f);
        SetAlpha(1f);
    }

    void LateUpdate()
    {
        if (owner == null) { Destroy(gameObject); return; }
        transform.position = owner.position + Vector3.up * (owner.localScale.y * 0.5f + 0.45f);

        float t = Time.time - shownAt;
        if (t <= ShowSeconds) SetAlpha(1f);
        else SetAlpha(1f - Mathf.Clamp01((t - ShowSeconds) / FadeSeconds));
    }

    void SetAlpha(float a)
    {
        var c = bg.color; c.a = 0.9f * a; bg.color = c;
        c = fill.color; c.a = a; fill.color = c;
    }
}
