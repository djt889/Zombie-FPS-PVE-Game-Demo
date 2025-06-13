using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    [Header("移动输入")]
    public Vector2 MoveInput;    // WASD输入值
    public Vector2 LookInput;    // 鼠标输入
    public bool IsSprinting;     // 奔跑状态
    public bool JumpTriggered;   // 跳跃触发
    public bool IsCrouching;     // 下蹲状态

    [Header("武器输入")]
    public bool FireTriggered;      // 射击触发
    public bool ReloadTriggered;    // 装弹触发
    public bool PickupTriggered;    // 拾取输入
    public bool DiscardTriggered;   // 丢弃输入
    public int SwitchWeaponType;    // 鼠标滚轮方向：-1向下，1向上
    public int SwitchWeaponIndex;   // 数字键切换武器

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

    // 拾取输入
    public void OnPickup(InputAction.CallbackContext context)
    {
        if (context.started) PickupTriggered = true;
    }

    // 丢弃输入
    public void OnDiscard(InputAction.CallbackContext context)
    {
        if (context.started) DiscardTriggered = true;
    }


    // 鼠标滚轮输入（-1向下，1向上）
    public void OnSwitchWeaponType(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<Vector2>().y;
        SwitchWeaponType = scrollValue > 0 ? 1 : (scrollValue < 0 ? -1 : 0);
    }
    // 字母数字1输入
    public void OnSwitchWeapon1(InputAction.CallbackContext context)
    {
        if (context.started) SwitchWeaponIndex = 0;
        Debug.Log("1");
    }
    // 字母数字2输入
    public void OnSwitchWeapon2(InputAction.CallbackContext context)
    {
        if (context.started) SwitchWeaponIndex = 1;
        Debug.Log("2");
    }
    // 字母数字3输入
    public void OnSwitchWeapon3(InputAction.CallbackContext context)
    {
        if (context.started) SwitchWeaponIndex = 2;
        Debug.Log("3");
    }

    // 重置瞬时触发
    public void ConsumeActions()
    {
        JumpTriggered = false;    // 防止连续跳跃
        FireTriggered = false;    // 重置射击触发
        ReloadTriggered = false;  // 重置装弹触发
        PickupTriggered = false;  // 重置拾取触发
        DiscardTriggered = false; // 重置丢弃触发
        SwitchWeaponType = 0;
        SwitchWeaponIndex = -1;
    }
}
