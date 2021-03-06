﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Timeline;

namespace ProjectTrader
{
    [Serializable]
    public struct GameDateTime
    {
        private const int TicksPerSecond = 1;
        private const int TicksPerMinute = TicksPerSecond * 60;
        private const int TicksPerHour = TicksPerMinute * 60;
        private const int TicksPerDay = TicksPerHour * 24;
        private const int TicksPerMonth = TicksPerDay * 30;
        private const int TicksPerYear = TicksPerDay * 12;

        /// <summary>
        /// 2,147,483,647초
        /// </summary>
        public const int MaxSeconds = int.MaxValue / TicksPerSecond;
        /// <summary>
        /// 35,791,394분
        /// </summary>
        public const int MaxMinutes = MaxSeconds / 60;
        /// <summary>
        /// 596,523시간
        /// </summary>
        public const int MaxHours = MaxMinutes / 60;
        /// <summary>
        /// 24,855일
        /// </summary>
        public const int MaxDays = MaxHours / 24;
        /// <summary>
        /// 828달
        /// </summary>
        public const int MaxMonth = MaxDays / 30;
        /// <summary>
        /// 69년
        /// </summary>
        public const int MaxYear = MaxMonth / 30;

        public GameDateTime(int year = 0, int month = 0, int day = 0, int hour = 0, int minute = 0, int second = 0)
        {
            ticks =
                year * TicksPerYear +
                month * TicksPerMonth +
                day * TicksPerDay +
                hour * TicksPerHour +
                minute * TicksPerMinute +
                second * TicksPerSecond;
        }

        public int ticks;

        public int TotalSeconds
        {
            get => ticks / TicksPerSecond;
            //set => ticks = value * TicksPerSecond;
        }

        public int TotalMinutes
        {
            get => ticks / TicksPerMinute;
            //set => ticks = value * TicksPerMinute;
        }

        public int TotalHours
        {
            get => ticks / TicksPerHour;
            //set => ticks = value * TicksPerHour;
        }

        public int TotalDays
        {
            get => ticks / TicksPerDay;
            //set => ticks = (value - 1) * TicksPerDay;
        }

        public int TotalMonths
        {
            get => ticks / TicksPerMonth;
            //set => ticks = (value - 1) * TicksPerMonth;
        }

        public int TotalYears
        {
            get => ticks / TicksPerYear;
            //set => ticks = (value - 1) * TicksPerYear;
        }

        public int Second
        {
            get => TotalSeconds % 60;
            set => AddSecond(value - Second);
        }

        public int Minute
        {
            get => TotalMinutes % 60;
            set => AddMinute(value - Minute);
        }

        public int Hour
        {
            get => TotalHours % 24;
            set => AddHour(value - Hour);
        }

        public int Day
        {
            get => TotalDays % 30;
            set => AddDay(value - Day);
        }

        public int Month
        {
            get => TotalMonths % 12;
            set => AddMonth(value - Month);
        }

        public int Year
        {
            get => TotalYears;
            set => AddYear(value - Year);
        }

        public void AddSecond() => ticks += TicksPerSecond;
        public void AddSecond(int value) => ticks += value * TicksPerSecond;
        public void AddMinute(int value) => ticks += value * TicksPerMinute;
        public void AddHour(int value) => ticks += value * TicksPerHour;
        public void AddDay(int value) => ticks += value * TicksPerDay;
        public void AddMonth(int value) => ticks += value * TicksPerMonth;
        public void AddYear(int value) => ticks += value * TicksPerYear;

        public override string ToString() => Year + "년 " + Month + "월 " + Day + "일 " + Hour + "시 " + Minute + "분 " + Second + "초";
    }
}
