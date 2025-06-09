using UnityEngine;

// 武器基类，定义所有武器的通用接口
public abstract class WeaponBase : MonoBehaviour
{
    [Header("通用武器设置")]
    public string weaponName; // 武器名称
    [SerializeField] protected WeaponType weaponType; // 武器类型
    [SerializeField] protected Transform firePoint;   // 射击点
    [SerializeField] protected int maxAmmo = 30;      // 最大弹药量
    [SerializeField] protected int currentAmmo;       // 当前弹药量
    [SerializeField] protected float reloadTime = 2f; // 装弹时间

    protected bool isReloading;                       // 装弹状态
    protected float nextFireTime;                     // 下次可射击时间

    // 初始化武器
    public virtual void Initialize(Animator animator)
    {
        currentAmmo = maxAmmo;
    }

    // 武器主逻辑更新
    public virtual void UpdateWeapon(bool fireInput)
    {
        // 装弹中不处理射击
        if (isReloading) return;

        // 自动装弹
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        // 射击处理
        if (fireInput && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + GetFireRate();
        }
    }

    // 武器射击逻辑（由子类实现）
    protected abstract void Fire();

    // 获取射击速率（由子类实现）
    protected abstract float GetFireRate();

    // 开始装弹
    public virtual void StartReload()
    {
        if (isReloading || currentAmmo >= maxAmmo) return;

        isReloading = true;
        Invoke(nameof(FinishReload), reloadTime);
    }

    // 完成装弹
    protected virtual void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    // 获取当前弹药状态
    public (int, int) GetAmmoStatus() => (currentAmmo, maxAmmo);

    // 获取武器类型
    public WeaponType GetWeaponType() => weaponType;

    // 获取武器名称
    public string GetWeaponName() => weaponName;

}