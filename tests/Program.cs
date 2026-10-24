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

// 默认设置：各品阶天数为原版一半，每个设施缩短 50%
Func<int, int> halfDays = w => Rules.VanillaDays[w - 1] / 2;

// 灵草/矿石：相对原版裸种的速度 = 原 growTime / 新 growTime × (100 + 新加速) / 100
for (int w = 1; w <= Rules.VanillaDays.Length; w++)
{
    int t = Rules.VanillaDays[w - 1];
    for (int n = 0; n <= 4; n++)
    {
        int time = Rules.CropTime(t, w, halfDays);
        int speed = Rules.FacilitySpeed(n * Rules.VanillaFacilityBonus, 50);
        double rel = (double)t / time * (100 + speed) / 100.0;
        if (w == 1 || w == 9 || w == 12 || Math.Abs(rel - 2 * Math.Pow(2, n)) > 1e-6)
            Check($"{Rules.GradeNames[w - 1]} {t} 天，{n} 个设施", rel, 2 * Math.Pow(2, n));
    }
}

// 玩家自己设的天数和百分比
Check("三阶上品设成 100 天", Rules.CropTime(10800, 9, w => 100), 100);
Check("设成 0 天也至少 1 天", Rules.CropTime(10800, 9, w => 0), 1);
Check("五阶（表外品阶）保持原值", Rules.CropTime(5000, 13, halfDays), 5000);
Check("没种东西不改", Rules.CropTime(0, 9, halfDays), 0);
Check("每个设施缩短 75%，2 个 = 剩 1/16（加速 +1500%）", Rules.FacilitySpeed(200, 75), 1500);
Check("每个设施缩短 0% = 设施无效", Rules.FacilitySpeed(300, 0), 0);
Check("没有设施时加速不变", Rules.FacilitySpeed(0, 50), 0);

// 灵鱼：每阶缩短 50%，一阶 ×0.5，二阶 ×0.25，三阶 ×0.125，四阶 ×0.0625
Check("一阶中品灵鱼 20 天", Rules.FishTime(20, 2, 50), 10);
Check("一阶上品灵鱼 30 天", Rules.FishTime(30, 3, 50), 15);
Check("二阶下品灵鱼 360 天", Rules.FishTime(360, 4, 50), 90);
Check("二阶上品灵鱼 1080 天", Rules.FishTime(1080, 6, 50), 270);
Check("三阶下品灵鱼 3600 天", Rules.FishTime(3600, 7, 50), 450);
Check("三阶上品灵鱼 10800 天", Rules.FishTime(10800, 9, 50), 1350);
Check("四阶上品 108000 天", Rules.FishTime(108000, 12, 50), 6750);
Check("灵鱼 0% = 原版", Rules.FishTime(10800, 9, 0), 10800);

Console.WriteLine(failed == 0 ? "全部通过" : $"{failed} 项失败");
return failed == 0 ? 0 : 1;
