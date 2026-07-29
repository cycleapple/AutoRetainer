using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;

namespace AutoRetainer.UI.Windows;

internal unsafe class SubmarineUnlockPlanUI : Window
{
    internal string SelectedPlanGuid = Guid.Empty.ToString();
    internal string SelectedPlanName => VoyageUtils.GetSubmarineUnlockPlanByGuid(SelectedPlanGuid)?.Name ?? "未選擇計畫或計畫無法辨識";
    internal SubmarineUnlockPlan SelectedPlan => VoyageUtils.GetSubmarineUnlockPlanByGuid(SelectedPlanGuid);

    public SubmarineUnlockPlanUI() : base("潛水艇航點解鎖規劃器")
    {
        P.WindowSystem.AddWindow(this);
    }

    internal Dictionary<uint, bool> RouteUnlockedCache = [];
    internal Dictionary<uint, bool> RouteExploredCache = [];
    internal int NumUnlockedSubs = 0;

    internal bool IsMapUnlocked(uint map, bool bypassCache = false)
    {
        if(!IsSubDataAvail()) return false;
        var throttle = $"Voyage.MapUnlockedCheck.{map}";
        if(!bypassCache && RouteUnlockedCache.TryGetValue(map, out var val) && !EzThrottler.Check(throttle))
        {
            return val;
        }
        else
        {
            EzThrottler.Throttle(throttle, 2500, true);
            RouteUnlockedCache[map] = HousingManager.IsSubmarineExplorationUnlocked((byte)map);
            return RouteUnlockedCache[map];
        }
    }

    internal bool IsMapExplored(uint map, bool bypassCache = false)
    {
        if(!IsSubDataAvail()) return false;
        var throttle = $"Voyage.MapExploredCheck.{map}";
        if(!bypassCache && RouteExploredCache.TryGetValue(map, out var val) && !EzThrottler.Check(throttle))
        {
            return val;
        }
        else
        {
            EzThrottler.Throttle(throttle, 2500, true);
            RouteExploredCache[map] = HousingManager.IsSubmarineExplorationExplored((byte)map);
            return RouteExploredCache[map];
        }
    }

    internal int? GetNumUnlockedSubs()
    {
        if(!IsSubDataAvail()) return null;
        NumUnlockedSubs = 1 + Unlocks.PointToUnlockPoint.Where(x => x.Value.Sub).Where(x => IsMapExplored(x.Key)).Count();
        return NumUnlockedSubs;
    }

    internal bool IsSubDataAvail()
    {
        if(HousingManager.Instance()->WorkshopTerritory == null) return false;
        if(HousingManager.Instance()->WorkshopTerritory->Submersible.Data.Length == 0) return false;
        if(HousingManager.Instance()->WorkshopTerritory->Submersible.Data[0].Name[0] == 0) return false;
        return true;
    }

    internal int GetAmountOfOtherPlanUsers(string guid)
    {
        var i = 0;
        C.OfflineData.Where(x => x.CID != Player.CID).Each(x => i += x.AdditionalSubmarineData.Count(a => a.Value.SelectedUnlockPlan == guid));
        return i;
    }

