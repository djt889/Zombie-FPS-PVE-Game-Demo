using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    // 移动输入
    [Header("Movement Inputs")]
    public Vector2 MoveInput;    // WASD输入值
    public Vector2 LookInput;    // 鼠标输入
    public bool IsSprinting;     // 奔跑状态

    // 动作输入
    [Header("Action Inputs")]
    public bool JumpTriggered;   // 跳跃触发
    public bool IsCrouching;     // 下蹲状态
    public bool IsAiming;        // 瞄准状态
    public bool FireTriggered;   // 射击触发
    public bool ReloadTriggered; // 装弹触发

    // 移动输入事件
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }
    // 视角输入事件
    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>(); ;
    }

    // 奔跑输入
    public void OnSprint(InputAction.CallbackContext context)
    {
        IsSprinting = context.ReadValueAsButton();
    }

    // 跳跃输入
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) JumpTriggered = true;
    }

    // 下蹲输入
    public void OnCrouch(InputAction.CallbackContext context)
    {
        IsCrouching = context.ReadValueAsButton();
    }

    // 瞄准输入
    public void OnAim(InputAction.CallbackContext context)
    {
        // 右键按住期间保持瞄准状态
        IsAiming = context.ReadValueAsButton();
    }

    // 射击输入
    public void OnFire(InputAction.CallbackContext context)
    {
        // 左键按下时触发（支持连发）
        FireTriggered = context.started;
    }

    // 装弹输入
    public void OnReload(InputAction.CallbackContext context)
    {
        // R键按下时触发（单次）
        if (context.started) ReloadTriggered = true;
    }

    // 重置瞬时触发
    public void ConsumeActions()
    {
        JumpTriggered = false;   // 防止连续跳跃
        FireTriggered = false;  // 重置射击触发
        ReloadTriggered = false; // 重置装弹触发
    }
}
