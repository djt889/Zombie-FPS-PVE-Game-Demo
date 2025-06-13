using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 输入协调器 - 管理所有输入处理脚本的执行顺序
[RequireComponent(typeof(WeaponPickupHandler))]
[RequireComponent(typeof(PlayerStateMachine))]
public class InputCoordinator : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private WeaponPickupHandler pickupHandler;
    [SerializeField] private PlayerStateMachine stateMachine;

    private InputHandler _input;

    private void Start()
    {
        _input = GetComponent<InputHandler>();
    }

    private void Update()
    {
        // 第一步：处理拾取输入（最高优先级）
        if (pickupHandler != null)
        {
            pickupHandler.ProcessPickupInput();
        }

        // 第二步：处理状态机和其他输入
        if (stateMachine != null)
        {
            stateMachine.ProcessInputs();
        }

        // 最后：重置瞬时输入
        _input.ConsumeActions();
    }
}