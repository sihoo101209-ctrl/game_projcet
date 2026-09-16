using UnityEngine;

public enum EnemyKind { Melee, Ranged, Tank }

/// <summary>
/// 플레이어·적·보스 생성. MapBuilder 에 프리팹이 꽂혀 있으면 그걸 쓰고,
/// 없으면 색깔 사각형으로 자동 생성한다 (프리팹 없이도 전체 게임 플레이 가능).
/// </summary>
public static class UnitFactory
{
    public static PlayerController CreatePlayer(Vector2 pos)
    {
        var prefab = MapBuilder.Instance != null ? MapBuilder.Instance.playerPrefab : null;
        GameObject go = prefab != null
            ? Object.Instantiate(prefab, pos, Quaternion.identity)
            : MakeUnit("Player", pos, new Color(0.3f, 0.9f, 1f), new Vector2(0.8f, 0.8f));

        go.tag = "Player";   // 없으면 방 진입이 기록되지 않는다 (절대 조건)
        EnsurePhysics(go, 0.45f);
        var pc = go.GetComponent<PlayerController>();
        if (pc == null) pc = go.AddComponent<PlayerController>();
        return pc;
    }

    public static EnemyBase CreateEnemy(EnemyKind kind, Vector2 pos, RoomController room)
    {
        GameObject prefab = null;
        Color color; Vector2 size;
        switch (kind)
        {
            case EnemyKind.Ranged:
                prefab = MapBuilder.Instance != null ? MapBuilder.Instance.rangedPrefab : null;
                color = new Color(1f, 0.6f, 0.15f); size = new Vector2(0.7f, 0.7f);
                break;
            case EnemyKind.Tank:
                prefab = MapBuilder.Instance != null ? MapBuilder.Instance.tankPrefab : null;
                color = new Color(0.55f, 0.1f, 0.1f); size = new Vector2(1.3f, 1.3f);
                break;
            default:
                prefab = MapBuilder.Instance != null ? MapBuilder.Instance.meleePrefab : null;
                color = new Color(0.95f, 0.25f, 0.25f); size = new Vector2(0.8f, 0.8f);
                break;
        }

        GameObject go = prefab != null
            ? Object.Instantiate(prefab, pos, Quaternion.identity)
            : MakeUnit("Enemy_" + kind, pos, color, size);
        EnsurePhysics(go, size.x * 0.55f);

        EnemyBase e = go.GetComponent<EnemyBase>();
        if (e == null)
        {
            switch (kind)
            {
                case EnemyKind.Ranged: e = go.AddComponent<RangedEnemy>(); break;
                case EnemyKind.Tank: e = go.AddComponent<TankEnemy>(); break;
                default: e = go.AddComponent<MeleeEnemy>(); break;
            }
        }
        e.Setup(room);
        return e;
    }

    public static BossController CreateBoss(bool isFinal, Vector2 pos, RoomController room)
    {
        var prefab = MapBuilder.Instance != null ? MapBuilder.Instance.bossPrefab : null;
        float s = isFinal ? 2f : 1.6f;
        GameObject go = prefab != null
            ? Object.Instantiate(prefab, pos, Quaternion.identity)
            : MakeUnit(isFinal ? "Boss_Final" : "Boss_Mini", pos, new Color(0.65f, 0.25f, 0.85f), new Vector2(s, s));
        EnsurePhysics(go, s * 0.55f);

        var boss = go.GetComponent<BossController>();
        if (boss == null) boss = go.AddComponent<BossController>();
        boss.SetupBoss(room, isFinal);
        return boss;
    }

    static GameObject MakeUnit(string name, Vector2 pos, Color color, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GameAssets.WhiteSprite;
        sr.color = color;
        sr.sortingOrder = 5;
        return go;
    }

    static void EnsurePhysics(GameObject go, float radius)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (go.GetComponent<Collider2D>() == null)
        {
            var c = go.AddComponent<CircleCollider2D>();
            // 스케일이 이미 크기를 결정하므로 로컬 반지름은 0.5 근처로
            c.radius = 0.5f * Mathf.Clamp(radius / (go.transform.localScale.x * 0.5f), 0.5f, 1f);
        }
    }
}
