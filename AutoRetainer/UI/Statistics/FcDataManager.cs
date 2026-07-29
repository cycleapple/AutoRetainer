using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.Statistics;
public sealed class FcDataManager
{
    private FcDataManager() { }

    public void Draw()
    {
        ImGui.Checkbox($"每 30 小時更新", ref C.UpdateStaleFCData);
        ImGui.SameLine();
        if(ImGuiEx.Button("更新", Player.Interactable))
        {
            S.FCPointsUpdater.ScheduleUpdateIfNeeded(true);
        }
        ImGui.SameLine();
        ImGui.Checkbox($"僅顯示錢包公會", ref C.DisplayOnlyWalletFC);
        if(ImGui.BeginTable("FCData", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn($"名稱", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn($"角色");
            ImGui.TableSetupColumn($"金幣");
            ImGui.TableSetupColumn($"公會點數");
            ImGui.TableSetupColumn($"##control");
            ImGui.TableHeadersRow();

            var totalGil = 0L;
            var totalPoint = 0L;

            var i = 0;
            foreach(var x in C.FCData)
            {
                if(x.Key == 0) continue;
                if(!x.Value.GilCountsTowardsChara && C.DisplayOnlyWalletFC) continue;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(C.NoNames ? $"公會 {++i}" : x.Value.Name);

                ImGui.TableNextColumn();
                foreach(var c in C.OfflineData.Where(z => z.FCID == x.Key))
                {
                    ImGuiEx.Text(x.Value.HolderChara == c.CID && x.Value.GilCountsTowardsChara ? EColor.GreenBright : null, Censor.Character(c.Name, c.World));
                    if(ImGuiEx.HoveredAndClicked("左鍵點擊：重新登入此角色"))
                    {
                        Svc.Commands.ProcessCommand($"/ays relog {c.Name}@{c.World}");
                    }
                    if(x.Value.GilCountsTowardsChara)
                    {
                        if(ImGuiEx.HoveredAndClicked("右鍵點擊：設為金幣持有角色", ImGuiMouseButton.Right))
                        {
                            x.Value.HolderChara = c.CID;
                        }
                    }
                }

                ImGui.TableNextColumn();
                if(x.Value.LastGilUpdate != -1 && x.Value.LastGilUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.Gil:N0}");
                    totalGil += x.Value.Gil;
                    ImGuiEx.Tooltip($"上次更新：{UpdatedWhen(x.Value.LastGilUpdate)}。Ctrl + 點擊以重設");
                    if(ImGuiEx.HoveredAndClicked() && ImGuiEx.Ctrl)
                    {
                        x.Value.LastGilUpdate = -1;
                        x.Value.Gil = 0;
                    }
                }
                else
                {
                    ImGuiEx.Text($"未知");
                }

                ImGui.TableNextColumn();
                if(x.Value.FCPointsLastUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.FCPoints:N0}");
                    totalPoint += x.Value.FCPoints;
                    ImGuiEx.Tooltip($"上次更新：{UpdatedWhen(x.Value.FCPointsLastUpdate)}");
                }
                else
                {
                    ImGuiEx.Text($"未知");
                }

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox($"\uf555##FC{x.Key}", ref x.Value.GilCountsTowardsChara, EColor.Green);
                ImGui.PopFont();
                ImGuiEx.Tooltip("將此公會標記為錢包公會。「金幣」頁籤會計入此公會的金幣。");
                ImGui.SameLine();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"{x.Key}Dele", enabled: ImGuiEx.Ctrl))
                {
                    new TickScheduler(() => C.FCData.Remove(x));
                }

                ImGuiEx.Tooltip($"按住 CTRL 並點擊即可刪除此公會。重新登入該公會所屬角色後，它會再次出現。");
            }

            ImGui.TableNextRow();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, EColor.GreenDark.ToUint());
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, EColor.GreenDark.ToUint());
            ImGui.TableNextColumn();
            ImGuiEx.Text($"總計");
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGuiEx.Text($"{totalGil:N0}");
            ImGui.TableNextColumn();
            ImGuiEx.Text($"{totalPoint:N0}");

            ImGui.EndTable();
        }


        string UpdatedWhen(long time)
        {
            var diff = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
            if(diff < 1000L * 60) return "剛剛";
            if(diff < 1000L * 60 * 60) return $"{(int)(diff / 1000 / 60)} 分鐘前";
            if(diff < 1000L * 60 * 60 * 60) return $"{(int)(diff / 1000 / 60 / 60)} 小時前";
            return $"{(int)(diff / 1000 / 60 / 60 / 24)} 天前";
        }
    }

    public OfflineCharacterData GetHolderChara(ulong fcid, FCData data)
    {
        if(C.OfflineData.TryGetFirst(x => x.FCID == fcid && x.CID == data.HolderChara, out var chara))
        {
            return chara;
        }
        else if(C.OfflineData.TryGetFirst(x => x.FCID == fcid, out var fchara))
        {
            data.HolderChara = fchara.CID;
            return fchara;
        }
        return null;
    }
}
