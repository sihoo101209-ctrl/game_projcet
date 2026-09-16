using UnityEngine;

/// <summary>
/// 준보스(방 7)·최종 보스(방 8) 공용. 변수만 분기 — 새 프리팹 금지.
///
///   준보스: HP 18발 · 돌진만 · 접촉 15
///   보스  : HP 36발 · 8유닛 초과 돌진 / 이하 샷건(60도 5발, 사거리 8) · 접촉 20
///
/// 돌진 = 1초 조준 → 직선 돌진 → 2초 경직. 이후 전체 쿨타임 1.5초.
/// 샷건 사거리 8 = 패턴 전환 거리 8 (일부러 일치 — "8유닛 밖 안전지대" 규칙 통일).
/// </summary>
public class BossController : EnemyBase
{
    public bool isFinal;

    const float PatternRange = 8f;
    const float AimSeconds = 1f;
    const float DashSeconds = 0.8f;
    const float DashSpeed = 14f;
    const float RecoverSeconds = 2f;
    const float CooldownSeconds = 1.5f;
    const float ShotgunRange = 8f;
    const float ShotgunSpeed = 9f;
    const int ShotgunPellets = 5;
    const float ShotgunArc = 60f;

    enum State { Decide, Aim, Dash, Recover, Cooldown }
    State state = State.Decide;
    float stateUntil;
    Vector2 dashDir;
    SpriteRenderer sr;
    Color baseColor;

    protected override void ConfigureStats()
    {
        // 기본값은 준보스 — SetupBoss 에서 최종 보스로 덮어씀
        maxHp = 180;
        contactDamage = 15;
        moveSpeed = 0f;
    }

    public void SetupBoss(RoomController r, bool final)
    {
        Setup(r);
        isFinal = final;
        maxHp = final ? 360 : 180;    // 36발 / 18발
        contactDamage = final ? 20 : 15;
        hp = maxHp;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    protected override void FixedUpdate()
    {
        if (Player == null) { rb.velocity = Vector2.zero; return; }

        switch (state)
        {
            case State.Decide:
                rb.velocity = Vector2.zero;
                if (isFinal && DistanceToPlayer <= PatternRange) FireShotgun();
                else BeginState(State.Aim, AimSeconds);
                break;

            case State.Aim:
                rb.velocity = Vector2.zero;
                if (sr != null)   // 조준 텔레그래프: 밝게 깜빡임
                    sr.color = Color.Lerp(baseColor, Color.white, Mathf.PingPong(Time.time * 6f, 1f));
                if (Time.time >= stateUntil)
                {
                    dashDir = ((Vector2)Player.position - rb.position).normalized;
                    BeginState(State.Dash, DashSeconds);
                }
                break;

            case State.Dash:
                rb.velocity = dashDir * DashSpeed;
                if (Time.time >= stateUntil) BeginState(State.Recover, RecoverSeconds);
                break;

            case State.Recover:
            case State.Cooldown:
                rb.velocity = Vector2.zero;
                if (sr != null) sr.color = baseColor;
                if (Time.time >= stateUntil)
                    BeginState(state == State.Recover ? State.Cooldown : State.Decide,
                               state == State.Recover ? CooldownSeconds : 0f);
                break;
        }
    }

    void BeginState(State s, float duration)
    {
        state = s;
        stateUntil = Time.time + duration;
        if (sr != null && s != State.Aim) sr.color = baseColor;
    }

    void FireShotgun()
    {
        Vector2 forward = ((Vector2)Player.position - rb.position).normalized;
        float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
        float step = ShotgunArc / (ShotgunPellets - 1);
        for (int i = 0; i < ShotgunPellets; i++)
        {
            float a = (baseAngle - ShotgunArc * 0.5f + step * i) * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Projectile.Spawn(false, rb.position + dir * 1.2f, dir,
                ShotgunSpeed, 10, ShotgunRange, new Color(0.9f, 0.4f, 1f));
        }
        BeginState(State.Cooldown, CooldownSeconds);
    }

    protected override void OnHit()
    {
        if (GameUI.Instance != null) GameUI.Instance.UpdateBossBar(hp, maxHp);
    }

    protected override void Die()
    {
        if (GameUI.Instance != null) GameUI.Instance.HideBossBar();
        if (isFinal)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.FinishCleared();
            if (GameUI.Instance != null) GameUI.Instance.ShowCleared();
        }
        base.Die();
    }
}
