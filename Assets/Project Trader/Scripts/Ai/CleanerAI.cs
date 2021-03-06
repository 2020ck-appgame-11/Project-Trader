﻿using UnityEngine;
using System.Collections;
using Pathfinding;
using ProjectTrader;
using System;

public class CleanerAI : MonoBehaviour
{
    public enum AiState
    {
        None,
        Finding,
        ToLostArticle,
        Cleaning,
        Cleaned
    }

    public AiState state;

    public GameObject targetObject;
    public Transform targetTransform;

    public ScrapManager scrapManager;

    public EmployeeAnimation animation;

    // A* 프로젝트
    IAstarAI astarAI;

    float waitTime = 0;

    // Use this for initialization
    void Start()
    {
        // 초기 상태 지정
        state = AiState.Finding;
        astarAI = GetComponent<IAstarAI>();

        scrapManager = FindObjectOfType<ScrapManager>();
        animation = GetComponent<EmployeeAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetTransform == null)
        {
            Action();
        }
        if ((astarAI.destination - transform.position).sqrMagnitude < 0.0001f)
        {
            targetTransform = null;
        }
    }

    void SetTarget(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        this.targetObject = targetTransform.gameObject;

        astarAI.destination = targetTransform.position;
        astarAI.SearchPath();
    }

    void Action()
    {
        // 유한 상태 머신
        switch (state)
        {
            case AiState.Finding:
                Finding();
                break;
            case AiState.ToLostArticle:
                ToLostArticle();
                break;
            case AiState.Cleaning:
                Cleaning();
                break;
            case AiState.Cleaned:
                Cleaned();
                break;
        }
    }

    private void Finding()
    {
        waitTime -= Time.deltaTime;
        if (waitTime < 0)
        {
            // 쓰레기 찾기
            //var scrapManager = FindObjectOfType<ScrapManager>();
            var trashes = scrapManager.FindTrashes();
            if (trashes.Length > 0)
            {
                // 가장 가까운 쓰레기 찾기
                Transform nearTransform = null;
                float nearSqrDistance = float.MaxValue;
                foreach (var trash in trashes)
                {
                    var trashTransform = trash.transform;
                    var trashSqrDistance = (trashTransform.position - this.transform.position).sqrMagnitude;
                    if (trashTransform.position.sqrMagnitude < nearSqrDistance)
                    {
                        nearTransform = trashTransform;
                        nearSqrDistance = trashSqrDistance;
                    }
                }
                SetTarget(nearTransform);
                state = AiState.ToLostArticle;
            }
            else
            {
                // 1초후 재탐색
                waitTime = 1;
            }
        }
    }

    private void ToLostArticle()
    {
        // 애니메이션 추가시 관련 코드 구현 해야함.
        if (targetObject != null)
        {
            var colider = targetObject.GetComponent<BoxCollider2D>();
            colider.enabled = false;
            FindObjectOfType<SoundControl>().CleanUpSound();
            var timer = scrapManager.CollectTrash(targetObject, 5f, (floatingTimer) =>
            {

                if (targetObject != null)
                {
                    // 해당 쓰레기 제거
                    //Destroy(targetObject);
                    var playData = PlayData.CurrentData;
                    playData.Stamina += 1;
                    FindObjectOfType<SoundControl>().CleanUpStop();
                    FindObjectOfType<Uiup>().Upstamina(1);
                    state = AiState.Cleaned;

                }
                else
                {
                    state = AiState.Finding;
                }
                //floatingTimer.FadeoutWithDestory();
            });
            if (timer == null)
                state = AiState.Finding;
            else
            {
                timer.boostRatio = 3;

                state = AiState.Cleaning;
                if (animation != null)
                    animation.IdleState = EmployeeAnimation.IdleStateType.Cleaning;
            }

            //FloatingTimer.Create(this.transform, 5f, (floatingTimer) =>
            //{
            //    // 해당 쓰레기 제거
            //    Destroy(targetObject);
            //    floatingTimer.FadeoutWithDestory();
            //    state = AiState.Cleaned;
            //});
        }
        else
        {
            // 탐색 시작;
            state = AiState.Finding;
        }
        state = AiState.Cleaning;
    }

    private void Cleaning()
    {

        if (targetObject == null)
        {
            state = AiState.Finding;
        }
    }

    private void Cleaned()
    {
        // 다시 추적 시작
        state = AiState.Finding;
        if (animation != null)
            animation.IdleState = EmployeeAnimation.IdleStateType.Default;
    }
}
