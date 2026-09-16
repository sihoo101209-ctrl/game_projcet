using UnityEngine;

/// <summary>원거리: HP 2발 · 속도 1.25 · 6유닛 거리 유지 · 1.5초마다 발사.</summary>
public class RangedEnemy : EnemyBase
{
    const float KeepDistance = 6f;
    const float FireInterval = 1.5f;
    const float ShotSpeed = 7f;      // 투사체 속도는 유지 (이속만 절반으로 조정됨)
    const float ShotRange = 14f;

    float nextFireTime;

    protected override void ConfigureStats()
    {
        maxHp = 20;
        moveSpeed = 1.25f;
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
