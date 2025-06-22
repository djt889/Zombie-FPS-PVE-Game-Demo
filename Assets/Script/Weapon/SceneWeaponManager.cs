using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

// 场景武器管理器
public class SceneWeaponManager : MonoBehaviour
{
    public static SceneWeaponManager Instance;

    [Header("SceneWeapon对象")]
    [SerializeField] private Transform sceneWeapon; // SceneWeapon对象

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        sceneWeapon = transform;

        SetMountLayers();
    }

    private void SetMountLayers()
    {
        if (sceneWeapon != null)
        {
            // 设置Handle及其所有子对象层级为Player
            SetLayerRecursive(sceneWeapon, LayerMask.NameToLayer("Weapon"));
        }
    }

    // 递归设置层级
    private void SetLayerRecursive(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
        parent.gameObject.SetActive(true);
        foreach (Transform child in parent)
        {
            SetLayerRecursive(child, layer);
        }
    }

    // 将武器添加到场景容器
    public void AddWeaponToScene(Transform weapon)
    {
        weapon.SetParent(transform);

        // 随机旋转角度（使丢弃的武器看起来自然）
        float randomYRotation = Random.Range(0f, 360f);
        weapon.rotation = Quaternion.Euler(0, randomYRotation, 0);

        // 确保层级正确
        SetLayerRecursive(weapon.transform, LayerMask.NameToLayer("Weapon"));
    }

    // 从场景容器中移除武器
    public void RemoveWeaponFromScene(Transform weapon)
    {
        if (weapon.parent == transform)
        {
            weapon.SetParent(null);
        }
    }
}