    public override void Draw()
    {
        C.SubmarineUnlockPlans.RemoveAll(x => x.Delete);
        ImGuiEx.InputWithRightButtonsArea("SUPSelector", () =>
        {
            if(ImGui.BeginCombo("##supsel", SelectedPlanName, ImGuiComboFlags.HeightLarge))
            {
                foreach(var x in C.SubmarineUnlockPlans)
                {
                    if(ImGui.Selectable(x.Name + $"##{x.GUID}"))
                    {
                        SelectedPlanGuid = x.GUID;
                    }
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGui.Button("新增計畫"))
            {
                var x = new SubmarineUnlockPlan();
                x.Name = $"計畫 {x.GUID}";
                C.SubmarineUnlockPlans.Add(x);
                SelectedPlanGuid = x.GUID;
            }
        });
        ImGui.Separator();
        if(SelectedPlan == null)
        {
            ImGuiEx.Text($"未選擇計畫或計畫無法辨識");
        }
        else
        {
            if(Data != null)
            {
                var users = GetAmountOfOtherPlanUsers(SelectedPlanGuid);
                var my = Data.AdditionalSubmarineData.Where(x => x.Value.SelectedUnlockPlan == SelectedPlanGuid);
                if(users == 0)
                {
                    if(!my.Any())
                    {
                        ImGuiEx.TextWrapped($"目前沒有任何潛水艇使用此計畫。");
                    }
                    else
                    {
                        ImGuiEx.TextWrapped($"此計畫由 {my.Select(X => X.Key).Print()} 使用。");
                    }
                }
                else
                {
                    if(!my.Any())
                    {
                        ImGuiEx.TextWrapped($"其他角色有 {users} 艘潛水艇使用此計畫。");
                    }
                    else
                    {
                        ImGuiEx.TextWrapped($"此計畫由 {my.Select(X => X.Key).Print()} 及其他角色的另外 {users} 艘潛水艇使用。");
                    }
                }
            }
            if(C.DefaultSubmarineUnlockPlan == SelectedPlanGuid)
            {
                ImGuiEx.Text($"此計畫已設為預設計畫。");
                ImGui.SameLine();
                if(ImGui.SmallButton("重設")) C.DefaultSubmarineUnlockPlan = "";
            }
            else
            {
                if(ImGui.SmallButton("將此計畫設為預設計畫")) C.DefaultSubmarineUnlockPlan = SelectedPlanGuid;
            }
            ImGuiEx.TextV("名稱：");
            ImGui.SameLine();
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputText($"##planname", ref SelectedPlan.Name, 100);
            ImGuiEx.LineCentered($"planbuttons", () =>
            {
                ImGuiEx.TextV($"套用此計畫至：");
                ImGui.SameLine();
                if(ImGui.Button("所有潛水艇"))
                {
                    C.OfflineData.Each(x => x.AdditionalSubmarineData.Each(s => s.Value.SelectedUnlockPlan = SelectedPlanGuid));
                }
                ImGui.SameLine();
                if(ImGui.Button("目前角色的潛水艇"))
                {
                    Data.AdditionalSubmarineData.Each(s => s.Value.SelectedUnlockPlan = SelectedPlanGuid);
                }
                ImGui.SameLine();
                if(ImGui.Button("不套用至任何潛水艇"))
                {
                    C.OfflineData.Each(x => x.AdditionalSubmarineData.Where(s => s.Value.SelectedUnlockPlan == SelectedPlanGuid).Each(s => s.Value.SelectedUnlockPlan = Guid.Empty.ToString()));
                }
            });
            ImGuiEx.LineCentered($"planbuttons2", () =>
            {
                if(ImGui.Button($"複製計畫設定"))
                {
                    Copy(JsonConvert.SerializeObject(SelectedPlan));
                }
                ImGui.SameLine();
                if(ImGui.Button($"貼上計畫設定"))
                {
                    try
                    {
                        SelectedPlan.CopyFrom(JsonConvert.DeserializeObject<SubmarineUnlockPlan>(Paste()));
                    }
                    catch(Exception ex)
                    {
                        DuoLog.Error($"Could not import plan: {ex.Message}");
                        ex.Log();
                    }
                }
                ImGui.SameLine();
                if(ImGuiEx.ButtonCtrl("刪除此計畫"))
                {
                    SelectedPlan.Delete = true;
                }
                ImGui.SameLine();
                if(ImGui.Button($"說明"))
                {
                    Svc.Chat.Print($"此處列出所有可解鎖航點。插件需要選擇解鎖目標時，會從清單中選取第一個可用目的地。請注意，不能只指定最終解鎖航點；必須選取沿途的所有目的地。");
                }
            });
            if(ImGui.BeginChild("Plan"))
            {
                if(!IsSubDataAvail())
                {
                    ImGuiEx.TextWrapped($"請開啟潛水艇清單以取得資料。");
                }
                ImGui.Checkbox($"解鎖潛水艇欄位。目前欄位：{GetNumUnlockedSubs()?.ToString() ?? "未知"}/4", ref SelectedPlan.UnlockSubs);
                ImGuiEx.TextWrapped($"解鎖潛水艇欄位的優先級一律高於解鎖航線。");
                ImGui.Checkbox("在沉沒海模式強制重複單一目的地。", ref SelectedPlan.EnforceDSSSinglePoint);
                ImGui.Checkbox("強制執行此計畫。", ref SelectedPlan.EnforcePlan);
                ImGuiEx.HelpMarker("此地圖中選取的所有解鎖航點，會由每艘符合條件的潛水艇持續執行，直到全部實際解鎖。");
                if(ImGui.BeginTable("##planTable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("航點", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("海域");
                    ImGui.TableSetupColumn("由此航點解鎖");
                    ImGui.TableHeadersRow();
                    foreach(var x in Unlocks.PointToUnlockPoint)
                    {
                        if(x.Value.Point < 9000)
                        {
                            ImGuiEx.PushID($"{x.Key}");
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            var data = Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(x.Key);
                            if(data != null)
                            {
                                try
                                {
                                    var col = IsMapUnlocked(x.Key);
                                    ImGuiEx.CollectionCheckbox($"{data?.FancyDestination()}", x.Key, SelectedPlan.ExcludedRoutes, true);
                                    if(col) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedGreen);
                                    if(col) ImGui.PopStyleColor();
                                    ImGui.TableNextColumn();
                                    ImGuiEx.TextV($"{data?.Map.ValueNullable?.Name}");
                                    ImGui.TableNextColumn();
                                    var notEnabled = !SelectedPlan.ExcludedRoutes.Contains(x.Key) && SelectedPlan.ExcludedRoutes.Contains(x.Value.Point);
                                    ImGuiEx.TextV(notEnabled ? ImGuiColors.DalamudRed : null, $"{Svc.Data.GetExcelSheet<SubmarineExploration>().GetRowOrDefault(x.Value.Point)?.FancyDestination()}");
                                }
                                catch(Exception e)
                                {
                                    e.Log();
                                }
                            }
                            ImGui.PopID();
                        }
                    }
                    ImGui.EndTable();
                }
                if(ImGui.CollapsingHeader("顯示目前航點探索順序"))
                {
                    ImGuiEx.Text(SelectedPlan.GetPrioritizedPointList().Select(x => $"{Svc.Data.GetExcelSheet<SubmarineExploration>().GetRow(x.point).Destination} ({x.justification})").Join("\n"));
                }
            }
            ImGui.EndChild();
        }
    }
}
