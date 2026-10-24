using System;

namespace ZaohuaFastPasture
{
    // 纯计算，不碰游戏对象，方便离线测试
    public static class Rules
    {
        // 原版每个灵泉/火炼池给相邻作物 +100%，多个相加
        public const int VanillaFacilityBonus = 100;

        public static readonly string[] GradeNames =
        {
            "一阶下品", "一阶中品", "一阶上品",
            "二阶下品", "二阶中品", "二阶上品",
            "三阶下品", "三阶中品", "三阶上品",
            "四阶下品", "四阶中品", "四阶上品",
        };

        // 原版 CommonConst.pastureGrowTimeByWeight[1..12]，单位天
        public static readonly int[] VanillaDays = { 10, 20, 30, 360, 720, 1080, 3600, 7200, 10800, 36000, 72000, 108000 };

        public static int ScaleTime(int growTime, double multiplier)
        {
            if (growTime <= 0)
            {
                return growTime;
            }
            return Math.Max(1, (int)Math.Round(growTime * multiplier));
        }

        // 品阶 weight 1..12 换成玩家设置的天数，其它品阶保持原值
        public static int CropTime(int growTime, int gradeWeight, Func<int, int> daysOfWeight)
        {
            if (growTime <= 0 || gradeWeight < 1 || gradeWeight > VanillaDays.Length)
            {
                return growTime;
            }
            return Math.Max(1, daysOfWeight(gradeWeight));
        }

        // 把原版累加的加速换算成设施个数 n，改为每个设施让时间减少 reducePercent%，相乘叠加：
        // 剩余时间 = (1 - reducePercent%)^n
        public static int FacilitySpeed(int vanillaSpeed, int reducePercent)
        {
            if (vanillaSpeed <= 0 || reducePercent < 0 || reducePercent >= 100)
            {
                return vanillaSpeed;
            }
            int n = vanillaSpeed / VanillaFacilityBonus;
            int rest = vanillaSpeed - n * VanillaFacilityBonus;
            double keep = 1.0 - reducePercent / 100.0;
            return (int)Math.Round((Math.Pow(1.0 / keep, n) - 1.0) * 100.0) + rest;
        }

        // 品阶 weight 1-3 为一阶，4-6 为二阶……
        public static int TierOfWeight(int weight)
        {
            return weight <= 0 ? 0 : (weight + 2) / 3;
        }

        // 灵鱼：每阶让时间减少 reducePercent%，生长时间 × (1 - reducePercent%)^阶
        public static int FishTime(int growTime, int gradeWeight, int reducePercent)
        {
            return ScaleTime(growTime, Math.Pow(1.0 - reducePercent / 100.0, TierOfWeight(gradeWeight)));
        }
    }
}
