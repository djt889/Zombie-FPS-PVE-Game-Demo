using System.Collections;
using UnityEngine;
using System;

// 武器基类，定义所有武器的通用接口
public abstract class WeaponBase : MonoBehaviour
{
    [Header("通用武器设置")]
    public string weaponName; // 武器名称
    [SerializeField] protected WeaponType weaponType; // 武器类型
    [SerializeField] protected int maxAmmo = 30;      // 最大弹药量
    [SerializeField] protected float reloadTime = 2f; // 装弹时间

    [Header("射击设置")]
    [SerializeField] protected Camera playerCamera; // 玩家摄像机（必须设置）
    [SerializeField] protected float maxRange = 100f; // 最大射程

    [Header("子弹设置")]
    [SerializeField] protected string bulletType; // 子弹类型标识符

    [Header("武器模型")]
    [SerializeField] private GameObject weaponModel;  // 武器模型

    [Header("双手持握设置")]
    [SerializeField] private Transform leftHandTarget;// 左手目标点

    [Header("状态")]
    [SerializeField] protected int currentAmmo; // 当前弹药量
    protected bool isReloading; // 是否正在装弹
    protected float nextFireTime; // 下次可射击时间
    private bool isEquipped; // 是否已装备
    protected bool isFiring; // 射击状态

    // 事件系统
    public event Action OnFire; // 开火事件
    public event Action OnReloadStart; // 开始装弹事件
    public event Action OnReloadComplete; // 装弹完成事件
    public event Action OnEquip; // 装备武器事件
    public event Action OnUnequip; // 卸下武器事件

    // 初始化武器
    // 初始化方法
    protected virtual void Awake()
    {
        // 自动获取主摄像机（如果未设置）
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError($"{weaponName}: 未找到主摄像机!");
            }
        }

        // 初始化弹药量
        currentAmmo = maxAmmo;

        // 初始状态为未装备
        isEquipped = false;
    }

    // 从屏幕中心发射射线
    protected Ray GetCenterRay()
    {
        // 获取屏幕中心点
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        return playerCamera.ScreenPointToRay(screenCenter);
    }

    // 获取子弹方向（考虑散布）
    protected Vector3 GetBulletDirection(float spreadAngle = 0f)
    {
        Ray ray = GetCenterRay();
        Vector3 direction = ray.direction;

        // 添加随机散布
        if (spreadAngle > 0f)
        {
            direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-spreadAngle, spreadAngle), playerCamera.transform.up) * direction;
            direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-spreadAngle, spreadAngle), playerCamera.transform.right) * direction;
        }

        return direction;
    }

    // 获取子弹发射位置
    protected Vector3 GetBulletSpawnPosition()
    {
        Ray ray = GetCenterRay();

        // 从摄像机位置向前偏移一点，避免与玩家碰撞
        return ray.origin + ray.direction * 0.2f;
    }

    // 获取干净的武器名称（不含Clone）
    public string GetCleanWeaponName()
    {
        return gameObject.name.Replace("(Clone)", "");
    }

    // 每帧更新武器状态
    public virtual void UpdateWeapon(bool fireInput)
    {
        // 如果未装备或正在装弹则直接返回
        if (!isEquipped || isReloading) return;

        // 处理自动装弹逻辑
        HandleAutoReload();
        // 处理射击逻辑
        HandleFiring(fireInput);
    }

    // 自动装弹检测
    private void HandleAutoReload()
    {
        // 如果弹药为空且不在装弹状态
        if (currentAmmo <= 0 && !isReloading)
        {
            StartReload(); // 开始装弹
        }
    }

    // 射击处理
    private void HandleFiring(bool fireInput)
    {
        // 如果满足射击条件：有输入、冷却结束、有弹药
        if (fireInput && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Fire(); // 调用抽象射击方法
            nextFireTime = Time.time + GetFireRate(); // 重置冷却时间
        }
    }

    // 装备/卸下武器
    public virtual void SetEquipped(bool state)
    {
        isEquipped = state; // 更新装备状态
        gameObject.SetActive(state); // 设置激活状态

        // 触发相应事件
        if (state) OnEquip?.Invoke();
        else OnUnequip?.Invoke();
    }

    // 开始装弹
    public virtual bool StartReload()
    {
        // 如果正在装弹或弹药已满则返回false
        if (isReloading || currentAmmo >= maxAmmo)
            return false;

        isReloading = true; // 设置装弹状态
        OnReloadStart?.Invoke(); // 触发开始装弹事件
        Invoke(nameof(FinishReload), reloadTime); // 延时调用完成装弹
        return true;
    }

    // 完成装弹
    protected virtual void FinishReload()
    {
        currentAmmo = maxAmmo; // 补满弹药
        isReloading = false; // 重置装弹状态
        OnReloadComplete?.Invoke(); // 触发装弹完成事件
    }

    // 添加弹药
    public void AddAmmo(int amount)
    {
        // 确保不超过最大弹药量
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
    }

    // 获取武器模型
    public GameObject GetWeaponModel()
    {
        if (weaponModel == null)
        {
            // 如果没有指定模型，使用自身
            return gameObject;
        }
        return weaponModel;
    }
    // 触发开火事件
    protected virtual void FireEvent() => OnFire?.Invoke();

    // 获取武器名称
    public string GetWeaponName() => weaponName;

    // 获取武器类型
    public WeaponType GetWeaponType() => weaponType;

    // 武器射击逻辑（由子类实现）
    protected abstract void Fire();

    // 获取射击速率（由子类实现）
    protected abstract float GetFireRate();

    // 获取当前弹药状态
    public (int current, int max) GetAmmoStatus() => (currentAmmo, maxAmmo);
    
    // 是否正在装弹
    public bool IsReloading => isReloading;

    //是否可以装弹
    public virtual bool CanReload() => currentAmmo < maxAmmo;

    // 获取子弹类型
    public string GetBulletType() => bulletType;

    // 获取左手目标点
    public Transform GetLeftHandTarget() => leftHandTarget;
}