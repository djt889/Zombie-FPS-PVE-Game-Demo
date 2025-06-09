using UnityEngine;


// 武器拾取器 - 定义可拾取的武器属性
// 挂载对象：场景中的武器道具
// 所需组件：Collider (设置为触发器)
public class WeaponPickup : MonoBehaviour
{
    [Header("武器设置")]
    [SerializeField] private WeaponBase weaponPrefab; // 武器预制体
    [SerializeField] private string weaponName = "武器"; // 武器名称

    [Header("视觉效果")]
    [SerializeField] private float rotationSpeed = 45f; // 旋转速度
    [SerializeField] private float floatHeight = 0.2f; // 浮动高度
    [SerializeField] private float floatSpeed = 1f; // 浮动速度

    private Vector3 startPosition; // 初始位置
    private Material originalMaterial; // 原始材质
    private Material highlightMaterial; // 高亮材质

    private void Start()
    {
        // 保存初始位置
        startPosition = transform.position;

        // 获取原始材质
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;

            // 创建高亮材质
            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = Color.yellow;
            highlightMaterial.EnableKeyword("_EMISSION");
            highlightMaterial.SetColor("_EmissionColor", Color.yellow * 0.5f);
        }
    }

    private void Update()
    {
        // 旋转效果
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 浮动效果
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // 高亮显示武器
    public void Highlight(bool highlight)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            renderer.material = highlight ? highlightMaterial : originalMaterial;
        }
    }

    // 获取武器预制体
    public WeaponBase GetWeaponPrefab() => weaponPrefab;

    // 获取武器名称
    public string GetWeaponName() => weaponName;
}