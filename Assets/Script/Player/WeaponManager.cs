using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("武器UI设置")]
    [SerializeField] private Image primaryWeaponUI; // 主武器UI
    [SerializeField] private GameObject primaryAmmoUI; // 主武器子弹UI
    [SerializeField] private Text primaryAmmoText; // 主武器弹药文本
    [SerializeField] private Image secondaryWeaponUI; // 副武器UI
    [SerializeField] private GameObject secondaryAmmoUI; // 副武器子弹UI
    [SerializeField] private Text secondaryAmmoText; // 副武器弹药文本
    [SerializeField] private Image meleeWeaponUI; // 近战武器UI

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

    private void Awake()
    {
        // 初始化UI状态
        UpdateWeaponUI();
    }

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
        // 更新UI显示
        UpdateWeaponUI();
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
        // 更新UI显示
        UpdateWeaponUI();
        // 更新子弹UI显示
        UpdateBulletUIVisibility();
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
                    if (currentWeapon == primaryWeapon && ikManager != null) ikManager.ClearLeftHandTarget();
                    primaryWeapon = null;
                }
                break;
            case WeaponType.Secondary:
                if (secondaryWeapon != null)
                {
                    if (currentWeapon == secondaryWeapon && ikManager != null) ikManager.ClearLeftHandTarget();
                    secondaryWeapon = null;
                }
                break;
            case WeaponType.Melee:
                if (meleeWeapon != null)
                {
                    if (currentWeapon == meleeWeapon && ikManager != null) ikManager.ClearLeftHandTarget();
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

        // 更新UI显示
        UpdateWeaponUI();
        // 更新子弹UI显示
        UpdateBulletUIVisibility();
    }

    // 更新子弹UI显示
    private void UpdateBulletUIVisibility()
    {
        // 根据当前武器类型显示/隐藏子弹UI
        switch (currentWeaponType)
        {
            case WeaponType.Primary:
                primaryAmmoUI.SetActive(true);
                secondaryAmmoUI.SetActive(false);
                break;

            case WeaponType.Secondary:
                primaryAmmoUI.SetActive(false);
                secondaryAmmoUI.SetActive(true);
                break;

            case WeaponType.Melee:
                primaryAmmoUI.SetActive(false);
                secondaryAmmoUI.SetActive(false);
                break;

            default:
                primaryAmmoUI.SetActive(false);
                secondaryAmmoUI.SetActive(false);
                break;
        }
    }

    // 更新武器UI显示
    private void UpdateWeaponUI()
    {
        // 更新主武器UI
        UpdateSlotUI(WeaponType.Primary, primaryWeapon, primaryWeaponUI, primaryAmmoText);

        // 更新副武器UI
        UpdateSlotUI(WeaponType.Secondary, secondaryWeapon, secondaryWeaponUI, secondaryAmmoText);

        // 更新近战武器UI
        UpdateSlotUI(WeaponType.Melee, meleeWeapon, meleeWeaponUI, null);
    }

    // 更新单个槽位UI
    private void UpdateSlotUI(WeaponType type, WeaponBase weapon, Image uiImage, Text ammoText)
    {
        // 如果有武器
        if (weapon != null)
        {
            // 显示UI元素
            uiImage.gameObject.SetActive(true);

            // 设置武器图标（背景图片）
            Sprite icon = weapon.GetWeaponUISprite();
            if (icon != null)
            {
                uiImage.sprite = icon;
            }

            // 设置弹药显示（如果有）
            if (ammoText != null)
            {
                var (current, max) = weapon.GetAmmoStatus();
                ammoText.text = $"{current}/{max}";
                ammoText.gameObject.SetActive(true);
            }

            // 设置透明度（无渐变效果）
            bool isSelected = currentWeaponType == type;
            SetSlotTransparency(uiImage, ammoText, isSelected);
        }
        else
        {
            // 没有武器时隐藏UI元素
            uiImage.gameObject.SetActive(false);
            if (ammoText != null) ammoText.gameObject.SetActive(false);
        }
    }

    // 设置槽位透明度（无渐变）
    private void SetSlotTransparency(Image uiImage, Text ammoText, bool isSelected)
    {
        Color targetColor = isSelected ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 185f / 255f);

        // 设置UI图片透明度
        uiImage.color = targetColor;

        // 设置弹药文本透明度
        if (ammoText != null)
        {
            ammoText.color = targetColor;
        }
    }

    // 更新武器逻辑
    public void UpdateWeapon(bool fireInput, bool reloadInput)
    {
        if (currentWeapon == null) return;

        // 装弹处理
        if (reloadInput && CanReload())
        {
            currentWeapon.StartReload();
            // 装弹后更新UI
            UpdateAmmoUI();
        }

        // 武器更新
        currentWeapon.UpdateWeapon(fireInput);

        // 射击后更新弹药显示
        if (fireInput)
        {
            UpdateAmmoUI();
        }
    }

    // 只更新弹药UI（优化性能）
    private void UpdateAmmoUI()
    {
        // 更新主武器弹药
        if (primaryWeapon != null && primaryAmmoText != null && primaryAmmoText.gameObject.activeSelf)
        {
            var (current, max) = primaryWeapon.GetAmmoStatus();
            primaryAmmoText.text = $"{current}/{max}";
        }

        // 更新副武器弹药
        if (secondaryWeapon != null && secondaryAmmoText != null && secondaryAmmoText.gameObject.activeSelf)
        {
            var (current, max) = secondaryWeapon.GetAmmoStatus();
            secondaryAmmoText.text = $"{current}/{max}";
        }
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