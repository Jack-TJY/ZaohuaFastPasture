using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace ZaohuaFastPasture
{
    // 造化鼎小世界：灵草/矿石基础生长时间缩短，灵泉/火炼池改为相乘叠加，灵鱼按品阶缩短。
    // 原版每次 RefreshPastureEffect 都从零重算 growTime 和 updateGrowSpeed，这里在它之后改写。
    [BepInPlugin("zaohua.fastpasture", "小世界加速", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const int HerbTypeId = 10;
        public const int MaterialTypeId = 11;
        public const int FishTypeId = 902;
        public const int SpringBuildId = 4;
        public const int FirePoolBuildId = 6;

        internal static ConfigEntry<float> BaseGrowTimeMultiplier;
        internal static ConfigEntry<float> FacilityTimeMultiplier;
        internal static ConfigEntry<float> FishTimeMultiplierPerTier;

        private void Awake()
        {
            BaseGrowTimeMultiplier = Config.Bind("General", "BaseGrowTimeMultiplier", 0.5f, "灵草/矿石基础生长时间倍率，0.5 = 减半");
            FacilityTimeMultiplier = Config.Bind("General", "FacilityTimeMultiplier", 0.5f, "每个相邻灵泉/火炼池的生长时间倍率，相乘叠加，0.5 = 每个减半（4 个 = 0.5^4）");
            FishTimeMultiplierPerTier = Config.Bind("General", "FishTimeMultiplierPerTier", 0.5f, "灵鱼每阶的生长时间倍率，一阶 ×0.5、四阶 ×0.5^4");
            new Harmony("zaohua.fastpasture").PatchAll();
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
                if (type == HerbTypeId || parent == HerbTypeId || type == MaterialTypeId || parent == MaterialTypeId)
                {
                    sto.growTime = Rules.ScaleTime(sto.growTime, BaseGrowTimeMultiplier.Value);
                    sto.updateGrowSpeed = Rules.FacilitySpeed(sto.updateGrowSpeed, FacilityTimeMultiplier.Value);
                }
                else if (type == FishTypeId || parent == FishTypeId)
                {
                    TbGradeCfg grade = Singleton<TbDataImpl>.Instance.GetGradeCfg(item.gradeId);
                    sto.growTime = Rules.FishTime(sto.growTime, grade?.weight ?? 0, FishTimeMultiplierPerTier.Value);
                }
            }
        }

        internal static string FacilityText()
        {
            float f = FacilityTimeMultiplier.Value;
            return f == 0.5f ? "减半" : "×" + f;
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
