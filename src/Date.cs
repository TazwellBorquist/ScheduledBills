using System.Collections;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace TazB_ScheduledBills
{

    public class Date : IComparable<Date>
    {
        public static readonly Date InvalidDate = new Date(60);

        // Rimworld years have 60 days. Day of year is stored as 0-59, with 0 being Aprimay 1st, and 59 being Decembary 15th
        public int DayOfYear;

        public Date(int dayOfYear)
        {
            DayOfYear = dayOfYear;
        }

        public Date(Quadrum quadrum, int day)
        {
            DayOfYear = day - 1;
            int quadrumDay = 0;

            switch (quadrum)
            {
                case Quadrum.Aprimay: quadrumDay = 0; break;
                case Quadrum.Jugust: quadrumDay = 15; break;
                case Quadrum.Septober: quadrumDay = 30; break;
                case Quadrum.Decembary: quadrumDay = 45; break;
                default: quadrumDay = 60; break;
            };

            DayOfYear += quadrumDay;
        }

        public void AddDays(int days)
        {
            DayOfYear = DayOfYear + days % 60;
        }

        public void ToQuadrumAndDay(out Quadrum quadrum, out int dayOfQuadrum)
        {
            dayOfQuadrum = (DayOfYear % 15) + 1;
            quadrum = Quadrum.Undefined;
             switch (DayOfYear / 15)
            {
                case 0: quadrum = Quadrum.Aprimay; break;
                case 1: quadrum = Quadrum.Jugust; break;
                case 2: quadrum = Quadrum.Septober; break;
                case 3: quadrum = Quadrum.Decembary; break;
                default: quadrum = Quadrum.Undefined; break;
            };
        }

        public string ToNextResetDayString()
        {
            ToQuadrumAndDay(out Quadrum quadrum, out int dayOfQuadrum);

            if (quadrum == Quadrum.Undefined)
            {
                return "never";
            }

            string quadrumString = "Undefined";
            switch (quadrum)
            {
                case Quadrum.Aprimay: quadrumString = "Aprimay"; break;
                case Quadrum.Jugust: quadrumString = "Jugust"; break;
                case Quadrum.Septober: quadrumString = "Septober"; break;
                case Quadrum.Decembary: quadrumString = "Decembary"; break;
                default: quadrumString = "Undefined"; break;
            };

            string dayOrdinal = "";
            switch (dayOfQuadrum)
            {
                case 1: dayOrdinal = "st"; break;
                case 2: dayOrdinal = "nd"; break;
                case 3: dayOrdinal = "rd"; break;
                default: dayOrdinal = "th"; break;
            };

            return String.Format("{0} {1}{2}", quadrumString, dayOfQuadrum, dayOrdinal);
        }

        int IComparable<Date>.CompareTo(Date other)
        {
            return other.DayOfYear - DayOfYear;
        }

        public static bool operator ==(Date x, Date y) => x.DayOfYear == y.DayOfYear;
        public static bool operator !=(Date x, Date y) => x.DayOfYear != y.DayOfYear;

        public override bool Equals(object obj) => obj is Date other && this == other;
        public override int GetHashCode() => DayOfYear;

        public static bool operator >(Date x, Date y) => x.DayOfYear > y.DayOfYear;
        public static bool operator <(Date x, Date y) => x.DayOfYear < y.DayOfYear;
        public static bool operator >=(Date x, Date y) => x.DayOfYear >= y.DayOfYear;
        public static bool operator <=(Date x, Date y) => x.DayOfYear <= y.DayOfYear;

        public static Date DateOnMap(Map map)
        {
            int dayOfYear = GenLocalDate.DayOfYear(map);
            return new Date(dayOfYear);
        }
    }
}