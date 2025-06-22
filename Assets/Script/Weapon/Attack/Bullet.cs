using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 子弹脚本 - 挂载子弹预制体
public class Bullet : MonoBehaviour
{
    [Header("子弹设置")]
    [SerializeField] private float lifetime = 5f; // 子弹存在时间
    [SerializeField] private float damage = 10f;  // 基础伤害值

    private float actualDamage; // 实际伤害值
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

    // 激活时设置自动回收
    private void OnEnable()
    {
        Invoke(nameof(ReturnToPool), lifetime);
    }

    // 禁用时取消调用
    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnToPool));
    }

    // 碰撞时处理
    private void OnCollisionEnter(Collision collision)
    {
        // 伤害处理逻辑（示例）
        // IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        // if (damageable != null)
        // {
        //     damageable.TakeDamage(actualDamage);
        // }

        ReturnToPool();
    }

    // 返回对象池
    private void ReturnToPool()
    {
        MultiBulletPool.Instance.ReturnBullet(gameObject);
    }
    void Update()
    {
        // 调试显示子弹轨迹
        Debug.DrawLine(transform.position, transform.position + rb.velocity * Time.deltaTime, Color.red);
    }
}