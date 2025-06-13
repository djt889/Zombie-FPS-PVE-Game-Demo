using UnityEngine;

// 场景武器管理器
public class SceneWeaponManager : MonoBehaviour
{
    public static SceneWeaponManager Instance;

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
    }

    // 将武器添加到场景容器
    public void AddWeaponToScene(Transform weapon)
    {
        weapon.SetParent(transform);

        // 随机旋转角度（使丢弃的武器看起来自然）
        float randomYRotation = Random.Range(0f, 360f);
        weapon.rotation = Quaternion.Euler(0, randomYRotation, 0);
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