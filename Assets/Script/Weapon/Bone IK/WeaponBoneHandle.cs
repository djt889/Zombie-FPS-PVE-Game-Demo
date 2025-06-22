using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

// 骨骼挂载点管理器
public class WeaponBoneHandle : MonoBehaviour
{
    [Header("Handle对象")]
    [SerializeField] private Transform handle; // Handle对象

    [Header("空手挂载点")]
    [SerializeField] private Transform emptyHandIK;

    [Header("武器挂载点")]
    private Transform primaryMount;   // 主武器挂载点
    private Transform secondaryMount; // 副武器挂载点
    private Transform meleeMount;     // 近战武器挂载点

    private void Awake()
    {
        handle = transform;

        // 确保层级设置为Player
        SetMountLayers();

        // 初始化挂载点引用
        InitializeMountPoints();
    }

    // 设置挂载点层级为Player
    private void SetMountLayers()
    {
        if (handle != null)
        {
            // 设置Handle及其所有子对象层级为Player
            SetLayerRecursive(handle, LayerMask.NameToLayer("Player"));
        }
    }

    // 递归设置层级
    private void SetLayerRecursive(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
        foreach (Transform child in parent)
        {
            SetLayerRecursive(child, layer);
        }
    }

    // 初始化挂载点引用
    private void InitializeMountPoints()
    {
        if (handle != null)
        {
            primaryMount = handle.Find("Primary");
            secondaryMount = handle.Find("Secondary");
            meleeMount = handle.Find("Melee");

            // 验证挂载点
            if (primaryMount == null) Debug.LogError("未找到Primary挂载点");
            if (secondaryMount == null) Debug.LogError("未找到Secondary挂载点");
            if (meleeMount == null) Debug.LogError("未找到Melee挂载点");
        }
    }

    // 获取指定类型的挂载点
    public Transform GetMountPoint(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Primary => primaryMount,
            WeaponType.Secondary => secondaryMount,
            WeaponType.Melee => meleeMount,
            _ => null
        };
    }

    // 将武器挂载到骨骼
    public void MountWeapon(WeaponBase weapon)
    {
        Transform mountPoint = GetMountPoint(weapon.GetWeaponType());
        if (mountPoint == null) return;

        // 设置父对象
        weapon.transform.SetParent(mountPoint);

        // 重置位置和旋转
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        weapon.transform.localScale = Vector3.one;

        // 确保层级正确
        SetLayerRecursive(weapon.transform, LayerMask.NameToLayer("Player"));
    }

    // 获取当前武器的左手目标
    public Transform GetCurrentLeftHandTarget()
    {
        // 获取当前活动武器类型对应的挂载点
        Transform activeMount = GetActiveMountPoint();
        if (activeMount == null || activeMount.childCount == 0) return emptyHandIK;

        // 获取挂载点上的武器
        WeaponBase weapon = activeMount.GetChild(0).GetComponent<WeaponBase>();
        if (weapon == null) return emptyHandIK;

        // 返回武器的左手目标
        return weapon.GetLeftHandTarget();
    }

    // 获取当前活动武器类型的挂载点
    private Transform GetActiveMountPoint()
    {
        // 从武器管理器获取当前武器类型
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager == null) return null;

        return GetMountPoint(weaponManager.currentWeaponType);
    }
}