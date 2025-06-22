using System.Collections;
using UnityEngine;

// 武器管理系统
public class WeaponManager : MonoBehaviour
{
    [Header("组件管理器")]
    [SerializeField] private WeaponBoneHandle boneHandleManager; // 武器骨骼挂载处理器
    [SerializeField] private IKManager ikManager; // IK管理器
    [SerializeField] private Animator playerAnimator; // 玩家动画控制器

    [Header("当前装备")]
    public WeaponBase currentWeapon; // 当前装备的武器
    public WeaponType currentWeaponType = WeaponType.Empty; // 当前武器类型

    [Header("动画设置")]
    private readonly int isHoldEmptyHash = Animator.StringToHash("IsHoldEmpty"); // 空手 TODO
    private readonly int isHoldPrimaryHash = Animator.StringToHash("IsHoldPrimary"); // 持主武器
    private readonly int isHoldSecondaryHash = Animator.StringToHash("IsHoldSecondary"); // 持副武器
    private readonly int isHoldKnifeHash = Animator.StringToHash("IsHoldKnife"); // 持刀

    // 当前装备的武器
    private WeaponBase primaryWeapon;  // 主武器槽位
    private WeaponBase secondaryWeapon; // 副武器槽位
    private WeaponBase meleeWeapon; // 近战武器槽位

    // 武器切换参数
    private float lastSwitchTime; // 上次切换武器的时间
    private const float switchCooldown = 0.1f; // 切换冷却时间

    // 添加武器到指定槽位
    public WeaponBase AddWeapon(WeaponBase newWeapon)
    {
        // 清理武器名称
        newWeapon.gameObject.name = newWeapon.GetCleanWeaponName();
        // 存放被替换的武器
        WeaponBase replacedWeapon = null;

        switch (newWeapon.GetWeaponType())
        {
            case WeaponType.Primary:
                // 设置新武器的装备状态
                if (primaryWeapon != null)
                {
                    primaryWeapon.SetEquipped(false);
                }
                replacedWeapon = primaryWeapon;
                primaryWeapon = newWeapon;
                boneHandleManager.MountWeapon(primaryWeapon);
                if (currentWeaponType == WeaponType.Primary)
                {
                    primaryWeapon.SetEquipped(true);
                    currentWeapon = primaryWeapon;
                }
                else primaryWeapon.SetEquipped(false);
                break;

            case WeaponType.Secondary:
                // 设置新武器的装备状态
                if (secondaryWeapon != null)
                {
                    secondaryWeapon.SetEquipped(false);
                }
                replacedWeapon = secondaryWeapon;
                secondaryWeapon = newWeapon;
                boneHandleManager.MountWeapon(secondaryWeapon);
                if (currentWeaponType == WeaponType.Secondary)
                {
                    secondaryWeapon.SetEquipped(true);
                    currentWeapon = secondaryWeapon;
                }
                else secondaryWeapon.SetEquipped(false);
                break;

            case WeaponType.Melee:
                // 设置新武器的装备状态
                if (meleeWeapon != null)
                {
                    meleeWeapon.SetEquipped(false);
                }
                replacedWeapon = meleeWeapon;
                meleeWeapon = newWeapon;
                boneHandleManager.MountWeapon(meleeWeapon);
                if (currentWeaponType == WeaponType.Melee)
                {
                    meleeWeapon.SetEquipped(true);
                    currentWeapon = meleeWeapon;
                }
                else meleeWeapon.SetEquipped(false);
                break;
        }

        // 如果是第一把武器，自动装备
        if (currentWeapon == null)
        {
            EquipWeapon(newWeapon.GetWeaponType());
        }

        Debug.Log($"装备武器: {newWeapon.GetWeaponName()}");
        return replacedWeapon;
    }

