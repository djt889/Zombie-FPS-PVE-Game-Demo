using UnityEngine;

// 主武器类 - 挂载所有主武器预制体
public class PrimaryWeapon : WeaponBase
{
    [Header("主武器设置")]
    [SerializeField] private float bulletSpeed = 25f;  // 子弹速度
    [SerializeField] private float damage = 25f;       // 伤害值
    [SerializeField] private float fireRate = 0.1f;    // 射击速率
    [SerializeField] private float spreadAngle = 1.5f; // 子弹散布角度

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
        // 计算随机散布
        Vector3 spread = firePoint.forward;
        spread = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), firePoint.up) * spread;
        spread = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), firePoint.right) * spread;

        // 使用多类型对象池获取特定子弹
        GameObject bullet = MultiBulletPool.Instance.GetBullet(
            bulletType,
            firePoint.position,
            Quaternion.LookRotation(spread)
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
            bulletScript.SetVelocity(spread * bulletSpeed);
            bulletScript.SetDamage(damage);
        }
        else
        {
            // 回退方案：直接设置刚体速度
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb) rb.velocity = spread * bulletSpeed;
        }

        // 触发开火事件
        FireEvent();

        // 重置射击状态
        isFiring = false;
    }

    // 获取射击速率
    protected override float GetFireRate() => fireRate;
}