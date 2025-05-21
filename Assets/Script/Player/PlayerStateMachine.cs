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
    [SerializeField] private Animator playerAnimator; //主角动画状态机组件
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

    [Header("动画设置")]
    private readonly int moveXHash = Animator.StringToHash("Move_X");
    private readonly int moveYHash = Animator.StringToHash("Move_Y");
    [SerializeField][Range(0, 1)] private float weight = 0f;

    [Header("状态管理")]
    public PlayerState CurrentState; // 当前状态（公开可读）
    private Vector3 verticalVelocity;    // 垂直速度（跳跃/下落）
    private float originalCameraY;       // 摄像机原始Y轴位置
    private bool isGrounded;             // 是否接触地面

    private void Awake()
    {
        // 初始化组件引用
        controller = GetComponent<CharacterController>(); // 获取角色控制器
        input = GetComponent<InputHandler>();         // 获取输入组件
        originalCameraY = cameraRoot.localPosition.y;    // 记录初始摄像机高度
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;    // 更新地面检测状态
        HandleStateTransition();               // 处理状态转换逻辑
        HandleHorizontalMovement();            // 处理水平移动
        HandleVerticalMovement();              // 处理垂直运动
        HandleCrouch();                        // 处理下蹲状态
        AnimationParameters();                 // 处理动画参数
        input.ConsumeActions();                // 重置瞬时输入
    }

    /// 处理状态转换逻辑
    private void HandleStateTransition()
    {
        if (input.ReloadTriggered)
        {
            SetState(PlayerState.Reloading);
            return;
        }

        if (input.FireTriggered && CurrentState != PlayerState.Reloading)
        {
            SetState(PlayerState.Fireting);
            return;
        }

        if (isGrounded)
        {
            if (input.IsCrouching)
            {
                SetState(PlayerState.Crouching);
                return;
            }

            if (input.MoveInput != Vector2.zero)
            {
                CurrentState = input.IsSprinting ? PlayerState.Running : PlayerState.Walking;
                return;
            }

            SetState(PlayerState.Idle); // 添加默认状态
        }
    }

    // 处理水平移动（地面/空中）
    private void HandleHorizontalMovement()
    {
        // 根据不同状态调整速度
        float targetSpeed = CurrentState switch
        {
            PlayerState.Running => runSpeed,
            PlayerState.Crouching => crouchSpeed,
            PlayerState.Aiming => aimSpeed,
            PlayerState.Walking => walkSpeed,
            _ => 0
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
        // 重力应用
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // 落地后重置速度
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        // 跳跃触发
        if (input.JumpTriggered && CanJump())
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            SetState(PlayerState.Jumping);
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
    
    private void AnimationParameters()
    {
        Vector3 horizontalVelocity = input.MoveInput;

        if (input.IsSprinting == true) horizontalVelocity *= 2;
        float moveX = Mathf.Clamp(horizontalVelocity.x, -2f, 2f);
        float moveY = Mathf.Clamp(horizontalVelocity.y, -2f, 2f);

        // 应用平滑过渡
        playerAnimator.SetFloat(moveXHash,
            Mathf.Lerp(playerAnimator.GetFloat(moveXHash), -moveX, 5f * Time.deltaTime));
        playerAnimator.SetFloat(moveYHash,
            Mathf.Lerp(playerAnimator.GetFloat(moveYHash), -moveY, 5f * Time.deltaTime));
    }
    

    // 判断是否可以跳跃
    private bool CanJump()
    {
        // 允许跳跃的条件：接触地面且处于允许跳跃的状态
        return isGrounded && (
            CurrentState == PlayerState.Idle ||
            CurrentState == PlayerState.Walking ||
            CurrentState == PlayerState.Running ||
            CurrentState == PlayerState.Aiming
        );
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log(playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow));
        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, new Vector3(0, 0, -1));
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, weight);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, weight);
    }

    // 安全切换状态
    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        ExitState(CurrentState); // 退出旧状态
        CurrentState = newState; // 更新状态
        EnterState(newState);    // 进入新状态
    }

    private void EnterState(PlayerState state)
    {
        /* 状态进入逻辑（示例）：
        switch(state) {
            case PlayerState.Crouching:
                // 播放下蹲音效
                break;
        }
        */
    }

    private void ExitState(PlayerState state)
    {
        /* 状态退出逻辑（示例）：
        switch(state) {
            case PlayerState.Fireting:
                // 重置射击冷却
                break;
        }
        */
    }
}