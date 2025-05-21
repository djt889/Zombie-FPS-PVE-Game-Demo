using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 角色状态枚举
public enum PlayerState
{
    Idle,        // 闲置
    Walking,     // 行走
    Running,     // 奔跑
    Crouching,   // 下蹲
    Jumping,     // 跳跃
    Falling,     // 下落
    Aiming,      // 瞄准
    Reloading,   // 装弹
    Fireting     // 射击
}
