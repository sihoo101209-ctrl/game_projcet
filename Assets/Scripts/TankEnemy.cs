/// <summary>탱커: HP 6발 · 속도 1 · 데미지 20 · 직선 추적.</summary>
public class TankEnemy : EnemyBase
{
    protected override void ConfigureStats()
    {
        maxHp = 60;         // 10 x 6발
        moveSpeed = 1f;
        contactDamage = 20;
    }
}
