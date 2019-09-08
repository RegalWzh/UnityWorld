﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Mover
{
    //经验值
    public int xpValue = 1;

    //追逐逻辑    
    public float speedMultiple = 0.75f;      //Enemy的速度为正常速度的speedMultiple倍
    public float triggerLength = 1.0f;      //在多少距离内能触发追逐
    public float chaseLength = 1.0f;        //能追逐到多远的距离
    public bool chasing;                    //是否追逐
    public bool collidingWithPlayer;

    private Transform playTransform;        //追逐玩家标识
    private Vector3 startingPosition;       //Enemy原始坐标

    //碰撞器
    public ContactFilter2D filter;
    private BoxCollider2D hitBox;
    private Collider2D[] hits = new Collider2D[10];

    protected override void Start()
    {
        base.Start();
        playTransform = GameManager.instance.player.transform;
        startingPosition = transform.position;
        hitBox = transform.GetChild(0).GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        collidingWithPlayer = false;

        //若Player在Enemy原始坐标chaseLength范围内时,可能被追逐
        if (Vector3.Distance(playTransform.position, startingPosition) < chaseLength)
        {
            //再 若Player与Enemy范围过近(triggerLength内),则Enemy开始追逐
            if (Vector3.Distance(playTransform.position, startingPosition) < triggerLength)
                chasing = true;

            //追逐状态:
            if (chasing)
            {
                //若Enemy与Player处于非碰撞状态,则继续进行追逐移动
                //否则保持与Player碰撞状态,无需再移动
                if (!collidingWithPlayer)
                {
                    UpdateMotor((playTransform.position - transform.position).normalized, speedMultiple);
                }
            }
            else
            {
                //非追逐状态: Enemy回到原始坐标
                UpdateMotor((startingPosition - transform.position), speedMultiple);
            }
        }
        else
        {
            //否则Enemy与Player距离过远,不再保持追逐状态,Enemy回到原始坐标
            UpdateMotor((startingPosition - transform.position), speedMultiple);
            chasing = false;
        }

   
        hitBox.OverlapCollider(filter, hits);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null)
                continue;

            //如果检测到范围内存在Player
            if(hits[i].tag=="Fighter" && hits[i].name == "Player")
            {
                collidingWithPlayer = true;
            }

            hits[i] = null;
        }
    }

    //Enemy死亡函数
    protected override void Death()
    {
        Destroy(gameObject);

        //玩家获得经验,显示+xp的UI
        GameManager.instance.GrantXP(xpValue);
        GameManager.instance.ShowText("+" + xpValue + " xp", 30, Color.magenta, transform.position, Vector3.up * 40, 1.0f);
    }
}
