using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRoot; // 摄像机根节点
    private CharacterController controller;
    private InputHandler input;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 5f;

    [Header("State Management")]
    public PlayerState CurrentState;
    private Vector3 verticalVelocity;
    private float originalCameraY;
    private bool isGrounded;

    [Header("Air Control")]
    [SerializeField] private float airControlMultiplier = 0.5f; // 空中移动系数
    [SerializeField] private float maxAirSpeed = 3f;          // 空中最大速度

    private Vector3 horizontalVelocity; // 新增水平速度分量

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputHandler>();
        originalCameraY = cameraRoot.localPosition.y;
    }

    private void Update()
    {
        isGrounded = controller.isGrounded;
        HandleStateTransition();
        HandleHorizontalMovement(); // 拆分为单独的水平移动处理
        HandleVerticalMovement();   // 垂直移动单独处理
        HandleCrouch();
        input.ConsumeActions();
    }

    private void HandleHorizontalMovement()
    {
        float speedMultiplier = 1f;
        float maxSpeed = walkSpeed;

        // 根据不同状态调整移动参数
        if (isGrounded)
        {
            speedMultiplier = CurrentState switch
            {
                PlayerState.Running => 1f,
                PlayerState.Crouching => 0.5f,
                _ => 1f
            };
            maxSpeed = GetCurrentSpeed();
        }
        else
        {
            speedMultiplier = airControlMultiplier;
            maxSpeed = maxAirSpeed;
        }

        // 计算目标移动方向
        Vector3 targetDirection = transform.TransformDirection(
            new Vector3(input.MoveInput.x, 0, input.MoveInput.y));

        // 渐进加速
        horizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            targetDirection * maxSpeed * speedMultiplier,
            (isGrounded ? 10f : 5f) * Time.deltaTime
        );

        controller.Move(horizontalVelocity * Time.deltaTime);
    }

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

    // 状态转换逻辑
    private void HandleStateTransition()
    {
        // 空中状态保持
        if (!isGrounded) return;

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

        SetState(PlayerState.Idle);
    }

    // 移动
    private void HandleMovement()
    {
        float speed = GetCurrentSpeed();
        Vector3 moveDirection = transform.TransformDirection(
            new Vector3(input.MoveInput.x, 0, input.MoveInput.y));

        controller.Move(moveDirection * speed * Time.deltaTime);

        // 跳跃
        if (input.JumpTriggered && CanJump())
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    // 下蹲
    private void HandleCrouch()
    {
        float targetHeight = CurrentState == PlayerState.Crouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        // 同步摄像机高度
        Vector3 camPos = cameraRoot.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, originalCameraY * (targetHeight / standHeight), crouchTransitionSpeed * Time.deltaTime);
        cameraRoot.localPosition = camPos;
    }

    // 重力
    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private float GetCurrentSpeed()
    {
        return CurrentState switch
        {
            PlayerState.Running => runSpeed,
            PlayerState.Crouching => crouchSpeed,
            PlayerState.Walking => walkSpeed,
            _ => 0f
        };
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

    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        // 可在此处触发状态切换事件
    }
}