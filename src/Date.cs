using System.Collections;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace TazB_ScheduledBills;

public class Date : IComparable<Date>
{
    public static readonly Date InvalidDate = new(60);

    // Rimworld years have 60 days. Day of year is stored as 0-59, with 0 being Aprimay 1st, and 59 being Decembary 15th
    public int DayOfYear;

    public Date(int dayOfYear)
    {
        DayOfYear = dayOfYear;
    }

    public Date(Quadrum quadrum, int day)
    {
        DayOfYear = day - 1;
        DayOfYear += quadrum switch
        {
            Quadrum.Aprimay => 0,
            Quadrum.Jugust => 15,
            Quadrum.Septober => 30,
            Quadrum.Decembary => 45,
            _ => 1 - day + 60
        };
    }

    public void AddDays(int days)
    {
        DayOfYear = DayOfYear + days % 60;
    }

    public void ToQuadrumAndDay(out Quadrum quadrum, out int dayOfQuadrum)
    {
        dayOfQuadrum = (DayOfYear % 15) + 1;

        quadrum = (DayOfYear / 15) switch
        {
            0 => Quadrum.Aprimay,
            1 => Quadrum.Jugust,
            2 => Quadrum.Septober,
            3 => Quadrum.Decembary,
            _ => Quadrum.Undefined
        };
    }

    public string ToNextResetDayString()
    {
        ToQuadrumAndDay(out Quadrum quadrum, out int dayOfQuadrum);

        if (quadrum == Quadrum.Undefined)
        {
            return "never";
        }

        string quadrumString = quadrum switch
        {
            Quadrum.Aprimay => "Aprimay",
            Quadrum.Jugust => "Jugust",
            Quadrum.Septober => "Septober",
            Quadrum.Decembary => "Decembary",
            _ => "Undefined",
        };

        string dayOrdinal = dayOfQuadrum switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th",
        };

        return String.Format("{0} {1}{2}", quadrumString, dayOfQuadrum, dayOrdinal);
    }

    int IComparable<Date>.CompareTo(Date? other)
    {
        int dayrhs = other switch
        {
            null => -1,
            _ => other.DayOfYear
        };

        return dayrhs - DayOfYear;
    }

    public static bool operator ==(Date x, Date y) => x.DayOfYear == y.DayOfYear;
    public static bool operator !=(Date x, Date y) => x.DayOfYear != y.DayOfYear;

    public override bool Equals(object? obj) => obj is Date other && this == other;
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