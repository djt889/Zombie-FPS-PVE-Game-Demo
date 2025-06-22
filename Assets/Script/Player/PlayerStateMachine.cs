using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Transform cameraRoot; // 摄像机根节点（用于下蹲时调整高度）
    private Animator playerAnimator; //主角动画状态机组件
    private CharacterController controller;        // 角色控制器组件
    private InputHandler input;                // 输入处理组件
    private WeaponManager weaponManager; // 武器管理器组件

    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;   // 基础移动速度
    [SerializeField] private float runSpeed = 8f;    // 冲刺速度
    [SerializeField] private float crouchSpeed = 2.5f; // 下蹲速度

    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 7f;    // 跳跃初速度
    [SerializeField] private float gravity = -9.81f;  // 重力加速度
    [SerializeField][Range(0, 1)] private float airControl = 0.7f; // 空中移动系数（0-1）

    [Header("下蹲设置")]
    [SerializeField] private float crouchHeight = 1f;     // 下蹲时角色高度
    [SerializeField] private float standHeight = 2f;      // 站立时原始高度
    [SerializeField] private float crouchTransitionSpeed = 5f; // 高度过渡速度

    [Header("动画设置")]
    private readonly int moveXHash = Animator.StringToHash("Move_X"); //X轴移动参数
    private readonly int moveYHash = Animator.StringToHash("Move_Y"); //Y轴移动参数
    private readonly int isRuningHash = Animator.StringToHash("IsRuning"); // 奔跑动画参数
    private readonly int isCrouchingHash = Animator.StringToHash("IsCrouching"); // 下蹲动画参数
    private readonly int isJumpHash = Animator.StringToHash("IsJump"); // 跳跃触发
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded"); // 落地触发

    [Header("状态管理")]
    public PlayerState CurrentState; // 当前状态（公开可读）
    private Vector3 verticalVelocity;    // 垂直速度（跳跃/下落）
    private bool wasGrounded; // 用于检测落地
    private float originalCameraY;       // 摄像机原始Y轴位置
    private bool isGrounded;             // 是否接触地面

    private void Awake()
    {
        // 初始化组件引用
        controller = GetComponent<CharacterController>(); // 获取角色控制器
        playerAnimator = GetComponent<Animator>();
        input = GetComponent<InputHandler>();         // 获取输入组件
        weaponManager = GetComponent<WeaponManager>(); // 获取武器管理器
        wasGrounded = true;     // 上一帧是否在地面上
        originalCameraY = cameraRoot.localPosition.y;    // 记录初始摄像机高度
    }

    public void ProcessInputs()
    {
        isGrounded = controller.isGrounded;    // 更新地面检测状态
        HandleStateTransition();               // 处理状态转换逻辑
        HandleHorizontalMovement();            // 处理水平移动
        HandleVerticalMovement();              // 处理垂直运动
        HandleCrouch();                        // 处理下蹲状态
        HandleWeaponInput();                   // 处理武器输入
        MoveAnimationParameters();             // 处理移动动画参数
        JumpAnimationParameters();             // 处理跳跃动画参数
    }

    /// 处理状态转换逻辑
    private void HandleStateTransition()
    {
        // 空中状态处理
        if (!isGrounded)
        {
            // 保持当前状态（射击/装弹）不变，仅当非动作状态时更新为跳跃/下落
            if (CurrentState != PlayerState.Fireing && CurrentState != PlayerState.Reloading)
            {
                CurrentState = verticalVelocity.y > 0 ? PlayerState.Jumping : PlayerState.Falling;
            }
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
            PlayerState.Walking => walkSpeed,
            PlayerState.Jumping or PlayerState.Falling => walkSpeed * airControl,
            _ => walkSpeed // 默认使用行走速度
        };

        // 计算移动方向
        Vector3 moveDirection = transform.TransformDirection(new Vector3(input.MoveInput.x, 0, input.MoveInput.y));

        // 平滑移动过渡
        Vector3 targetVelocity = moveDirection * targetSpeed;
        controller.Move(targetVelocity * Time.deltaTime);
    }

    // 处理垂直运动（跳跃/重力）
    private void HandleVerticalMovement()
    {
        // 允许在任何地面状态跳跃（包括移动时）
        if (input.JumpTriggered && CanJump())
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            SetState(PlayerState.Jumping);
        }

        // 重力应用
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
            if ((CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling))
            { 
                SetState(PlayerState.Idle);
            }
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
            if (!isGrounded && verticalVelocity.y < 0 && CurrentState != PlayerState.Falling)
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

        playerAnimator.SetBool(isCrouchingHash, CurrentState == PlayerState.Crouching);
    }

    // 武器
    private void HandleWeaponInput()
    {
        // 鼠标滚轮切换武器类型
        if (input.SwitchWeaponType != 0)
        {
            // 计算新武器类型索引
            int newType = (int)weaponManager.currentWeaponType + input.SwitchWeaponType;

            // 循环处理索引 (0-Primary, 1-Secondary, 2-Melee)
            if (newType < 0) newType = 2;
            if (newType > 2) newType = 0;

            // 装备新类型武器
            weaponManager.EquipWeapon((WeaponType)newType);
        }

        // 数字键切换武器类型
        switch (input.SwitchWeaponIndex)
        {
            case 0:
                weaponManager.EquipWeapon(WeaponType.Primary);
                break;
            case 1:
                weaponManager.EquipWeapon(WeaponType.Secondary);
                break;
            case 2:
                weaponManager.EquipWeapon(WeaponType.Melee);
                break;
        }

        //丢弃武器
        if (input.DiscardTriggered)
        {
            WeaponBase currentWeapon = weaponManager.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                // 从武器管理器移除
                weaponManager.RemoveWeapon(currentWeapon.GetWeaponType());

                // 丢弃到场景中
                GetComponent<WeaponPickupHandler>().DropWeapon(currentWeapon);
            }
        }
        // 传递输入到武器管理器
        weaponManager.UpdateWeapon(input.FireTriggered, input.ReloadTriggered);
    }

    //动画
    private void MoveAnimationParameters()
    {
        Vector3 horizontalVelocity = input.MoveInput;
        horizontalVelocity.z = 0;

        // 根据状态调整动画速度
        float speedMultiplier = 1f;
        if (CurrentState == PlayerState.Running) speedMultiplier = 1.5f;
        if (CurrentState == PlayerState.Crouching) speedMultiplier = 0.6f;

        // 应用速度乘数
        horizontalVelocity *= speedMultiplier;

        // 平滑动画过渡
        float moveX = Mathf.Clamp(horizontalVelocity.x, -2f, 2f);
        float moveY = Mathf.Clamp(horizontalVelocity.y, -2f, 2f);

        playerAnimator.SetFloat(moveXHash,
            Mathf.Lerp(playerAnimator.GetFloat(moveXHash), moveX, 8f * Time.deltaTime));
        playerAnimator.SetFloat(moveYHash,
            Mathf.Lerp(playerAnimator.GetFloat(moveYHash), moveY, 8f * Time.deltaTime));

        playerAnimator.SetBool(isRuningHash, CurrentState == PlayerState.Running);
    }

    private void JumpAnimationParameters()
    {
        playerAnimator.SetBool(isGroundedHash, isGrounded);
        // 如果刚落地，立即关闭跳跃动画
        if (wasGrounded == false && controller.isGrounded)
        {
            //playerAnimator.Play("Move");
            playerAnimator.SetBool(isJumpHash, false);
        }
        // 否则根据空中状态设置跳跃动画
        else if (CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling)
        {
            playerAnimator.SetBool(isJumpHash, true);
        }

        // 记录上一帧地面状态
        wasGrounded = isGrounded;
    }

    private bool CanJump()
    {
        // 允许在移动状态（行走/奔跑）时跳跃
        return isGrounded && (
            CurrentState == PlayerState.Idle ||
            CurrentState == PlayerState.Walking ||
            CurrentState == PlayerState.Running ||
            CurrentState == PlayerState.Crouching ||
            CurrentState == PlayerState.Falling
        );
    }

    // 安全切换状态
    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
    }
}