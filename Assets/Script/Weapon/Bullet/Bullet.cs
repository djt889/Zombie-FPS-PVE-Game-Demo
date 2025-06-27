using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子弹设置")]
    [SerializeField] private float lifetime = 5f; // 子弹存在时间
    [SerializeField] private float baseDamage = 10f;  // 基础伤害值

    private float actualDamage; // 实际伤害值
    private float maxRange = 100f; // 最大射程
    private float traveledDistance; // 已飞行距离
    private Vector3 startPosition; // 起始位置
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // 默认不使用重力
        }
    }

    private void OnEnable()
    {
        // 重置状态
        traveledDistance = 0f;
        startPosition = transform.position;

        // 激活时设置自动回收
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void Update()
    {
        // 计算已飞行距离
        traveledDistance = Vector3.Distance(startPosition, transform.position);

        // 超过最大射程自动回收
        if (traveledDistance > maxRange)
        {
            ReturnToPool();
        }
    }

    // 设置子弹速度
    public void SetVelocity(Vector3 velocity)
    {
        if (rb) rb.velocity = velocity;
    }

    // 设置子弹伤害
    public void SetDamage(float damage)
    {
        actualDamage = damage;
    }

    // 设置最大射程
    public void SetMaxRange(float range)
    {
        maxRange = range;
    }

    private void OnDisable()
    {
        // 禁用时取消调用
        CancelInvoke(nameof(ReturnToPool));
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 伤害处理逻辑
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(actualDamage);
        }

        // 触发准星命中效果
        SimpleCrosshair crosshair = FindObjectOfType<SimpleCrosshair>();
        if (crosshair != null)
        {
            crosshair.ShowHitIndicator();
        }

        ReturnToPool();
    }

    // 返回对象池
    private void ReturnToPool()
    {
        MultiBulletPool.Instance.ReturnBullet(gameObject);
    }
}