using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lumina.Excel.Sheets;
using System.Numerics;
using GrandCompany = ECommons.ExcelServices.GrandCompany;
using GrandCompanyRank = Lumina.Excel.Sheets.GrandCompanyRank;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class ExchangeLists : InventoryManagementBase
{
    private static readonly Dictionary<GCExchangeCategoryTab, string> CategoryNames = new()
    {
        [GCExchangeCategoryTab.Weapons] = "武器",
        [GCExchangeCategoryTab.Armor] = "防具",
        [GCExchangeCategoryTab.Materiel] = "軍用品",
        [GCExchangeCategoryTab.Materials] = "素材",
    };

    private ImGuiEx.RealtimeDragDrop<GCExchangeItem> DragDrop = new("GCELDD", x => x.ID);
    public override string Name { get; } = "軍隊籌備／兌換清單";
    private GCExchangeCategoryTab? SelectedCategory = null;
    private GCExchangeCategoryTab? SelectedCategory2 = null;
    private GCExchangeRankTab? SelectedRank = null;
    private Guid SelectedPlanGuid = Guid.Empty;

    public override int DisplayPriority => -5;

    public override void Draw()
    {
        C.AdditionalGCExchangePlans.Where(x => x.GUID == Guid.Empty).Each(x => x.GUID = Guid.NewGuid());
        ImGuiEx.TextWrapped($"""
            選擇軍隊稀有品調度期間要自動購買的物品。
            購買邏輯：
            - 系統會嘗試購買清單中第一個可用物品。
            - 持續購買，直到物品欄中的數量達到指定目標。
            若清單中沒有可購買的物品，或物品欄無法容納：
            - 系統會改為購買探險幣。
            - 會持續購買，直到探險幣達到 65,000 枚。
            探險幣達上限且無法購買其他物品後：
            - 超出的軍票會被捨棄。
            """);

        var selectedPlan = C.AdditionalGCExchangePlans.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selplan", selectedPlan?.DisplayName ?? "預設計畫"))
            {
                if(ImGui.Selectable("預設計畫", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalGCExchangePlans)
                {
                    ImGuiEx.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new GCExchangePlan();
                C.AdditionalGCExchangePlans.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("新增計畫");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultGCExchangePlan).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("複製");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<GCExchangePlan>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();  
                    C.AdditionalGCExchangePlans.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("貼上");
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultGCExchangePlan = selectedPlan.DSFClone();
                    C.DefaultGCExchangePlan.Name = "";
                    C.DefaultGCExchangePlan.GUID.Regenerate();
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("將此計畫設為預設計畫；目前的預設計畫會被覆寫。按住 CTRL 並點擊。");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("刪除此計畫。按住 CTRL 並點擊。");
            }
        });

        if(SelectedPlanGuid == Guid.Empty)
        {
            DrawGCEchangeList(C.DefaultGCExchangePlan);
        }
        else
        {
            if(Data != null)
            {
                if(Data.ExchangePlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"目前角色正在使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("取消指派"))
                    {
                        Data.ExchangePlan = Guid.Empty;
                    }
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, UiBuilder.IconFont, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, $"目前角色未使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("指派"))
                    {
                        Data.ExchangePlan = selectedPlan.GUID;
                    }
                }
                ImGui.SameLine();
            }

            var charas = C.OfflineData.Where(x => x.ExchangePlan == selectedPlan.GUID).ToArray();
            if(charas.Length > 0)
            {
                ImGuiEx.Text($"共有 {charas.Length} 個角色使用");
                ImGuiEx.Tooltip($"{charas.Select(x => x.NameWithWorldCensored)}");
            }
            else
            {
                ImGuiEx.Text($"沒有任何角色使用");
            }

                var planIndex = C.AdditionalGCExchangePlans.IndexOf(x => x.GUID == SelectedPlanGuid);
            if(planIndex == -1)
            {
                SelectedPlanGuid = Guid.Empty;
            }
            else
            {
                DrawGCEchangeList(C.AdditionalGCExchangePlans[planIndex]);
            }
        }
    }

    public void DrawGCEchangeList(GCExchangePlan plan)
    {
        ref string getFilter() => ref Ref<string>.Get($"{plan.ID}filter");
        ref bool onlySelected() => ref Ref<bool>.Get($"{plan.ID}onlySel");
        ref string getFilter2() => ref Ref<string>.Get($"{plan.ID}filter2");

        ImGuiEx.PushID(plan.ID);
        plan.Validate();

        ImGuiEx.InputWithRightButtonsArea("GCPlanSettings", () =>
        {
            if(ReferenceEquals(plan, C.DefaultGCExchangePlan))
            {
                ImGui.BeginDisabled();
                var s = "預設兌換計畫無法重新命名";
                ImGui.InputText("##name", ref s, 1);
                ImGui.EndDisabled();
            }
            else
            {
                ImGui.InputTextWithHint($"##name", "名稱", ref plan.Name, 100);
                ImGuiEx.Tooltip("兌換計畫名稱");
            }
        }, () =>
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt("保留軍票", ref plan.RemainingSeals.ValidateRange(0, 70000), 0, 0);
            ImGuiEx.HelpMarker($"執行購買清單後會保留此數量的軍票。但依角色軍階而定，此數值最多只能設為軍票上限減去 20,000。");
            ImGui.SameLine();
            ImGui.Checkbox("最後以購買物品結束", ref plan.FinalizeByPurchasing);
            ImGuiEx.HelpMarker("啟用後，完成最後一次兌換後會購買物品；否則會等軍票再次達到上限才購買。");
        });

        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##Add Items", "新增物品", ImGuiComboFlags.HeightLarge))
        {
            ImGuiEx.InputWithRightButtonsArea(() =>
            {
                ImGui.InputTextWithHint("##filter2", "搜尋……", ref getFilter2(), 100);
            }, () =>
            {
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo("##cat2", ref SelectedCategory2, names: CategoryNames, nullName: "所有分類");
                ImGuiEx.Tooltip("分類");
            });
            foreach(var x in Utils.SharedGCExchangeListings)
            {
                if(getFilter2().Length > 0
                    && !x.Value.Data.Name.GetText().Contains(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    && !x.Value.Category.ToString().Equals(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[x.Value.MinPurchaseRank].Equals(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory2 != null && x.Value.Category != SelectedCategory2.Value) continue;
                var cont = plan.Items.Select(s => s.ItemID).ToArray();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Value.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.Handle, new(ImGui.GetTextLineHeight()));
                    ImGui.SameLine();
                }
                if(ImGui.Selectable(x.Value.Data.GetName() + $"##{x.Key}", cont.Contains(x.Key), ImGuiSelectableFlags.DontClosePopups))
                {
                    plan.Items.Add(new(x.Key, 0));
                }
            }
            ImGui.EndCombo();
        }
        if(ImGui.BeginPopup("Ex"))
        {
            if(ImGui.Selectable("最佳化武器與防具購買，以取得額外公會點數"))
            {
                List<GCExchangeItem> items = [];
                var qualifyingItems = Utils.SharedGCExchangeListings.Where(x => (x.Value.Category == GCExchangeCategoryTab.Weapons || x.Value.Category == GCExchangeCategoryTab.Armor) && x.Value.Data.GetRarity() == ItemRarity.Green).ToDictionary();
                plan.Items.RemoveAll(x => qualifyingItems.ContainsKey(x.ItemID));
                foreach(var item in qualifyingItems)
                {
                    items.Add(new(item.Key, 0));
                }
                items = items.OrderByDescending(x => (double)Svc.Data.GetExcelSheet<GCSupplyDutyReward>().GetRow(x.Data.Value.LevelItem.RowId).SealsExpertDelivery / (double)Utils.SharedGCExchangeListings[x.ItemID].Seals).ToList();
                foreach(var x in items)
                {
                    plan.Items.Add(x);
                    x.Quantity = Utils.SharedGCExchangeListings[x.ItemID].Data.IsUnique ? 1 : 999;
                }
            }
            ImGuiEx.Tooltip("選擇此選項會將所有可購買的武器與防具加入計畫。這些物品會在購買後立即繳回軍隊，以最大化公會點數；所有項目會放在清單末端，僅在沒有其他可購買項目時才購買。");
            if(ImGui.Selectable("新增所有缺少的物品"))
            {
                foreach(var x in Utils.SharedGCExchangeListings)
                {
                    if(!plan.Items.Any(i => i.ItemID == x.Key))
                    {
                        plan.Items.Add(new(x.Key, 0));
                    }
                }
            }
            if(ImGui.Selectable("將數量重設為 0"))
            {
                plan.Items.Each(x => x.Quantity = 0);
                plan.Items.Each(x => x.QuantitySingleTime = 0);
            }
            if(ImGui.Selectable("移除數量為 0 的物品"))
            {
                plan.Items.RemoveAll(x => x.Quantity == 0 && x.QuantitySingleTime == 0);
            }
            if(ImGuiEx.Selectable("清除清單（按住 CTRL 並點擊）", enabled: ImGuiEx.Ctrl))
            {
                plan.Items.Clear();
            }
            ImGui.EndPopup();
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.AngleDoubleDown, "操作"))
        {
            ImGui.OpenPopup("Ex");
        }
        ImGui.SameLine();
        ImGuiEx.InputWithRightButtonsArea("Fltr2", () =>
        {
            ImGui.InputTextWithHint("##filter", "搜尋……", ref getFilter(), 100);
        }, () =>
        {
            ImGui.Checkbox("僅顯示已選取", ref onlySelected());
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.EnumCombo("##cat", ref SelectedCategory, names: CategoryNames, nullName: "所有分類");
            ImGuiEx.Tooltip("分類");
        });



        DragDrop.Begin();
        if(ImGuiEx.BeginDefaultTable("GCDeliveryList", ["##dragDrop", "~物品", "軍隊", "品級", "價格", "分類", "保留", "一次性", "##controls"]))
        {
            for(var i = 0; i < plan.Items.Count; i++)
            {
                var currentItem = plan.Items[i];
                var meta = Utils.SharedGCExchangeListings[currentItem.ItemID];
                if(onlySelected() && currentItem.Quantity == 0) continue;
                if(getFilter().Length > 0
                    && !meta.Data.Name.GetText().Contains(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !meta.Category.ToString().Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[meta.MinPurchaseRank].Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory != null && meta.Category != SelectedCategory.Value) continue;
                ImGuiEx.PushID(currentItem.ID);
                ImGui.TableNextRow();
                DragDrop.SetRowColor(currentItem);
                ImGui.TableNextColumn();
                DragDrop.NextRow();
                if(ImGuiEx.IconButton(FontAwesomeIcon.AngleDoubleUp))
                {
                    new TickScheduler(() =>
                    {
                        plan.Items.Remove(currentItem);
                        plan.Items.Insert(0, currentItem);
                    });
                }
                ImGui.SameLine(0, 1);
                ImGuiEx.Tooltip("移至最上方");
                DragDrop.DrawButtonDummy(currentItem, plan.Items, i);
                ImGui.TableNextColumn();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(meta.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.Handle, new(ImGui.GetFrameHeight()));
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Data.Name.GetText()}");
                ImGui.TableNextColumn();
                foreach(var c in Enum.GetValues<GrandCompany>().Where(x => x != GrandCompany.Unemployed))
                {
                    if(ThreadLoadImageHandler.TryGetIconTextureWrap(60870 + (int)c, false, out var ctex))
                    {
                        var trans = !meta.Companies.Contains(c);
                        if(trans) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                        ImGui.Image(ctex.Handle, new(ImGui.GetFrameHeight()));
                        if(trans) ImGui.PopStyleVar();
                        ImGuiEx.Tooltip($"{c}" + (trans ? "（不可用）" : ""));
                        ImGui.SameLine(0, 1);
                    }
                }
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Data.LevelItem.RowId}");
                ImGui.TableNextColumn();
                if(Svc.Data.GetExcelSheet<GrandCompanyRank>().TryGetRow(meta.MinPurchaseRank, out var rank) && ThreadLoadImageHandler.TryGetIconTextureWrap(rank.IconFlames, false, out var tex))
                {
                    ImGui.Image(tex.Handle, new(ImGui.GetFrameHeight()));
                    var rankName = Utils.GCRanks[meta.MinPurchaseRank];
                    ImGuiEx.Tooltip(rankName);
                    if(ImGuiEx.HoveredAndClicked()) getFilter() = rankName;
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Seals}");
                ImGui.TableNextColumn();
                ImGuiEx.TextV(CategoryNames.GetValueOrDefault(meta.Category, meta.Category.ToString()));
                if(ImGuiEx.HoveredAndClicked()) getFilter() = meta.Category.ToString();
                ImGui.TableNextColumn();
                if(currentItem.Data.Value.IsUnique)
                {
                    ImGuiEx.Checkbox("唯一", ref currentItem.Quantity);
                }
                else
                {
                    ImGui.SetNextItemWidth(100f.Scale());
                    ImGui.InputInt("##qty", ref currentItem.Quantity.ValidateRange(0, int.MaxValue), 0, 0);
                }
                ImGuiEx.Tooltip("選擇要保留在物品欄中的數量");
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100f.Scale());
                ImGui.InputInt("##qtyonetime", ref currentItem.QuantitySingleTime.ValidateRange(0, currentItem.Data.Value.IsUnique ? 1 : int.MaxValue), 0, 0);
                ImGuiEx.Tooltip("選擇一次性購買數量。任何使用此計畫的角色進行購買時，都會從此數值扣除；歸零後會改用「保留」數量。");
                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Clone))
                {
                    plan.Items.Insert(i + 1, currentItem.JSONClone());
                }
                ImGuiEx.Tooltip("複製此項目。");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    new TickScheduler(() => plan.Items.Remove(currentItem));
                }
                ImGuiEx.Tooltip($"若清單中有多個相同項目則刪除此項；若只有一項則將數量設為 0");
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();
        ImGui.PopID();
    }
}
