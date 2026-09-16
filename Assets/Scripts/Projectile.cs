using UnityEngine;

/// <summary>
/// 투사체. 플레이어 것과 적 것 공용.
/// 벽·문에 닿으면 소멸, 상대 1체 명중 시 소멸 (관통 없음). 같은 편은 통과.
/// </summary>
public class Projectile : MonoBehaviour
{
    bool fromPlayer;
    int damage;
    float speed;
    float range;
    Vector2 dir;
    Vector2 origin;

    public static Projectile Spawn(bool fromPlayer, Vector2 pos, Vector2 dir,
                                   float speed, int damage, float range, Color color)
    {
        GameObject go;
        var prefab = fromPlayer && MapBuilder.Instance != null ? MapBuilder.Instance.projectilePrefab : null;
        if (prefab != null)
        {
            go = Object.Instantiate(prefab, pos, Quaternion.identity);
        }
        else
        {
            go = new GameObject(fromPlayer ? "PlayerShot" : "EnemyShot");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GameAssets.WhiteSprite;
            sr.color = color;
            sr.sortingOrder = 6;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.useFullKinematicContacts = true;   // 2D 키네마틱은 이게 없으면 정적 벽·문과 트리거 이벤트가 안 난다

        var col = go.GetComponent<Collider2D>();
        if (col == null) col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        var p = go.GetComponent<Projectile>();
        if (p == null) p = go.AddComponent<Projectile>();
        p.fromPlayer = fromPlayer;
        p.damage = damage;
        p.speed = speed;
        p.range = range;
        p.dir = dir.normalized;
        p.origin = pos;
        return p;
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
        if (((Vector2)transform.position - origin).sqrMagnitude > range * range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.isTrigger) return;   // 방 트리거 등은 무시

        if (fromPlayer)
        {
            var enemy = col.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeHit(damage);
                Destroy(gameObject);
                return;
            }
            if (col.GetComponentInParent<PlayerController>() != null) return; // 자기 자신 통과
        }
        else
        {
            var player = col.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            if (col.GetComponentInParent<EnemyBase>() != null) return;        // 같은 편 통과
        }

        // 그 외 (벽·잠긴 문) → 소멸
        Destroy(gameObject);
    }
}
