using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("组件引用")]
    private Camera playerCamera; // 玩家摄像机
    [SerializeField] private Transform cameraRoot; // 摄像机根节点（用于下蹲时调整高度）
    private Animator playerAnimator; //主角动画状态机组件
    private CharacterController controller;        // 角色控制器组件
    private InputHandler input;                // 输入处理组件

    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;   // 基础移动速度
    [SerializeField] private float runSpeed = 8f;    // 冲刺速度
    [SerializeField] private float crouchSpeed = 2.5f; // 下蹲速度
    [SerializeField] private float aimSpeed = 3f;     // 瞄准时移动速度

    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 7f;    // 跳跃初速度
    [SerializeField] private float gravity = -9.81f;  // 重力加速度
    [SerializeField][Range(0, 1)] private float airControl = 0.7f; // 空中移动系数（0-1）

    [Header("下蹲设置")]
    [SerializeField] private float crouchHeight = 1f;     // 下蹲时角色高度
    [SerializeField] private float standHeight = 2f;      // 站立时原始高度
    [SerializeField] private float crouchTransitionSpeed = 5f; // 高度过渡速度

    [Header("瞄准设置")]
    [SerializeField] private float aimFOV = 45f; // 瞄准时的视野
    [SerializeField] private float defaultFOV = 60f; // 默认视野
    [SerializeField] private float aimTransitionSpeed = 5f; // 瞄准过渡速度

    [Header("射击设置")]
    [SerializeField] private float fireRate = 0.1f; // 射击间隔
    [SerializeField] private GameObject bulletPrefab; // 子弹预制体
    [SerializeField] private Transform firePoint; // 射击点
    [SerializeField] private float bulletSpeed = 30f; // 子弹速度
    [SerializeField] private int maxAmmo = 30; // 最大弹药量
    [SerializeField] private int currentAmmo; // 当前弹药量

    [Header("动画设置")]
    private readonly int moveXHash = Animator.StringToHash("Move_X");
    private readonly int moveYHash = Animator.StringToHash("Move_Y");
    private readonly int isAimingHash = Animator.StringToHash("IsAiming"); // 瞄准动画参数
    private readonly int isFireingHash = Animator.StringToHash("IsFireing"); // 射击动画参数
    private readonly int reloadHash = Animator.StringToHash("Reload"); // 装弹动画参数
    [SerializeField][Range(0, 1)] private float weight = 1f;
    //public GameObject RightHandGrip;
    //public GameObject LeftHandGrip;

    [Header("状态管理")]
    public PlayerState CurrentState; // 当前状态（公开可读）
    private PlayerState previousState; // 记录之前的状态
    private Vector3 verticalVelocity;    // 垂直速度（跳跃/下落）
    private float originalCameraY;       // 摄像机原始Y轴位置
    private bool isGrounded;             // 是否接触地面
    private bool isAiming; // 瞄准状态标志
    private bool isFireing; // 射击状态标志
    private float nextFireTime; // 下次可射击时间
    private float fireTimer; // 射击计时器


    private void Awake()
    {
        // 初始化组件引用
        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        controller = GetComponent<CharacterController>(); // 获取角色控制器
        playerAnimator = GetComponent<Animator>();
        input = GetComponent<InputHandler>();         // 获取输入组件
        originalCameraY = cameraRoot.localPosition.y;    // 记录初始摄像机高度
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;    // 更新地面检测状态
        HandleStateTransition();               // 处理状态转换逻辑
        HandleHorizontalMovement();            // 处理水平移动
        HandleVerticalMovement();              // 处理垂直运动
        HandleCrouch();                        // 处理下蹲状态
        HandleAiming();                        // 处理瞄准逻辑
        HandleFireing();                       // 处理射击逻辑
        AnimationParameters();                 // 处理动画参数
        input.ConsumeActions();                // 重置瞬时输入
    }

    /// 处理状态转换逻辑
    private void HandleStateTransition()
    {
        // 装弹优先
        if (input.ReloadTriggered && currentAmmo < maxAmmo && CanReload())
        {
            SetState(PlayerState.Reloading);
            return;
        }

        // 射击状态（优先级高于瞄准）
        if (input.FireTriggered && CanFire())
        {
            SetState(PlayerState.Fireing);
            return;
        }

        // 瞄准状态
        if (input.IsAiming && CanAim())
        {
            SetState(PlayerState.Aiming);
            return;
        }

        // 如果松开瞄准键，返回之前状态
        if (CurrentState == PlayerState.Aiming && !input.IsAiming)
        {
            SetState(previousState);
            return;
        }

        // 空中状态处理
        if (!isGrounded)
        {
            CurrentState = verticalVelocity.y > 0 ? PlayerState.Jumping : PlayerState.Falling;
            return;
        }

        // 下蹲判断
        if (input.IsCrouching)
        {
            SetState(PlayerState.Crouching);
            return;
        }

        // 移动状态
        if (input.MoveInput != Vector2.zero)
        {
            CurrentState = input.IsSprinting ? PlayerState.Running : PlayerState.Walking;
            return;
        }

        SetState(PlayerState.Idle);
    }

    // 处理水平移动（地面/空中）
    private void HandleHorizontalMovement()
    {
        // 根据不同状态调整移动参数
        float targetSpeed = CurrentState switch
        {
            PlayerState.Running => runSpeed,
            PlayerState.Crouching => crouchSpeed,
            PlayerState.Aiming => aimSpeed,
            PlayerState.Walking => walkSpeed,
            _ => isAiming ? aimSpeed : 0
        };

        // 应用空中控制系数
        if (!isGrounded) targetSpeed *= airControl;

        // 计算移动方向
        Vector3 moveDirection = transform.TransformDirection(new Vector3(input.MoveInput.x, 0, input.MoveInput.y));

        // 平滑移动过渡
        Vector3 targetVelocity = moveDirection * targetSpeed;
        controller.Move(targetVelocity * Time.deltaTime);
    }

    // 处理垂直运动（跳跃/重力）

    private void HandleVerticalMovement()
    {
        // 跳跃触发检测（任何状态都可触发）
        if (input.JumpTriggered && CanJump())
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            SetState(PlayerState.Jumping);
        }

        // 重力应用
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
            if (CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling)
            {
                SetState(PlayerState.Idle);
            }
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
            if (!isGrounded && verticalVelocity.y < 0)
            {
                SetState(PlayerState.Falling);
            }
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    // 下蹲
    private void HandleCrouch()
    {
        // 计算目标高度
        float targetHeight = CurrentState == PlayerState.Crouching ?
            crouchHeight :
            standHeight;

        // 平滑过渡角色高度
        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        // 同步调整摄像机高度
        Vector3 camPos = cameraRoot.localPosition;
        camPos.y = Mathf.Lerp(
            camPos.y,
            originalCameraY * (targetHeight / standHeight),
            crouchTransitionSpeed * Time.deltaTime
        );
        cameraRoot.localPosition = camPos;
    }


    // 处理瞄准逻辑
    private void HandleAiming()
    {
        // 更新瞄准状态标志
        isAiming = CurrentState == PlayerState.Aiming ||
                  (isAiming && CurrentState == PlayerState.Fireing);

        // 更新瞄准动画参数
        playerAnimator.SetBool(isAimingHash, isAiming);

        // 视野变化（瞄准时缩小视野）
        float targetFOV = isAiming ? aimFOV : defaultFOV;
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            aimTransitionSpeed * Time.deltaTime
        );
    }

    // 处理射击逻辑
    private void HandleFireing()
    {
        isFireing = CurrentState == PlayerState.Fireing;
        playerAnimator.SetBool(isFireingHash, isFireing);

        // 射击状态处理
        if (isFireing)
        {
            // 射击冷却计时
            if (Time.time >= nextFireTime && currentAmmo > 0)
            {
                FireWeapon();
                nextFireTime = Time.time + fireRate;
            }

            // 射击状态持续时间
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate * 2)
            {
                fireTimer = 0;
                SetState(isAiming ? PlayerState.Aiming : previousState);
            }
        }
    }

    // 实际射击逻辑
    private void FireWeapon()
    {
        // 减少弹药
        currentAmmo--;

        // 创建子弹
        if (bulletPrefab && firePoint)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb) rb.velocity = firePoint.forward * bulletSpeed;

            // 可选：添加枪口闪光、声音等效果
        }
    }

    //动画
    private void AnimationParameters()
    {

        Vector3 horizontalVelocity = input.MoveInput;
        horizontalVelocity.z = 0;

        // 根据状态调整动画速度
        float speedMultiplier = 1f;
        if (CurrentState == PlayerState.Running) speedMultiplier = 1.5f;
        if (CurrentState == PlayerState.Crouching) speedMultiplier = 0.6f;
        if (CurrentState == PlayerState.Aiming) speedMultiplier = 0.8f;

        // 应用速度乘数
        horizontalVelocity *= speedMultiplier;

        // 平滑动画过渡
        float moveX = Mathf.Clamp(horizontalVelocity.x, -2f, 2f);
        float moveY = Mathf.Clamp(horizontalVelocity.y, -2f, 2f);

        playerAnimator.SetFloat(moveXHash,
            Mathf.Lerp(playerAnimator.GetFloat(moveXHash), moveX, 8f * Time.deltaTime));
        playerAnimator.SetFloat(moveYHash,
            Mathf.Lerp(playerAnimator.GetFloat(moveYHash), moveY, 8f * Time.deltaTime));
    }

    private bool CanJump()
    {
        // 允许在以下状态跳跃：
        // - 站立/行走/奔跑/瞄准状态
        // - 且接触地面
        return isGrounded && (
            CurrentState == PlayerState.Idle ||
            CurrentState == PlayerState.Walking ||
            CurrentState == PlayerState.Running
        );
    }
    // 判断是否可以瞄准
    private bool CanAim()
    {
        return isGrounded &&
               !input.IsCrouching &&
               CurrentState != PlayerState.Reloading &&
               CurrentState != PlayerState.Jumping &&
               CurrentState != PlayerState.Falling;
    }

    // 判断是否可以射击
    private bool CanFire()
    {
        return currentAmmo > 0 &&
               CurrentState != PlayerState.Reloading &&
               CurrentState != PlayerState.Jumping &&
               CurrentState != PlayerState.Falling;
    }

    // 判断是否可以装弹
    private bool CanReload()
    {
        return isGrounded &&
               CurrentState != PlayerState.Jumping &&
               CurrentState != PlayerState.Falling;
    }

    // 安全切换状态（已更新）
    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        // 记录之前状态（瞄准状态专用）
        if (newState != PlayerState.Aiming && CurrentState != PlayerState.Aiming)
            previousState = CurrentState;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(newState);
    }

    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Reloading:
                // 触发装弹动画
                playerAnimator.SetTrigger(reloadHash);
                // 重置射击计时器
                fireTimer = 0;
                break;

            case PlayerState.Aiming:
                // 设置瞄准标志
                isAiming = true;
                break;

            case PlayerState.Fireing:
                // 重置射击计时器
                fireTimer = 0;
                break;
        }
    }


    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Reloading:
                // 装弹完成，补充弹药
                currentAmmo = maxAmmo;
                break;

            case PlayerState.Aiming:
                // 重置瞄准标志
                isAiming = false;
                break;
        }
    }

    // 动画事件回调（由装弹动画调用）
    public void OnReloadComplete()
    {
        if (CurrentState == PlayerState.Reloading)
        {
            SetState(previousState);
        }
    }
}