public struct DamageEventParams
{
    public int Damage;
    public Ship DamagedBy;
}

public interface IDamageable
{
    void ApplyDamage(DamageEventParams damageEvent);
}
