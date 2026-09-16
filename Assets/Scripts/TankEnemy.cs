/// <summary>탱커: HP 9발 · 속도 2 · 데미지 20 · 직선 추적.</summary>
public class TankEnemy : EnemyBase
{
    protected override void ConfigureStats()
    {
        maxHp = 90;         // 10 x 9발
        moveSpeed = 2f;
        contactDamage = 20;
    }
}
