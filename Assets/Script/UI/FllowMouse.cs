using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FllowMouse : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动灵敏度")] 
    public float moveSensitivity = 0.2f;
    [Tooltip("平滑速度")] 
    public float smoothSpeed = 5f;
    
    [Header("视差设置")]
    private bool enableParallax = true;
    [Tooltip("视差强度 (0=无移动, 1=完全跟随)")] 
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;
    
    [Header("边界限制")]
    private bool enableBoundaries = true;
    [Tooltip("最大水平偏移")] 
    public float maxHorizontalOffset = 50f;
    [Tooltip("最大垂直偏移")] 
    public float maxVerticalOffset = 30f;
    
    [Header("效果预览")]
    [SerializeField] private Vector2 currentMousePosition;
    [SerializeField] private Vector2 calculatedOffset;
    
    private Vector2 targetPosition;
    private Vector2 initialPosition;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        targetPosition = initialPosition;
    }

    void Update()
    {
        // 获取鼠标在屏幕上的标准化位置（0-1范围）
        Vector2 mousePosNormalized = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
        
        currentMousePosition = mousePosNormalized;

        // 转换为偏移向量（中心为原点，范围-1到1）
        Vector2 mouseOffset = new Vector2(
            -(mousePosNormalized.x - 0.5f) * 2f,
            -(mousePosNormalized.y - 0.5f) * 2f
        );

        // 应用视差效果（前景移动更多，背景移动更少）
        float effectiveFactor = enableParallax ? 
            Mathf.Lerp(parallaxFactor, 1f, mouseOffset.magnitude) : 
            1f;

        // 计算目标位置
        calculatedOffset = mouseOffset * moveSensitivity * effectiveFactor;
        targetPosition = initialPosition + calculatedOffset;
        
        // 应用边界限制
        if (enableBoundaries)
        {
            targetPosition.x = Mathf.Clamp(
                targetPosition.x, 
                initialPosition.x - maxHorizontalOffset, 
                initialPosition.x + maxHorizontalOffset
            );
            
            targetPosition.y = Mathf.Clamp(
                targetPosition.y, 
                initialPosition.y - maxVerticalOffset, 
                initialPosition.y + maxVerticalOffset
            );
        }

        // 平滑移动到目标位置
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    // 重置到初始位置
    public void ResetPosition()
    {
        targetPosition = initialPosition;
    }
    
    // 在编辑器中可视化边界
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        RectTransform rt = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        
        // 计算UI元素的中心点
        Vector3 center = (corners[0] + corners[2]) / 2;
        
        // 计算边界框
        float width = maxHorizontalOffset * 2;
        float height = maxVerticalOffset * 2;
        
        // 绘制边界框
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireCube(center, new Vector3(width, height, 0));
        
        // 绘制当前位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(rt.position, 3f);
        
        // 绘制初始位置
        Gizmos.color = Color.cyan;
        Vector3 initialWorldPos = transform.parent.TransformPoint(initialPosition);
        Gizmos.DrawSphere(initialWorldPos, 3f);
    }
    #endif
}

