using UnityEngine;

// 副武器类 - 挂载在所有副武器预制体
public class SecondaryWeapon : WeaponBase
{
    [Header("副武器设置")]
    [SerializeField] private float bulletSpeed = 20f; // 子弹速度
    [SerializeField] private float damage = 35f;     // 伤害值
    [SerializeField] private float fireRate = 0.2f;  // 射击速率

    protected override void Fire()
    {
        Debug.Log($"{weaponName}: 开火触发");
        // 更新射击状态
        isFiring = true;

        // 减少弹药
        currentAmmo--;

        // 确保有发射点
        if (firePoint == null)
        {
            Debug.LogError($"{weaponName} 缺少 firePoint 引用！请设置子弹发射点。");
            return;
        }

        // 使用多类型对象池获取特定子弹
        GameObject bullet = MultiBulletPool.Instance.GetBullet(
            bulletType,
            firePoint.position,
            firePoint.rotation
        );

        if (bullet == null)
        {
            Debug.LogWarning($"{weaponName}: 获取子弹失败!");
            return;
        }

        // 设置子弹属性
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript)
        {
            bulletScript.SetVelocity(firePoint.forward * bulletSpeed);
            bulletScript.SetDamage(damage);
        }
        else
        {
            // 回退方案
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb) rb.velocity = firePoint.forward * bulletSpeed;
        }

        // 触发开火事件
        FireEvent();

        // 重置射击状态
        isFiring = false;
    }

    protected override float GetFireRate() => fireRate;
}