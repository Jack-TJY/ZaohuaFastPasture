# ZaohuaFastPasture 小世界加速

《造化仙缘》BepInEx 插件：加速造化鼎小世界里灵草、矿石和灵鱼的生长。

## 效果

- 灵草（灵田）和矿石（矿脉）的基础生长时间 ×0.5。
- 灵泉/火炼池从原版的「每个 +100%，多个相加」改为「每个让生长时间 ×0.5，多个相乘」。挨 n 个设施时生长时间 ×0.5ⁿ。
- 灵鱼（灵池）按品阶缩短：生长时间 ×0.5^阶（一阶 ×0.5，二阶 ×0.25，三阶 ×0.125，四阶 ×0.0625）。
- 灵泉、火炼池的说明文字同步改成新效果。

相对原版不挨设施的作物：

| 相邻设施 | 原版 | 本插件 |
| --- | --- | --- |
| 0 | 1× | 2× |
| 1 | 2× | 4× |
| 2 | 3× | 8× |
| 3 | 4× | 16× |
| 4 | 5× | 32× |

## 安装

需要先装好 BepInEx 5。把 `ZaohuaFastPasture.dll` 放进 `GodWorld\BepInEx\plugins\`。旧存档里已经种着的作物也会立刻按新倍率生长。

卸载：删掉 dll。已存档的作物保留加速后的数字，直到下次放置、移动建筑或播种时恢复原版。

## 配置

第一次运行后生成 `BepInEx\config\zaohua.fastpasture.cfg`：

| 配置项 | 默认 | 含义 |
| --- | --- | --- |
| BaseGrowTimeMultiplier | 0.5 | 灵草/矿石基础生长时间倍率 |
| FacilityTimeMultiplier | 0.5 | 每个相邻灵泉/火炼池的时间倍率，相乘叠加 |
| FishTimeMultiplierPerTier | 0.5 | 灵鱼每阶的时间倍率 |

## 原理

原版 `PastureImpl.RefreshPastureEffect` 每次都从零算出每块地的 `growTime` 和 `updateGrowSpeed`（灵泉/火炼池各 +100%，相加）。插件在它之后改写这两个值，并在 `RefreshGrow` 前先重算一次，让旧存档立刻生效。

## 编译

把游戏 `Zaohua_Data\Managed` 里的 `Assembly-CSharp.dll`、`UnityEngine.dll`、`UnityEngine.CoreModule.dll` 和 `BepInEx\core` 里的 `BepInEx.dll`、`0Harmony.dll` 复制到 `lib\`，然后：

```
dotnet build src -c Release
dotnet run --project tests   # 离线核对倍率
```

## 许可

MIT
