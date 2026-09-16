using UnityEngine;

/// <summary>
/// 플레이어: WASD 이동(5유닛/초) · 마우스 꾹 누르기 자동 연사 · HP 100 · 무적 0.5초.
/// 대시·구르기·탄약 제한 없음 (변수 추가 금지).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public const int MaxHP = 100;
    public int HP { get; private set; } = MaxHP;
    public bool IsDead { get; private set; }

    const float MoveSpeed = 5f;
    const float ProjectileSpeed = 15f;
    const float ProjectileRange = 8f;    // 화면 절반쯤에서 소멸
    const int ProjectileDamage = 10;
    const float InvulnSeconds = 0.5f;

    float fireInterval = 0.5f;           // 무기 획득 시 0.3
    float nextFireTime;
    float invulnUntil;

    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (GameUI.Instance != null) GameUI.Instance.SetHP(HP, MaxHP);
    }

    void Update()
    {
        if (IsDead || Time.timeScale == 0f) return;

        // 이동 (모두 물리 속도 기반 → 프레임 독립)
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.velocity = input.normalized * MoveSpeed;

        // 마우스 꾹 누르기 자동 연사
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && Camera.main != null)
        {
            nextFireTime = Time.time + fireInterval;
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = ((Vector2)mouse - rb.position).normalized;
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;
            Projectile.Spawn(true, rb.position + dir * 0.7f, dir,
                ProjectileSpeed, ProjectileDamage, ProjectileRange,
                new Color(1f, 0.95f, 0.4f));
        }

        // 무적 시간 동안 깜빡임
        if (sr != null)
        {
            bool invuln = Time.time < invulnUntil;
            var c = sr.color;
            c.a = invuln && Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 0.35f : 1f;
            sr.color = c;
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || Time.time < invulnUntil) return;
        invulnUntil = Time.time + InvulnSeconds;
        HP = Mathf.Max(0, HP - damage);
        if (GameUI.Instance != null) GameUI.Instance.SetHP(HP, MaxHP);
        if (HP <= 0) Die();
    }

    public void Heal(int amount)
    {
        HP = Mathf.Min(MaxHP, HP + amount);
        if (GameUI.Instance != null) GameUI.Instance.SetHP(HP, MaxHP);
    }

    public void UpgradeWeapon() => fireInterval = 0.3f;

    void Die()
    {
        IsDead = true;
        rb.velocity = Vector2.zero;
        if (GameLogger.Instance != null) GameLogger.Instance.RecordDeath(transform.position);
        DeathScreen.Show();
    }

    /// <summary>사망 화면의 "다시 시도" — 현재 방 입구에서 부활, 그 방의 적 전원 리셋.</summary>
    public void Respawn()
    {
        if (RoomController.All.TryGetValue(RoomController.CurrentRoomId, out var room))
        {
            transform.position = room.entryPoint;
            room.ResetForRetry();
        }
        HP = MaxHP;
        IsDead = false;
        invulnUntil = Time.time + 1f;   // 부활 직후 잠깐 무적
        if (GameUI.Instance != null) GameUI.Instance.SetHP(HP, MaxHP);
    }
}
