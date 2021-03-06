﻿using ProjectTrader;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

class GameDateTimeManager : MonoBehaviour
{
    // 초기화 및 디버그용 변수
    [Serializable]
    public struct GameDateTimeInitData
    {
        public int year;
        public int month;
        public int day;
        public int hour;
        public int minute;
        public int second;

        public static explicit operator GameDateTime(GameDateTimeInitData gameDateTimeInitData)
        {
            return new GameDateTime(
                gameDateTimeInitData.year,
                gameDateTimeInitData.month,
                gameDateTimeInitData.day,
                gameDateTimeInitData.hour,
                gameDateTimeInitData.minute,
                gameDateTimeInitData.second);
        }

        public static explicit operator GameDateTimeInitData(GameDateTime gameDateTime)
        {
            return new GameDateTimeInitData()
            {
                year = gameDateTime.Year,
                month = gameDateTime.Month,
                day = gameDateTime.Day,
                hour = gameDateTime.Hour,
                minute = gameDateTime.Minute,
                second = gameDateTime.Second
            };
        }
    }

    [SerializeField] VisitorManager visitorManager;
    [SerializeField] JudgeResultWindow reviewResultWindow;
    [SerializeField] ResultWindow resultWindow;

    // 현실시간 5분 = 게임시간 1일
    // 현실시간 300초 = 게임시간 1440분 = 게임시간 86400초
    // 현실시간 1초 = 게임시간 288초

    // 여는 시간
    [SerializeField]
    private int openningHour = 9;
    // 닫는 시간
    [SerializeField]
    private int closingHour = 21;
    /// <summary>
    /// 게임내 시간 비율. 1초 = gameTimeScale
    /// </summary>
    [SerializeField]
    private float gameTimeScale = 288f;
    [SerializeField]
    private bool isStopped = false;
    [Header("디버깅용")]
    [SerializeField]
    private bool isNext;
    [SerializeField]
    private GameDateTimeInitData data;
    [SerializeField]
    private GameDateTime gameDateTime;
    [SerializeField]
    private float timePart = 0f;

    public bool IsStopped
    {
        get => isStopped;
        set => isStopped = value;
    }

    // 현재 게임내 시간을 가져옵니다.
    public GameDateTime GameDateTime
    {
        get => gameDateTime;
        set
        {
            gameDateTime = value;

            if (PlayData.CurrentData != null)
                PlayData.CurrentData.Date = value;

#if UNITY_EDITOR
            // 초기화
            data = (GameDateTimeInitData)gameDateTime;
#endif
        }
    }

    /// <summary>
    /// 현재 시간의 소수점부를 가져옵니다.
    /// </summary>
    public float TimePart
    {
        get => timePart;
        set => timePart = value;
    }
    public int OpenningHour => this.openningHour;
    public int ClosingHour => this.closingHour;

    public void TimeStart()
    {
        isStopped = false;
    }

    public void TimeStop()
    {
        isStopped = true;
    }

    /// <summary>
    /// 시간을 초기화합니다.
    /// </summary>
    public void Reset()
    {
        gameDateTime = default;
        timePart = default;
        data = default;
        isStopped = false;
    }

    private void Start()
    {
        //GameDateTime = (GameDateTime)data;
    }

    private void Update()
    {
        if (!IsStopped)
        {
            // 업데이트
            timePart += gameTimeScale * Time.deltaTime;

            int gameTimeInt = (int)timePart;
            // 소수점부 남기고 0으로 초기화
            timePart -= gameTimeInt;

            this.gameDateTime = PlayData.CurrentData.Date;
            var gameDateTime = this.gameDateTime;
            gameDateTime.AddSecond(gameTimeInt);
            this.gameDateTime = gameDateTime;
            PlayData.CurrentData.Date = gameDateTime;

            TimeCheck();
            // 문닫을 시간 확인
            ClosingTimeUpdate();

#if UNITY_EDITOR
            // 출력
            data = (GameDateTimeInitData)gameDateTime;
#endif
        }
        else
        {
#if UNITY_EDITOR
            // 디버깅용으로 인스펙터에서 IsNext가 활성화되었을때 동작하기 위한 용도임
            if (isNext)
            {
                isNext = false;
                Opening();
            }
#endif
        }
    }

    bool checked12h;

    /// <summary>
    /// 시간을 체크하고 특정 시간의 이벤트를 실행합니다.
    /// </summary>
    private void TimeCheck()
    {
        const bool isTest = false;

        // 12시
        if (GameDateTime.Hour >= 12 && !checked12h)
        {
            // 심사일 체크
            if (isTest || gameDateTime.TotalDays > 0 && gameDateTime.TotalDays % 14 == 0)
            {
                // 심사 연출 시작
                FindObjectOfType<JudgeManager>().StartDirecting();
            }

            checked12h = true;
        }

        // 문닫을 시간이 지났으면
        if (GameDateTime.Hour >= closingHour)
        {
            Closing();

            // 체크 변수 초기화
            checked12h = false;
        }
    }

    private void ClosingTimeUpdate()
    {
    }

    /// <summary>
    /// 가게문을 닫고 결산 윈도우 띄우기
    /// </summary>
    public void Closing()
    {
        TimeStop();
        var gameDateTime = PlayData.CurrentData.Date;
        // 닫는 시간으로 수정
        gameDateTime.Hour = closingHour;
        gameDateTime.Minute = 0;
        gameDateTime.Second = 0;
        PlayData.CurrentData.Date = gameDateTime;

        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            // 손님이 모두 나갈때까지 대기
            while (visitorManager.visitors.Count > 0)
            {
                yield return null;
            }

            //TODO: 결산 윈도우 호출
            resultWindow.gameObject.SetActive(true);

            // 결산 윈도우가 닫힐때까지 대기
            while (resultWindow.gameObject.activeSelf)
            {
                yield return null;
            }

            // 저장
            FindObjectOfType<DataSave>().GameSave();

            // 현재 열려있는 상점 씬이 현재 레벨과 다르면 씬 재로딩
            if ((int)SceneLoadManager.CurrentLoadedShopScene + 1 != PlayData.CurrentData.Level)
            {
                //yield return new WaitForSeconds(1);
                SceneLoadManager.Instance.LoadGameScene();
            }
        }
    }

    public void Opening()
    {
        TimeStart();
        var gameDateTime = PlayData.CurrentData.Date;
        // 여는 시간으로 수정
        gameDateTime.Hour = openningHour;
        gameDateTime.Minute = 0;
        gameDateTime.Second = 0;
        // 다음날
        gameDateTime.AddDay(1);
        PlayData.CurrentData.Date = gameDateTime;

        // 일일 퀘스트 갱신
        PlayData.CurrentData.UpdateDailyQuest();

        // 통계 초기화
        PlayData.CurrentData.DailyStatisticsData.Clear();
    }
}