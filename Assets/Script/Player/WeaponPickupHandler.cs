using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 武器拾取控制器
public class WeaponPickupHandler : MonoBehaviour
{
    [Header("拾取设置")]
    [SerializeField] private float pickupDistance = 3f; // 拾取距离
    [SerializeField] private LayerMask pickupLayer; // 可拾取层
    [SerializeField] private float dropOffset = 1.5f; // 丢弃偏移距离

    [Header("UI提示")]
    [SerializeField] private GameObject pickupPrompt; // 拾取提示UI
    [SerializeField] private TMPro.TextMeshProUGUI pickupText; // 拾取文本

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
    }

    public void ProcessPickupInput()
    {
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

                // 更新文本
                if (pickupText != null)
                {
                    pickupText.text = $"按 E 拾取 {weaponPickup.GetWeaponName()}";
                }
            }
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
                // 从场景容器中移除武器
                SceneWeaponManager.Instance.RemoveWeaponFromScene(currentTargetWeapon.transform);

                // 创建新武器实例
                WeaponBase newWeapon = Instantiate(weaponPrefab);

                // 添加新武器到武器管理器，并获取被替换的旧武器
                WeaponBase replacedWeapon = weaponManager.AddWeapon(newWeapon);

                // 如果替换了旧武器，将其丢弃到场景中
                if (replacedWeapon != null)
                {
                    DropWeapon(replacedWeapon);
                }

                // 销毁场景中的武器拾取器
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
    // 尝试丢弃武器
    public void DropWeapon(WeaponBase weapon)
    {
        // 创建武器拾取器
        string cleanName = weapon.GetWeaponName().Replace("(Clone)", "");
        GameObject pickupObject = new GameObject($"{cleanName}_Pickup");

        // 添加拾取器组件
        WeaponPickup pickup = pickupObject.AddComponent<WeaponPickup>();
        pickup.Initialize(weapon);

        // 销毁当前手中丢弃的武器
        Destroy(weapon.gameObject);

        // 设置位置（角色脚下）
        Vector3 dropPosition = transform.position;
        dropPosition.y = 0.2f; // 确保在地面上
        pickupObject.transform.position = dropPosition;

        Debug.Log($"已丢弃武器: {weapon.GetWeaponName()}");
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