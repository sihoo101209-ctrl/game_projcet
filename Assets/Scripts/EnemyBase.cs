using UnityEngine;

/// <summary>
/// 모든 적의 공통 기반. 서브클래스는 ConfigureStats() 로 수치를 정하고 Move() 로 행동한다.
/// 플레이어 접촉 시 데미지 (플레이어 쪽 무적 0.5초가 연타를 막아준다).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    public int maxHp = 30;
    public int hp;
    public float moveSpeed = 3.5f;
    public int contactDamage = 10;

    protected RoomController room;
    protected Rigidbody2D rb;
    EnemyHealthBar bar;

    protected Transform Player =>
        PlayerController.Instance != null && !PlayerController.Instance.IsDead
            ? PlayerController.Instance.transform : null;

    protected float DistanceToPlayer =>
        Player != null ? Vector2.Distance(rb.position, Player.position) : float.MaxValue;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ConfigureStats();
        hp = maxHp;
        if (!(this is BossController))          // 보스는 상단 고정 바 사용
            bar = EnemyHealthBar.Attach(this);
    }

    /// <summary>서브클래스에서 maxHp / moveSpeed / contactDamage 설정.</summary>
    protected virtual void ConfigureStats() { }

    public virtual void Setup(RoomController r) => room = r;

    protected virtual void FixedUpdate()
    {
        if (Player == null) { rb.velocity = Vector2.zero; return; }
        Move();
    }

    /// <summary>기본: 플레이어 직선 추적.</summary>
    protected virtual void Move() =>
        rb.velocity = ((Vector2)Player.position - rb.position).normalized * moveSpeed;

    public void TakeHit(int damage)
    {
        if (hp <= 0) return;
        hp -= damage;
        if (bar != null) bar.Flash(hp, maxHp);
        OnHit();
        if (hp <= 0) Die();
    }

    protected virtual void OnHit() { }

    protected virtual void Die()
    {
        if (room != null) room.OnEnemyDied(this);
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player") && PlayerController.Instance != null)
            PlayerController.Instance.TakeDamage(contactDamage);
    }
}
