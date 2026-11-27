using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        /// <summary>
        /// Today's local calendar day as yyyymmdd, the day the replay cap (v1 #80) counts against.
        /// Uses DateTime.Now like the other day-based views (TodayWeather, the returning diary), so
        /// the QA tour's fixed hour (Now) never changes which day pays.
        /// </summary>
        private static int LocalDayStamp()
        {
            DateTime today = DateTime.Now;
            return today.Year * 10000 + today.Month * 100 + today.Day;
        }
    }
}
