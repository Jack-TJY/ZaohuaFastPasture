using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace ZaohuaFastPasture
{
    // 造化鼎小世界：灵草/矿石基础生长时间按品阶可调，灵泉/火炼池改为相乘叠加，灵鱼按品阶缩短。
    // 原版每次 RefreshPastureEffect 都从零重算 growTime 和 updateGrowSpeed，这里在它之后改写。
    [BepInPlugin("zaohua.fastpasture", "小世界加速", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const int HerbTypeId = 10;
        public const int MaterialTypeId = 11;
        public const int FishTypeId = 902;
        public const int SpringBuildId = 4;
        public const int FirePoolBuildId = 6;

        internal static ConfigEntry<int> FacilityReducePercent;
        internal static ConfigEntry<int>[] CropDays;
        internal static ConfigEntry<int> FishReducePercent;

        private void Awake()
        {
            // 管理器设置页按分组名排序，所以分组名带序号
            FacilityReducePercent = Config.Bind("1 灵泉与火炼池（相乘叠加）", "每个设施缩短百分比", 50,
                new ConfigDescription("每个相邻的灵泉（灵草）/火炼池（矿石）让生长时间减少的百分比。多个设施相乘叠加，不是相加："
                    + "剩余时间 = (1 - 该值%)^设施个数。默认 50：1 个 ×0.5，2 个 ×0.25，4 个 ×0.0625。原版是每个 +100% 速度、多个相加。",
                    new AcceptableValueRange<int>(0, 95)));
            CropDays = new ConfigEntry<int>[Rules.GradeNames.Length];
            for (int i = 0; i < CropDays.Length; i++)
            {
                int vanilla = Rules.VanillaDays[i];
                CropDays[i] = Config.Bind("2 灵草与矿石基础生长时间（天）", Rules.GradeNames[i], Math.Max(1, vanilla / 2),
                    new ConfigDescription($"{Rules.GradeNames[i]}灵草/矿石不挨设施时的生长天数（1 年 = 360 天）。原版 {vanilla} 天，默认为原版一半。挨着设施时再按第 1 组相乘缩短。",
                        new AcceptableValueRange<int>(1, 1000000)));
            }
            FishReducePercent = Config.Bind("3 灵鱼", "每阶缩短百分比", 50,
                new ConfigDescription("灵鱼按品阶缩短：生长时间 × (1 - 该值%)^阶。默认 50：一阶 ×0.5，二阶 ×0.25，三阶 ×0.125，四阶 ×0.0625。0 = 原版。",
                    new AcceptableValueRange<int>(0, 95)));
            Config.SettingChanged += (sender, args) => RefreshNow();
            new Harmony("zaohua.fastpasture").PatchAll();
        }

        // 在游戏里改了设置就立刻重算，鼎里显示的剩余时间跟着变
        private static void RefreshNow()
        {
            try
            {
                if (BsSaveDataImpl.nowActor != null)
                {
                    Singleton<PastureImpl>.Instance.RefreshPastureEffect();
                }
            }
            catch (Exception)
            {
                // 还没进存档或数据没加载完，下次结算时会自动重算
            }
        }

        internal static void Apply()
        {
            if (BsSaveDataImpl.nowActor == null)
            {
                return;
            }
            foreach (TbPastureBuildSto sto in BsSaveDataImpl.nowActor.pastureBuildStoList)
            {
                if (!sto.itemId.IsId() || sto.growTime <= 0)
                {
                    continue;
                }
                TbItemCfg item = Singleton<TbItemImpl>.Instance.GetItemCfg(sto.itemId);
                if (item == null)
                {
                    continue;
                }
                int type = item.typeId;
                int parent = Singleton<TbItemImpl>.Instance.GetParentByTypeId(type);
                int weight = Singleton<TbDataImpl>.Instance.GetGradeCfg(item.gradeId)?.weight ?? 0;
                if (type == HerbTypeId || parent == HerbTypeId || type == MaterialTypeId || parent == MaterialTypeId)
                {
                    sto.growTime = Rules.CropTime(sto.growTime, weight, w => CropDays[w - 1].Value);
                    sto.updateGrowSpeed = Rules.FacilitySpeed(sto.updateGrowSpeed, FacilityReducePercent.Value);
                }
                else if (type == FishTypeId || parent == FishTypeId)
                {
                    sto.growTime = Rules.FishTime(sto.growTime, weight, FishReducePercent.Value);
                }
            }
        }

        internal static string FacilityText()
        {
            int p = FacilityReducePercent.Value;
            return p == 50 ? "减半" : "-" + p + "%";
        }
    }

    [HarmonyPatch(typeof(PastureImpl), nameof(PastureImpl.RefreshPastureEffect))]
    internal static class RefreshPastureEffectPatch
    {
        private static void Postfix()
        {
            Plugin.Apply();
        }
    }

    // 存档里存的是上次重算的结果，结算生长前先按当前倍率重算，旧存档的作物也立刻生效
    [HarmonyPatch(typeof(PastureImpl), nameof(PastureImpl.RefreshGrow))]
    internal static class RefreshGrowPatch
    {
        private static void Prefix(PastureImpl __instance)
        {
            if (BsSaveDataImpl.nowActor != null)
            {
                __instance.RefreshPastureEffect();
            }
        }
    }

    [HarmonyPatch(typeof(TbPastureBuildCfg), nameof(TbPastureBuildCfg.GetEffDes), MethodType.Getter)]
    internal static class GetEffDesPatch
    {
        private static void Postfix(TbPastureBuildCfg __instance, ref string __result)
        {
            if (__instance.id == Plugin.SpringBuildId)
            {
                __result = "四周相邻灵草生长时间" + Plugin.FacilityText() + "，多个相乘";
            }
            else if (__instance.id == Plugin.FirePoolBuildId)
            {
                __result = "四周相邻矿石生长时间" + Plugin.FacilityText() + "，多个相乘";
            }
        }
    }
}
