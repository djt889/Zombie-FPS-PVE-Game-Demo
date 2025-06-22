using System.Collections.Generic;
using UnityEngine;

// 多类型子弹池管理器（挂载在场景中的空对象上）
public class MultiBulletPool : MonoBehaviour
{
    public static MultiBulletPool Instance; // 单例实例

    [System.Serializable]
    public class BulletPoolSetting
    {
        public string bulletType;       // 子弹类型标识符
        public GameObject prefab;       // 子弹预制体
        public int initialSize = 20;    // 初始池大小
        public int maxSize = 100;       // 最大池大小
        public bool expandable = true;  // 是否可扩展
    }

    [Header("子弹池设置")]
    public List<BulletPoolSetting> poolSettings = new List<BulletPoolSetting>();

    private Dictionary<string, Queue<GameObject>> bulletPools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, BulletPoolSetting> poolSettingsDict = new Dictionary<string, BulletPoolSetting>();

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初始化所有子弹池
    private void InitializePools()
    {
        // 将设置转换为字典以便快速查找
        foreach (var setting in poolSettings)
        {
            poolSettingsDict[setting.bulletType] = setting;

            // 初始化每个类型的池
            var queue = new Queue<GameObject>();
            for (int i = 0; i < setting.initialSize; i++)
            {
                GameObject bullet = CreateNewBullet(setting);
                queue.Enqueue(bullet);
            }
            bulletPools[setting.bulletType] = queue;
        }
    }

    // 创建新子弹
    private GameObject CreateNewBullet(BulletPoolSetting setting)
    {
        GameObject bullet = Instantiate(setting.prefab, transform);
        bullet.SetActive(false);

        // 添加子弹类型标识组件
        var typeInfo = bullet.AddComponent<BulletTypeInfo>();
        typeInfo.bulletType = setting.bulletType;

        return bullet;
    }

    // 获取子弹
    public GameObject GetBullet(string bulletType, Vector3 position, Quaternion rotation)
    {
        // 检查子弹类型是否存在
        if (!bulletPools.ContainsKey(bulletType))
        {
            Debug.LogError($"未找到子弹类型: {bulletType}");
            return null;
        }

        var pool = bulletPools[bulletType];
        var setting = poolSettingsDict[bulletType];

        GameObject bullet = null;

        // 尝试从池中取出子弹
        if (pool.Count > 0)
        {
            bullet = pool.Dequeue();
        }
        // 如果池为空且可扩展，创建新子弹
        else if (setting.expandable && pool.Count < setting.maxSize)
        {
            bullet = CreateNewBullet(setting);
        }
        else
        {
            Debug.LogWarning($"子弹池已满: {bulletType}");
            return null;
        }

        // 设置子弹位置和旋转
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.SetActive(true);

        return bullet;
    }

    // 返回子弹到池中
    public void ReturnBullet(GameObject bullet)
    {
        // 获取子弹类型信息
        var typeInfo = bullet.GetComponent<BulletTypeInfo>();
        if (typeInfo == null)
        {
            Debug.LogError("子弹缺少BulletTypeInfo组件");
            return;
        }

        string bulletType = typeInfo.bulletType;

        // 检查子弹类型是否存在
        if (!bulletPools.ContainsKey(bulletType))
        {
            Debug.LogError($"无法返回子弹: 未知类型 {bulletType}");
            return;
        }

        // 重置子弹状态
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);

        // 重置刚体速度
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 将子弹放回池中
        bulletPools[bulletType].Enqueue(bullet);
    }
}

// 子弹类型标识组件（自动添加到子弹上）
public class BulletTypeInfo : MonoBehaviour
{
    public string bulletType;
}