using System;

namespace ZaohuaFastPasture
{
    // 纯计算，不碰游戏对象，方便离线测试
    public static class Rules
    {
        // 原版每个灵泉/火炼池给相邻作物 +100%，多个相加
        public const int VanillaFacilityBonus = 100;

        public static int ScaleTime(int growTime, double multiplier)
        {
            if (growTime <= 0)
            {
                return growTime;
            }
            return Math.Max(1, (int)Math.Round(growTime * multiplier));
        }

        // 把原版累加的加速换算成设施个数 n，改为每个设施让时间乘 facilityTime（相乘叠加）
        public static int FacilitySpeed(int vanillaSpeed, double facilityTime)
        {
            if (vanillaSpeed <= 0 || facilityTime <= 0)
            {
                return vanillaSpeed;
            }
            int n = vanillaSpeed / VanillaFacilityBonus;
            int rest = vanillaSpeed - n * VanillaFacilityBonus;
            return (int)Math.Round((Math.Pow(1.0 / facilityTime, n) - 1.0) * 100.0) + rest;
        }

        // 品阶 weight 1-3 为一阶，4-6 为二阶……
        public static int TierOfWeight(int weight)
        {
            return weight <= 0 ? 0 : (weight + 2) / 3;
        }

        public static int FishTime(int growTime, int gradeWeight, double perTier)
        {
            return ScaleTime(growTime, Math.Pow(perTier, TierOfWeight(gradeWeight)));
        }
    }
}
