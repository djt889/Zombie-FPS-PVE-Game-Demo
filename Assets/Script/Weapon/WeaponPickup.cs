using UnityEngine;


// 武器拾取器
public class WeaponPickup : MonoBehaviour
{
    [Header("武器设置")]
    [SerializeField] private WeaponBase weaponPrefab; // 武器预制体
    [SerializeField] private string weaponName = "武器"; // 武器名称

    [Header("视觉效果")]
    [SerializeField] private float rotationSpeed = 45f; // 旋转速度
    [SerializeField] private float floatHeight = 0.05f; // 浮动高度
    [SerializeField] private float floatSpeed = 1f; // 浮动速度

    private Vector3 startPosition; // 初始位置
    private bool isInScene; // 是否在场景中

    private void Start()
    {
        // 保存初始位置
        startPosition = transform.position;

        // 检查是否在SceneWeapon容器中
        CheckSceneStatus();
    }

    private void Update()
    {
        // 只有在场景容器中时才执行旋转和浮动
        if (isInScene)
        {
            // 旋转效果
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // 浮动效果
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    // 检查武器是否在场景容器中
    private void CheckSceneStatus()
    {
        isInScene = transform.parent != null &&
                    transform.parent.name == "SceneWeapon";
    }

    // 初始化武器拾取器（用于丢弃的武器）
    public void Initialize(WeaponBase weapon)
    {
        weaponPrefab = weapon;
        weaponName = weapon.GetWeaponName();

        // 创建武器模型
        GameObject weaponModel = Instantiate(
            weapon.GetWeaponModel(),
            transform.position,
            Quaternion.identity,
            transform
        );
        weaponModel.name = weaponModel.name.Replace("(Clone)", "");


        // 添加到场景容器
        SceneWeaponManager.Instance.AddWeaponToScene(transform);

        // 更新状态
        isInScene = true;
    }

    // 获取武器预制体
    public WeaponBase GetWeaponPrefab() => weaponPrefab;

    // 获取武器名称
    public string GetWeaponName() => weaponName;
}