    // 装备指定类型的武器
    public void EquipWeapon(WeaponType type)
    {
        // 冷却检查
        if (Time.time - lastSwitchTime < switchCooldown || currentWeaponType == type) return;
        lastSwitchTime = Time.time;

        // 隐藏当前武器
        if (currentWeapon != null)
        {
            currentWeapon.SetEquipped(false);
            currentWeapon.gameObject.SetActive(false);
        }

        // 装备新武器
        switch (type)
        {
            case WeaponType.Primary:
                if (primaryWeapon != null)
                {
                    currentWeapon = primaryWeapon;
                    currentWeaponType = WeaponType.Primary;
                }
                break;

            case WeaponType.Secondary:
                if (secondaryWeapon != null)
                {
                    currentWeapon = secondaryWeapon;
                    currentWeaponType = WeaponType.Secondary;
                }
                break;

            case WeaponType.Melee:
                if (meleeWeapon != null)
                {
                    currentWeapon = meleeWeapon;
                    currentWeaponType = WeaponType.Melee;
                }
                break;
            
        }
        if (currentWeapon != null)
        {
            currentWeapon.SetEquipped(true);
            currentWeapon.gameObject.SetActive(true);
            boneHandleManager.MountWeapon(currentWeapon); // 确保挂载到骨骼
            StartCoroutine(DelayedIKRefresh()); // 强制刷新IK（解决可能的延迟问题）
            UpdateHoldAnimationParameters();
        }
        else
        {
            currentWeaponType = WeaponType.Empty;
            Debug.LogWarning($"尝试装备武器失败: {type}");
        }
    }

    private IEnumerator DelayedIKRefresh()
    {
        yield return null; // 等待一帧
        if (ikManager != null)
        {
            ikManager.RefreshIKConstraint();
        }
    }

    private void UpdateHoldAnimationParameters()
    {
        playerAnimator.SetBool(isHoldEmptyHash,
            currentWeaponType != WeaponType.Primary &&
            currentWeaponType != WeaponType.Secondary &&
            currentWeaponType != WeaponType.Melee 
            );
        playerAnimator.SetBool(isHoldPrimaryHash, currentWeaponType == WeaponType.Primary);
        playerAnimator.SetBool(isHoldSecondaryHash, currentWeaponType == WeaponType.Secondary);
        playerAnimator.SetBool(isHoldKnifeHash, currentWeaponType == WeaponType.Melee);
    }

    // 移除指定类型的武器
    public void RemoveWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Primary:
                // 如果是当前武器，清理IK目标
                if (primaryWeapon != null)
                {
                    if (currentWeapon == primaryWeapon && ikManager != null)
                    {
                        ikManager.ClearLeftHandTarget();
                    }
                    primaryWeapon = null;
                }
                break;
            case WeaponType.Secondary:
                if (secondaryWeapon != null)
                {
                    if (currentWeapon == secondaryWeapon && ikManager != null)
                    {
                        ikManager.ClearLeftHandTarget();
                    }
                    secondaryWeapon = null;
                }
                break;
            case WeaponType.Melee:
                if (meleeWeapon != null)
                {
                    if (currentWeapon == meleeWeapon && ikManager != null)
                    {
                        ikManager.ClearLeftHandTarget();
                    }
                    meleeWeapon = null;
                }
                break;
        }

        // 如果移除的是当前武器，装备其他可用武器
        if (currentWeapon != null && currentWeapon.GetWeaponType() == type)
        {
            if (primaryWeapon != null) EquipWeapon(WeaponType.Primary);
            else if (secondaryWeapon != null) EquipWeapon(WeaponType.Secondary);
            else if (meleeWeapon != null) EquipWeapon(WeaponType.Melee);
            else
            {
                // 没有其他武器，设置为空手状态
                currentWeapon = null;
                currentWeaponType = WeaponType.Empty;
                UpdateHoldAnimationParameters();
            }
        }
    }

    // 更新武器逻辑
    public void UpdateWeapon(bool fireInput, bool reloadInput)
    {
        if (currentWeapon == null) return;

        // 装弹处理
        if (reloadInput && CanReload()) currentWeapon.StartReload();

        // 武器更新
        currentWeapon.UpdateWeapon(fireInput);
    }

    // 判断是否可以装弹
    private bool CanReload()
    {
        if (currentWeapon == null) return false;

        var (currentAmmo, maxAmmo) = currentWeapon.GetAmmoStatus();
        // 检查条件：有弹药空间、不在装弹中、不在射击中等
        return currentAmmo < maxAmmo &&
               !currentWeapon.IsReloading &&
               currentWeapon.CanReload();
    }

    // 获取当前武器
    public WeaponBase GetCurrentWeapon() => currentWeapon;
}