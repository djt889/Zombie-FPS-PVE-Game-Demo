using UnityEngine;

// 武器管理系统
public class WeaponManager : MonoBehaviour
{
    [Header("武器挂载点")]
    [SerializeField] private Transform primarySlot;    // 主武器挂载点
    [SerializeField] private Transform secondarySlot;  // 副武器挂载点
    [SerializeField] private Transform meleeSlot;      // 近战武器挂载点

    [Header("当前装备")]
    public WeaponBase currentWeapon; // 当前装备的武器
    public WeaponType currentWeaponType = WeaponType.Primary; // 当前武器类型

    // 当前装备的武器
    private WeaponBase primaryWeapon;
    private WeaponBase secondaryWeapon;
    private WeaponBase meleeWeapon;

    // 武器切换参数
    private float lastSwitchTime;
    private const float switchCooldown = 0.1f; // 切换冷却时间

    // 添加武器到指定槽位
    public WeaponBase AddWeapon(WeaponBase newWeapon)
    {
        // 清理武器名称
        newWeapon.gameObject.name = newWeapon.GetCleanWeaponName();

        WeaponBase replacedWeapon = null;

        switch (newWeapon.GetWeaponType())
        {
            case WeaponType.Primary:
                // 保存被替换的武器
                replacedWeapon = primaryWeapon;

                // 设置新武器
                primaryWeapon = newWeapon;
                primaryWeapon.transform.SetParent(primarySlot, false);
                primaryWeapon.transform.localPosition = Vector3.zero;
                primaryWeapon.transform.localRotation = Quaternion.identity;

                // 自动装备
                EquipWeapon(WeaponType.Primary);
                break;

            case WeaponType.Secondary:
                replacedWeapon = secondaryWeapon;

                secondaryWeapon = newWeapon;
                secondaryWeapon.transform.SetParent(secondarySlot, false);
                secondaryWeapon.transform.localPosition = Vector3.zero;
                secondaryWeapon.transform.localRotation = Quaternion.identity;
                break;

            case WeaponType.Melee:
                replacedWeapon = meleeWeapon;

                meleeWeapon = newWeapon;
                meleeWeapon.transform.SetParent(meleeSlot, false);
                meleeWeapon.transform.localPosition = Vector3.zero;
                meleeWeapon.transform.localRotation = Quaternion.identity;
                break;
        }

        Debug.Log($"装备武器: {newWeapon.GetWeaponName()}");

        return replacedWeapon;
    }

    // 装备指定类型的武器
    public void EquipWeapon(WeaponType type)
    {
        // 冷却检查
        if (Time.time - lastSwitchTime < switchCooldown) return;
        lastSwitchTime = Time.time;

        // 隐藏所有武器
        if (primaryWeapon != null) primaryWeapon.gameObject.SetActive(false);
        if (secondaryWeapon != null) secondaryWeapon.gameObject.SetActive(false);
        if (meleeWeapon != null) meleeWeapon.gameObject.SetActive(false);

        // 装备新武器
        switch (type)
        {
            case WeaponType.Primary:
                if (primaryWeapon != null)
                {
                    currentWeapon = primaryWeapon;
                    primaryWeapon.gameObject.SetActive(true);
                    currentWeaponType = WeaponType.Primary;
                }
                break;

            case WeaponType.Secondary:
                if (secondaryWeapon != null)
                {
                    currentWeapon = secondaryWeapon;
                    secondaryWeapon.gameObject.SetActive(true);
                    currentWeaponType = WeaponType.Secondary;
                }
                break;

            case WeaponType.Melee:
                if (meleeWeapon != null)
                {
                    currentWeapon = meleeWeapon;
                    meleeWeapon.gameObject.SetActive(true);
                    currentWeaponType = WeaponType.Melee;
                }
                break;
        }
    }

    // 移除指定类型的武器
    public void RemoveWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Primary:
                primaryWeapon = null;
                break;
            case WeaponType.Secondary:
                secondaryWeapon = null;
                break;
            case WeaponType.Melee:
                meleeWeapon = null;
                break;
        }

        // 如果移除的是当前武器，装备其他可用武器
        if (currentWeapon != null && currentWeapon.GetWeaponType() == type)
        {
            if (primaryWeapon != null) EquipWeapon(WeaponType.Primary);
            else if (secondaryWeapon != null) EquipWeapon(WeaponType.Secondary);
            else if (meleeWeapon != null) EquipWeapon(WeaponType.Melee);
            else currentWeapon = null;
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
        }

        // 武器更新
        currentWeapon.UpdateWeapon(fireInput);
    }

    // 判断是否可以装弹
    private bool CanReload()
    {
        if (currentWeapon == null) return false;

        var ammo = currentWeapon.GetAmmoStatus();
        return ammo.Item1 < ammo.Item2;
    }

    // 获取当前武器
    public WeaponBase GetCurrentWeapon() => currentWeapon;
}