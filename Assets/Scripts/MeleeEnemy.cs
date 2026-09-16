/// <summary>근접: HP 3발 · 속도 3.5 · 데미지 10 · 직선 추적.</summary>
public class MeleeEnemy : EnemyBase
{
    protected override void ConfigureStats()
    {
        maxHp = 30;         // 플레이어 투사체 10 x 3발
        moveSpeed = 3.5f;
        contactDamage = 10;
    }
}
