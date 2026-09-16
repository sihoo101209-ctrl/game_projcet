using UnityEngine;

/// <summary>원거리: HP 3발 · 속도 2.5 · 6유닛 거리 유지 · 1.5초마다 발사.</summary>
public class RangedEnemy : EnemyBase
{
    const float KeepDistance = 6f;
    const float FireInterval = 1.5f;
    const float ShotSpeed = 7f;      // 플레이어(5)보다 약간 빠른 정도 — 피할 수 있어야 한다
    const float ShotRange = 14f;

    float nextFireTime;

    protected override void ConfigureStats()
    {
        maxHp = 30;
        moveSpeed = 2.5f;
        contactDamage = 10;
    }

    protected override void Move()
    {
        float d = DistanceToPlayer;
        Vector2 toPlayer = ((Vector2)Player.position - rb.position).normalized;
        if (d > KeepDistance + 0.5f) rb.velocity = toPlayer * moveSpeed;
        else if (d < KeepDistance - 0.5f) rb.velocity = -toPlayer * moveSpeed;
        else rb.velocity = Vector2.zero;
    }

    void Update()
    {
        if (Player == null || Time.timeScale == 0f) return;
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + FireInterval;

        Vector2 dir = ((Vector2)Player.position - rb.position).normalized;
        Projectile.Spawn(false, rb.position + dir * 0.6f, dir,
            ShotSpeed, contactDamage, ShotRange, new Color(1f, 0.35f, 0.85f));
    }
}
