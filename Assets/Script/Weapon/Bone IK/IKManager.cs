using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// IK 管理器
public class IKManager : MonoBehaviour
{
    [Header("IK 组件")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK; // 左手IK约束
    [SerializeField] private RigBuilder rigBuilder; // Rig构建器

    [Header("挂载点引用")]
    [SerializeField] private WeaponBoneHandle boneHandleManager; // 武器骨骼管理器

    [SerializeField] private Transform currentLeftHandTarget; // 当前左手目标
    private Transform lastLeftHandTarget; // 上一帧的左手目标
    private Coroutine refreshRoutine;        // 刷新协程引用

    private void Start()
    {
        // 初始禁用IK
        if (leftHandIK != null)
        {
            leftHandIK.weight = 0;
        }
    }

    private void Update()
    {
        // 确保每帧更新左手目标
        UpdateLeftHandTarget();
    }

    // 定期更新左手目标
    private void UpdateLeftHandTarget()
    {
        if (leftHandIK == null || boneHandleManager == null) return;

        // 从骨骼管理器获取当前左手目标
        Transform newTarget = boneHandleManager.GetCurrentLeftHandTarget();

        // 检查目标是否变化
        if (newTarget != currentLeftHandTarget && newTarget != null)
        {
            currentLeftHandTarget = newTarget;

            if (currentLeftHandTarget != null)
            {
                // 更新IK目标
                leftHandIK.data.target = currentLeftHandTarget;

                // 启动刷新协程
                StartRefreshIK();
            }
        }

        // 检查手动拖拽变化（编辑器调试用）
        if (leftHandIK.data.target != lastLeftHandTarget)
        {
            currentLeftHandTarget = leftHandIK.data.target;
            StartRefreshIK();
        }

        // 更新最后记录的目标
        lastLeftHandTarget = leftHandIK.data.target;

        // 权重过渡
        float targetWeight = currentLeftHandTarget != null ? 1f : 0f;
        leftHandIK.weight = Mathf.MoveTowards(leftHandIK.weight, targetWeight, 10f * Time.deltaTime);
    }


    public void ClearLeftHandTarget()
    {
        if (leftHandIK != null)
        {
            // 清除目标并重置权重
            leftHandIK.data.target = null;
            currentLeftHandTarget = null;
            leftHandIK.weight = 0f;
        }
    }

    // 当动画事件触发时调用
    public void ActivateFullIK()
    {
        if (leftHandIK != null)
        {
            leftHandIK.weight = 1f;
        }
    }

    // 启动IK刷新（防止重复刷新）
    private void StartRefreshIK()
    {
        // 如果已有刷新在进行，先停止
        if (refreshRoutine != null)
        {
            StopCoroutine(refreshRoutine);
        }
        refreshRoutine = StartCoroutine(RefreshIKConstraint());
    }

    // 刷新IK
    public IEnumerator RefreshIKConstraint()
    {
        if (leftHandIK == null) yield break;

        Debug.Log("开始刷新IK约束...");

        // 1. 重置权重强制刷新
        float originalWeight = leftHandIK.weight;
        leftHandIK.weight = 0f;

        // 2. 等待一帧确保动画系统更新
        yield return null;

        // 3. 通过Rig Builder重建
        if (rigBuilder != null)
        {
            rigBuilder.Build();
            yield return null; // 额外等待一帧
        }
        else
        {
            Debug.LogWarning("未找到Rig Builder，尝试直接刷新约束");
            // 禁用再启用约束
            leftHandIK.enabled = false;
            yield return null;
            leftHandIK.enabled = true;
            yield return null;
        }

        // 4. 验证骨骼链
        if (!leftHandIK.IsValid())
        {
            Debug.LogError("IK约束无效! 请检查骨骼链设置", this);
            Debug.Log($"Root: {leftHandIK.data.root?.name ?? "null"}");
            Debug.Log($"Mid: {leftHandIK.data.mid?.name ?? "null"}");
            Debug.Log($"Tip: {leftHandIK.data.tip?.name ?? "null"}");
        }

        // 5. 恢复权重
        leftHandIK.weight = originalWeight;

        Debug.Log("IK约束刷新完成");
    }
}