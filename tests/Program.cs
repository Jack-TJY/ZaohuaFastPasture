using System;
using ZaohuaFastPasture;

// 离线核对倍率：dotnet run --project tests
int failed = 0;
void Check(string name, double actual, double expected)
{
    bool ok = Math.Abs(actual - expected) < 1e-6;
    if (!ok) failed++;
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {name}: {actual} (期望 {expected})");
}

// 灵草/矿石：相对原版裸种的速度 = 原 growTime / 新 growTime × (100 + 新加速) / 100
int[] baseTimes = { 10, 20, 30, 360, 720, 1080, 3600, 7200, 10800, 36000, 72000, 108000 };
foreach (int t in baseTimes)
{
    for (int n = 0; n <= 4; n++)
    {
        int time = Rules.ScaleTime(t, 0.5);
        int speed = Rules.FacilitySpeed(n * Rules.VanillaFacilityBonus, 0.5);
        double rel = (double)t / time * (100 + speed) / 100.0;
        if (t == 10800 || t == 108000 || t == 10) Check($"基础 {t} 天，{n} 个设施", rel, 2 * Math.Pow(2, n));
        else if (Math.Abs(rel - 2 * Math.Pow(2, n)) > 1e-6) Check($"基础 {t} 天，{n} 个设施", rel, 2 * Math.Pow(2, n));
    }
}
Check("没有设施时加速不变", Rules.FacilitySpeed(0, 0.5), 0);
Check("1 天作物不会变成 0", Rules.ScaleTime(1, 0.5), 1);

// 灵鱼：一阶 ×0.5，二阶 ×0.25，三阶 ×0.125，四阶 ×0.0625
Check("一阶中品灵鱼 20 天", Rules.FishTime(20, 2, 0.5), 10);
Check("一阶上品灵鱼 30 天", Rules.FishTime(30, 3, 0.5), 15);
Check("二阶下品灵鱼 360 天", Rules.FishTime(360, 4, 0.5), 90);
Check("二阶上品灵鱼 1080 天", Rules.FishTime(1080, 6, 0.5), 270);
Check("三阶下品灵鱼 3600 天", Rules.FishTime(3600, 7, 0.5), 450);
Check("三阶上品灵鱼 10800 天", Rules.FishTime(10800, 9, 0.5), 1350);
Check("四阶上品 108000 天", Rules.FishTime(108000, 12, 0.5), 6750);

Console.WriteLine(failed == 0 ? "全部通过" : $"{failed} 项失败");
return failed == 0 ? 0 : 1;
