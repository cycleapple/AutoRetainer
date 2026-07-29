using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.Reflection;
using ECommons.Throttlers;
using Lumina.Excel.Sheets;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class EntrustManager : InventoryManagementBase
{
    public override string Name { get; } = "委託保管管理器";
    private Guid SelectedGuid = Guid.Empty;
    private string Filter = "";
    private InventoryManagementCommon InventoryManagementCommon = new();

    public override void Draw()
    {
        ImGuiEx.TextWrapped("使用進階委託保管管理器，將指定物品交給指定雇員保管。你可以在此設定計畫，再於雇員設定視窗將計畫指派給雇員。");
        ImGui.Checkbox("啟用", ref C.EnableEntrustManager);
        ImGui.Checkbox("在聊天欄輸出已交付保管的物品", ref C.EnableEntrustChat);
        var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == SelectedGuid);

        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo($"##select", selectedPlan?.Name ?? "選擇計畫……", ImGuiComboFlags.HeightLarge))
            {
                for(var i = 0; i < C.EntrustPlans.Count; i++)
                {
                    var plan = C.EntrustPlans[i];
                    ImGuiEx.PushID(plan.Guid.ToString());
                    if(ImGui.Selectable(plan.Name, plan == selectedPlan))
                    {
                        SelectedGuid = plan.Guid;
                    }
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var plan = new EntrustPlan();
                C.EntrustPlans.Add(plan);
                SelectedGuid = plan.Guid;
                plan.Name = $"委託保管計畫 {C.EntrustPlans.Count}";
            }
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: selectedPlan != null && ImGuiEx.Ctrl))
            {
                C.EntrustPlans.Remove(selectedPlan);
            }
            ImGuiEx.Tooltip("按住 CTRL 並點擊");
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy, enabled: selectedPlan != null))
            {
                Copy(EzConfig.DefaultSerializationFactory.Serialize(selectedPlan, false));
            }
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste, enabled: EzThrottler.Check("ImportPlan")))
            {
                try
                {
                    var plan = EzConfig.DefaultSerializationFactory.Deserialize<EntrustPlan>(Paste()) ?? throw new NullReferenceException();
                    plan.Guid = Guid.NewGuid();
                    if(plan.GetType().GetFieldPropertyUnions(ReflectionHelper.AllFlags).Any(x => x.GetValue(plan) == null)) throw new NullReferenceException();
                    C.EntrustPlans.Add(plan);
                    SelectedGuid = plan.Guid;
                    Notify.Success("已從剪貼簿匯入計畫");
                    EzThrottler.Throttle("ImportPlan", 2000, true);
                }
                catch(Exception e)
                {
                    DuoLog.Error(e.Message);
                }
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint($"##name", "計畫名稱", ref selectedPlan.Name, 100);
            ImGui.Checkbox("交付重複物品", ref selectedPlan.Duplicates);
            ImGuiEx.HelpMarker("模擬遊戲原生的「交付重複物品」功能：將雇員物品欄中已有的物品交付給雇員，直到該物品堆疊滿為止。此功能不影響水晶；明確加入下方清單的物品與分類也不會由此選項處理。");
            ImGui.Indent();
            ImGui.Checkbox("允許建立新的物品堆疊", ref selectedPlan.DuplicatesMultiStack);
            ImGuiEx.HelpMarker("允許「交付重複物品」為選定雇員已有的物品建立新堆疊。");
            ImGui.Unindent();
            ImGui.Checkbox("允許從兵裝庫交付物品", ref selectedPlan.AllowEntrustFromArmory);
            ImGui.Checkbox("僅限手動執行", ref selectedPlan.ManualPlan);
            ImGuiEx.HelpMarker("將此計畫標記為僅限手動執行。只有手動點擊「交付物品」按鈕時才會處理此計畫，絕不會自動執行。");
            ImGui.Checkbox("排除保護清單中的物品", ref selectedPlan.ExcludeProtected);
            ImGui.Separator();
            ImGuiEx.TreeNodeCollapsingHeader($"交付物品分類（已選 {selectedPlan.EntrustCategories.Count} 個）###ecats", () =>
            {
                ImGuiEx.TextWrapped($"可在此選擇要整類交付的物品分類。下方個別選取的物品會排除於這些規則之外。");
                if(ImGui.BeginTable("EntrustTable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.BordersInner))
                {
                    ImGui.TableSetupColumn("##1");
                    ImGui.TableSetupColumn("物品名稱", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("保留數量");
                    ImGui.TableHeadersRow();
                    foreach(var x in Svc.Data.GetExcelSheet<ItemUICategory>())
                    {
                        if(x.Name == "" || x.RowId == 39) continue;
                        var contains = selectedPlan.EntrustCategories.Any(s => s.ID == x.RowId);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Icon, true, out var icon))
                        {
                            ImGui.Image(icon.Handle, new(ImGui.GetFrameHeight()));
                        }
                        ImGui.TableNextColumn();
                        if(ImGui.Checkbox(x.Name.ToString(), ref contains))
                        {
                            if(contains)
                            {
                                selectedPlan.EntrustCategories.Add(new() { ID = x.RowId });
                            }
                            else
                            {
                                selectedPlan.EntrustCategories.RemoveAll(s => s.ID == x.RowId);
                            }
                        }
                        ImGui.TableNextColumn();
                        if(selectedPlan.EntrustCategories.TryGetFirst(s => s.ID == x.RowId, out var result))
                        {
                            ImGui.SetNextItemWidth(130f);
                            ImGui.InputInt($"##amtkeep{result.ID}", ref result.AmountToKeep);
                        }
                    }
                    ImGui.EndTable();
                }
            });
            ImGuiEx.TreeNodeCollapsingHeader($"交付個別物品（已選 {selectedPlan.EntrustItems.Count} 個）###eitems", () =>
            {
                InventoryManagementCommon.DrawListNew(
                    itemId => selectedPlan.EntrustItems.Add(itemId), 
                    itemId => selectedPlan.EntrustItems.Remove(itemId), 
                    selectedPlan.EntrustItems, (x) =>
                {
                    var amount = selectedPlan.EntrustItemsAmountToKeep.SafeSelect(x);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(130f);
                    if(ImGui.InputInt($"##amtkeepitem{x}", ref amount))
                    {
                        selectedPlan.EntrustItemsAmountToKeep[x] = amount;
                    }
                    ImGuiEx.Tooltip("要保留在自身物品欄中的數量");
                });
            });
            ImGuiEx.TreeNodeCollapsingHeader($"快速新增／移除", () =>
            {
                ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), $"此文字顯示期間，將游標移至物品上並按住：");
                ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Shift－新增至交付計畫");
                ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Alt－從交付計畫移除");
                if(Svc.GameGui.HoveredItem > 0)
                {
                    var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
                    if(ImGui.GetIO().KeyShift)
                    {
                        if(!selectedPlan.EntrustItems.Contains(id))
                        {
                            selectedPlan.EntrustItems.Add(id);
                            Notify.Success($"已將 {ExcelItemHelper.GetName(id)} 新增至交付計畫 {selectedPlan.Name}");
                        }
                    }
                    if(ImGui.GetIO().KeyAlt)
                    {
                        if(selectedPlan.EntrustItems.Contains(id))
                        {
                            selectedPlan.EntrustItems.Remove(id);
                            Notify.Success($"已從交付計畫 {selectedPlan.Name} 移除 {ExcelItemHelper.GetName(id)}");
                        }
                    }
                }
            });
        }
    }
}
