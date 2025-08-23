using RimWorld;
using Verse;

namespace TazB_ScheduledBills;

[DefOf]
public static class BillRepeatModeDefOf
{
    public static BillRepeatModeDef TazB_XPerNDays;
}

public class BillInfo
{
    public List<Date> Dates = [];
    public int Frequency = 1;
    public Date RepeatsOn = new(0);
    public int RepeatCount = 1;
}

public static class ExtraBillInfo
{
    private static readonly Dictionary<Bill_Production, BillInfo> _extraBillInfo = [];
    private static readonly Dictionary<Map, Date> _dayOfYearOnMap = [];
    private static readonly Dictionary<Map, bool> _dayChangedOnMap = [];

    private const int DELAY_TICKS = GenDate.TicksPerHour;

    public static void AddDistinctMap(Map map)
    {
        if (_dayOfYearOnMap.ContainsKey(map))
            return;

        _dayOfYearOnMap.Add(
            map,
            Date.DateOnMap(map)
        );
    }

    public static void AddBill(Bill_Production bill)
    {
        AddDistinctMap(bill.billStack.billGiver.Map);
        _extraBillInfo.AddDistinct(bill, new());
    }

    public static BillInfo? GetInfo(Bill_Production bill)
    {
        return _extraBillInfo.TryGetValue(bill);
    }

    public static void Tick()
    {
        if (GenTicks.TicksAbs % DELAY_TICKS == 0)
        {
            foreach (var kvp in _dayOfYearOnMap)
            {
                int currDay = GenLocalDate.DayOfYear(kvp.Key);
                _dayChangedOnMap[kvp.Key] = _dayOfYearOnMap[kvp.Key].DayOfYear != currDay;
                _dayOfYearOnMap[kvp.Key].DayOfYear = currDay;
            }

            CleanDeleted();
            UpdateBills();
        }
    }

    private static void UpdateBills()
    {
        foreach (var (bill, info) in _extraBillInfo)
        {
            Map bill_map = bill.billStack.billGiver.Map;
            // The day has changed since last checked
            if (_dayChangedOnMap[bill_map])
            {
                if (info.RepeatsOn.DayOfYear == _dayOfYearOnMap[bill_map].DayOfYear)
                {
                    bill.repeatCount = info.RepeatCount;
                    info.RepeatsOn.AddDays(info.Frequency);
                }
            }
        }
    }

    private static void CleanDeleted()
    {
        List<Bill_Production> toRemove = [];

        foreach (var (bill, _) in _extraBillInfo)
        {
            if (bill.DeletedOrDereferenced)
            {
                toRemove.Add(bill);
            }
        }

        foreach (var bill in toRemove)
        {
            _extraBillInfo.Remove(bill);
        }
    }
}
