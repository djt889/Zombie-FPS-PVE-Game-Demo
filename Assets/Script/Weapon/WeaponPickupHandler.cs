using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 武器拾取控制器 - 处理射线检测和拾取逻辑
// 挂载对象：玩家角色
// 所需组件：Camera (主摄像机)
public class WeaponPickupHandler : MonoBehaviour
{
    [Header("拾取设置")]
    [SerializeField] private float pickupDistance = 3f; // 拾取距离
    [SerializeField] private LayerMask pickupLayer; // 可拾取层

    [Header("UI提示")]
    [SerializeField] private GameObject pickupPrompt; // 拾取提示UI

    private Camera playerCamera; // 玩家摄像机
    private InputHandler input; // 输入处理器
    private WeaponManager weaponManager; // 武器管理器

    // 当前瞄准的武器
    private WeaponPickup currentTargetWeapon;

    private void Start()
    {
        playerCamera = Camera.main;
        input = GetComponent<InputHandler>();
        weaponManager = GetComponent<WeaponManager>();

        // 确保拾取提示初始为禁用状态
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // 每帧检测拾取目标
        CheckForPickupTarget();

        // 处理拾取输入
        if (input.PickupTriggered && currentTargetWeapon != null)
        {
            AttemptPickup();
        }
    }

    // 检测可拾取武器
    private void CheckForPickupTarget()
    {
        // 创建射线（从摄像机中央射出）
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // 重置当前目标
        currentTargetWeapon = null;

        // 显示/隐藏UI提示
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }

        // 射线检测
        if (Physics.Raycast(ray, out hit, pickupDistance, pickupLayer))
        {
            // 检查是否击中武器拾取器
            WeaponPickup weaponPickup = hit.collider.GetComponent<WeaponPickup>();
            if (weaponPickup != null)
            {
                currentTargetWeapon = weaponPickup;

                // 显示UI提示
                if (pickupPrompt != null)
                {
                    pickupPrompt.SetActive(true);
                }

                // 高亮显示武器
                weaponPickup.Highlight(true);
                return;
            }
        }

        // 没有检测到武器时取消高亮
        if (currentTargetWeapon != null)
        {
            currentTargetWeapon.Highlight(false);
            currentTargetWeapon = null;
        }
    }

    // 尝试拾取武器
    private void AttemptPickup()
    {
        if (currentTargetWeapon != null)
        {
            // 获取武器预制体
            WeaponBase weaponPrefab = currentTargetWeapon.GetWeaponPrefab();
            if (weaponPrefab != null)
            {
                // 创建新武器实例
                WeaponBase newWeapon = Instantiate(weaponPrefab);

                // 添加到武器管理器
                weaponManager.AddWeapon(newWeapon);

                // 销毁场景中的武器对象
                Destroy(currentTargetWeapon.gameObject);

                // 隐藏UI提示
                if (pickupPrompt != null)
                {
                    pickupPrompt.SetActive(false);
                }
                // 重置当前目标
                currentTargetWeapon = null;
            }
        }
    }

    // 调试可视化
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 rayEnd = playerCamera.transform.position + playerCamera.transform.forward * pickupDistance;
            Gizmos.DrawLine(playerCamera.transform.position, rayEnd);
        }
    }
}