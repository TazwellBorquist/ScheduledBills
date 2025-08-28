using HarmonyLib;
using RimWorld;
using Verse;

namespace TazB_ScheduledBills
{

    [DefOf]
    public static class BillRepeatModeDefOf
    {
        public static BillRepeatModeDef TazB_XPerNDays;
    }

    public class BillInfo
    {
        public List<Date> Dates = new List<Date>();
        public int Frequency = 1;
        public Date RepeatsOn = new Date(0);
        public int RepeatCount = 1;
    }

    public static class ExtraBillInfo
    {
        private static readonly Dictionary<Bill_Production, BillInfo> _extraBillInfo = new Dictionary<Bill_Production, BillInfo>();
        private static readonly Dictionary<Map, Date> _dayOfYearOnMap = new Dictionary<Map, Date>();
        private static readonly Dictionary<Map, bool> _dayChangedOnMap = new Dictionary<Map, bool>();

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
            _extraBillInfo.AddDistinct(bill, new BillInfo());
        }

        public static BillInfo GetInfo(Bill_Production bill)
        {
            return _extraBillInfo.GetValueSafe(bill);
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

        private static void CleanDeleted()
        {
            List<Bill_Production> toRemove = new List<Bill_Production>();

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
    }

}