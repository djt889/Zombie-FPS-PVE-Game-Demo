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

        // 确保摄像机已设置
        if (playerCamera == null)
        {
            Debug.LogError($"{weaponName}: 摄像机未设置!");
            return;
        }

        // 获取子弹方向和位置
        Vector3 direction = GetBulletDirection();
        Vector3 spawnPosition = GetBulletSpawnPosition();

        // 使用多类型对象池获取特定子弹
        GameObject bullet = MultiBulletPool.Instance.GetBullet(
            bulletType,
            spawnPosition,
            Quaternion.LookRotation(direction)
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
            bulletScript.SetVelocity(direction * bulletSpeed);
            bulletScript.SetDamage(damage);
            bulletScript.SetMaxRange(maxRange);
        }
        else
        {
            // 回退方案
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb) rb.velocity = direction * bulletSpeed;
        }

        // 触发开火事件
        FireEvent();

        // 重置射击状态
        isFiring = false;
    }

    protected override float GetFireRate() => fireRate;
}