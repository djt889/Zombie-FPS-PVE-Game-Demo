using UnityEngine;
using UnityEngine.UI;

// 简单的准星点控制器（挂载在UI Canvas上）
public class SimpleCrosshair : MonoBehaviour
{
    [Header("准星设置")]
    [SerializeField] private Image crosshairDot; // 准星点UI图像
    [SerializeField] private Color defaultColor = Color.white; // 默认颜色
    [SerializeField] private Color hitColor = Color.red; // 命中颜色
    [SerializeField] private float hitIndicatorDuration = 0.1f; // 命中指示持续时间

    private void Start()
    {
        // 确保准星点初始可见
        if (crosshairDot != null)
        {
            crosshairDot.color = defaultColor;
        }
    }

    // 显示命中效果
    public void ShowHitIndicator()
    {
        if (crosshairDot != null)
        {
            crosshairDot.color = hitColor;
            Invoke(nameof(ResetCrosshair), hitIndicatorDuration);
        }
    }

    // 重置准星颜色
    private void ResetCrosshair()
    {
        if (crosshairDot != null)
        {
            crosshairDot.color = defaultColor;
        }
    }

    // 显示/隐藏准星
    public void SetCrosshairVisible(bool visible)
    {
        if (crosshairDot != null)
        {
            crosshairDot.enabled = visible;
        }
    }
}