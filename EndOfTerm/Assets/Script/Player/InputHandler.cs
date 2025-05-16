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

    // 重置瞬时触发
    public void ConsumeActions()
    {
        JumpTriggered = false;
    }
}
