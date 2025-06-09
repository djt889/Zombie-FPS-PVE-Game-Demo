using UnityEngine;

/// <summary>
/// 武器管理系统（简化版）- 每个挂载点只有一个武器
/// 挂载对象：玩家角色
/// </summary>
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
    private const float switchCooldown = 0.3f; // 切换冷却时间

    // 添加武器到指定槽位
    public void AddWeapon(WeaponBase newWeapon)
    {
        switch (newWeapon.GetWeaponType())
        {
            case WeaponType.Primary:
                // 如果已有主武器，先销毁
                if (primaryWeapon != null)
                {
                    Destroy(primaryWeapon.gameObject);
                }

                // 设置新武器
                primaryWeapon = newWeapon;
                primaryWeapon.transform.SetParent(primarySlot, false);
                primaryWeapon.transform.localPosition = Vector3.zero;
                primaryWeapon.transform.localRotation = Quaternion.identity;

                // 自动装备
                EquipWeapon(WeaponType.Primary);
                break;

            case WeaponType.Secondary:
                if (secondaryWeapon != null)
                {
                    Destroy(secondaryWeapon.gameObject);
                }

                secondaryWeapon = newWeapon;
                secondaryWeapon.transform.SetParent(secondarySlot, false);
                secondaryWeapon.transform.localPosition = Vector3.zero;
                secondaryWeapon.transform.localRotation = Quaternion.identity;
                break;

            case WeaponType.Melee:
                if (meleeWeapon != null)
                {
                    Destroy(meleeWeapon.gameObject);
                }

                meleeWeapon = newWeapon;
                meleeWeapon.transform.SetParent(meleeSlot, false);
                meleeWeapon.transform.localPosition = Vector3.zero;
                meleeWeapon.transform.localRotation = Quaternion.identity;
                break;
        }

        Debug.Log($"装备武器: {newWeapon.GetWeaponName()}");
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