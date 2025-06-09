using UnityEngine;

// 近战武器类（刀、棍等）
public class MeleeWeapon : WeaponBase
{
    [Header("近战武器设置")]
    [SerializeField] private float attackRange = 1.5f;    // 攻击范围
    [SerializeField] private float attackDamage = 50f;    // 攻击伤害
    [SerializeField] private float attackCooldown = 0.8f; // 攻击冷却
    [SerializeField] private LayerMask attackLayers;      // 可攻击层

    private float lastAttackTime;

    protected override void Fire()
    {
        // 冷却检查
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // 执行攻击检测
        PerformMeleeAttack();
    }

    protected override float GetFireRate() => attackCooldown;

    // 近战攻击检测
    private void PerformMeleeAttack()
    {
        // 射线检测攻击范围内敌人
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, attackRange, attackLayers))
        {
            // 伤害敌人
            //IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    damageable.TakeDamage(attackDamage);
            //}
        }
    }
}