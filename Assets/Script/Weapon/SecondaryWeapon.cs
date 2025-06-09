using UnityEngine;

// 副武器类（手枪等）
public class SecondaryWeapon : WeaponBase
{
    [Header("副武器设置")]
    [SerializeField] private GameObject bulletPrefab; // 子弹预制体
    [SerializeField] private float bulletSpeed = 30f; // 子弹速度
    [SerializeField] private float damage = 35f;     // 伤害值
    [SerializeField] private float fireRate = 0.2f;  // 射击速率
    [SerializeField] private float recoilForce = 0.5f; // 后坐力

    protected override void Fire()
    {
        // 减少弹药
        currentAmmo--;

        // 创建子弹
        if (bulletPrefab && firePoint)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb) rb.velocity = firePoint.forward * bulletSpeed;

            // 设置子弹伤害
            //Bullet bulletScript = bullet.GetComponent<Bullet>();
            //if (bulletScript) bulletScript.SetDamage(damage);
        }
    }

    protected override float GetFireRate() => fireRate;
}