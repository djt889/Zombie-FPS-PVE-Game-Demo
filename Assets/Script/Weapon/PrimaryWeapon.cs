using UnityEngine;

// 主武器类（步枪、冲锋枪等）
public class PrimaryWeapon : WeaponBase
{
    [Header("主武器设置")]
    [SerializeField] private GameObject bulletPrefab;  // 子弹预制体
    [SerializeField] private float bulletSpeed = 35f;  // 子弹速度
    [SerializeField] private float damage = 25f;       // 伤害值
    [SerializeField] private float fireRate = 0.1f;    // 射击速率
    [SerializeField] private float spreadAngle = 1.5f; // 子弹散布角度

    protected override void Fire()
    {
        // 减少弹药
        currentAmmo--;

        // 创建子弹
        if (bulletPrefab && firePoint)
        {
            // 计算随机散布
            Vector3 spread = firePoint.forward;
            spread = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), firePoint.up) * spread;
            spread = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), firePoint.right) * spread;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(spread));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb) rb.velocity = spread * bulletSpeed;

            // 设置子弹伤害
            //Bullet bulletScript = bullet.GetComponent<Bullet>();
            //if (bulletScript) bulletScript.SetDamage(damage);
        }
    }

    protected override float GetFireRate() => fireRate;
}