using AutoRetainer.Modules.Voyage;
using AutoRetainer.UI.MainWindow.MultiModeTab;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.Configuration;
using ECommons.Funding;
using NightmareUI;

namespace AutoRetainer.UI.MainWindow;

internal unsafe class AutoRetainerWindow : Window
{
    private TitleBarButton LockButton;

    public AutoRetainerWindow() : base($"")
    {
        PatreonBanner.IsOfficialPlugin = () => true;
        LockButton = new()
        {
            Click = OnLockButtonClick,
            Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
            IconOffset = new(3, 2),
            ShowTooltip = () => ImGui.SetTooltip("鎖定視窗位置與大小"),
        };
        SizeConstraints = new()
        {
            MinimumSize = new(250, 100),
            MaximumSize = new(9999, 9999)
        };
        P.WindowSystem.AddWindow(this);
        AllowPinning = false;
        TitleBarButtons.Add(new()
        {
            Click = (m) => { if(m == ImGuiMouseButton.Left) S.NeoWindow.IsOpen = true; },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("開啟設定視窗"),
        });
        TitleBarButtons.Add(LockButton);
    }

    private Action<string> SomeAction;

    private void OnLockButtonClick(ImGuiMouseButton m)
    {
        SomeAction += (s) => { };
        SomeAction -= (s) => { };
        if(m == ImGuiMouseButton.Left)
        {
            C.PinWindow = !C.PinWindow;
            LockButton.Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        }
    }

    public override void PreDraw()
    {
        var prefix = SchedulerMain.PluginEnabled ? $" [{SchedulerMain.Reason}]" : "";
        var tokenRem = TimeSpan.FromMilliseconds(Utils.GetRemainingSessionMiliSeconds());
        WindowName = $"{P.Name} {P.GetType().Assembly.GetName().Version}{prefix} | {FormatToken(tokenRem)}###AutoRetainer";
        if(C.PinWindow)
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(C.WindowPos);
            ImGui.SetNextWindowSize(C.WindowSize);
        }
    }

    private string FormatToken(TimeSpan time)
    {
        if(time.TotalMilliseconds > 0)
        {
            if(time.Days > 0)
            {
                return $"工作階段將於 {time.Days} 天後到期" + (time.Hours > 0 ? $" {time.Hours} 小時" : "");
            }
            else
            {
                if(time.Hours > 0)
                {
                    return $"工作階段將於 {time.Hours} 小時後到期";
                }
                else
                {
                    return $"工作階段將於一小時內到期";
                }
            }
        }
        else
        {
            return "工作階段已到期";
        }
    }
    public override void Draw()
    {
        //ImGuiEx.Text(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "This version MUST NOT BE RUNNING UNATTENDED.");
        if(!C.AcceptedDisclamer)
        {
            new NuiBuilder()
                .Section("免責聲明")
                .TextWrapped(ImGuiColors.DalamudYellow, "嚴禁將 AutoRetainer 用於現金交易（RMT）。")
                .TextWrapped(ImGuiColors.DalamudRed, "為避免不必要的後果，使用 AutoRetainer 時請遵守以下規則：")
                .TextWrapped("1. 請勿在遊戲聊天中承認使用 AutoRetainer；")
                .TextWrapped("2. 請勿長時間讓 AutoRetainer 在無人照看時執行；")
                .TextWrapped("3. 每日遊玩與 AutoRetainer 使用時間合計請勿超過 16 小時；檢查雇員／潛水艇的流程之間應保留未活動時段；")
                .TextWrapped("4. 請勿回覆透過交易或聊天進行所謂外掛檢查的玩家；請立即將這些玩家加入黑名單；")
                .TextWrapped("5. 若遭遊戲管理員詢問，請一律聲稱所有操作皆為手動，切勿承認使用插件。")
                .TextWrapped("未遵守上述原則可能使你的帳號面臨風險。")
                .TextWrapped(GradientColor.Get(ImGuiColors.DalamudYellow, ImGuiColors.DalamudRed), "不得將 AutoRetainer 用於現金交易或其他商業用途；此類用途不提供任何支援。")
                .Widget(() =>
                {
                    if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Check, "接受並繼續"))
                    {
                        C.AcceptedDisclamer = true;
                        EzConfig.Save();
                    }
                })
                .Draw();
            return;
        }
        var e = SchedulerMain.PluginEnabledInternal;
        var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;

        if(disabled)
        {
            ImGui.BeginDisabled();
        }
        if(ImGui.Checkbox($"啟用 {P.Name}", ref e))
        {
            P.WasEnabled = false;
            if(e)
            {
                SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
            }
            else
            {
                SchedulerMain.DisablePlugin();
            }
        }
        if(C.ShowDeployables && (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) || VoyageScheduler.Enabled))
        {
            ImGui.SameLine();
            ImGui.Checkbox($"探索載具", ref VoyageScheduler.Enabled);
        }
        if(disabled)
        {
            ImGui.EndDisabled();
            ImGuiComponents.HelpMarker($"此選項由多角色模式控制。按住 CTRL 可強制變更。");
        }

        if(P.WasEnabled)
        {
            ImGui.SameLine();
            ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudGrey, ImGuiColors.DalamudGrey3, 500), $"已暫停");
        }

        ImGui.SameLine();
        if(ImGui.Checkbox("多角色", ref MultiMode.Enabled))
        {
            MultiMode.OnMultiModeEnabled();
        }
        if(C.ShowNightMode)
        {
            ImGui.SameLine();
            if(ImGui.Checkbox("夜間", ref C.NightMode))
            {
                MultiMode.BailoutNightMode();
            }
        }
        if(C.DisplayMMType)
        {
            ImGui.SameLine();
            ImGuiEx.SetNextItemWidthScaled(100f);
            ImGuiEx.EnumCombo("##mode", ref C.MultiModeType);
        }
        if(C.CharEqualize && MultiMode.Enabled)
        {
            ImGui.SameLine();
            if(ImGui.Button("重設計數器"))
            {
                MultiMode.CharaCnt.Clear();
            }
        }

        Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

        if(IPC.Suppressed)
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, $"插件運作遭其他插件抑制。");
            ImGui.SameLine();
            if(ImGui.SmallButton("取消"))
            {
                IPC.Suppressed = false;
            }
        }

        if(P.TaskManager.IsBusy)
        {
            ImGui.SameLine();
            if(ImGui.Button($"中止 {P.TaskManager.NumQueuedTasks} 個工作"))
            {
                P.TaskManager.Abort();
            }
        }

        PatreonBanner.DrawRight();
        ImGuiEx.EzTabBar("tabbar", PatreonBanner.Text,
                        ("雇員", MultiModeUI.Draw, null, true),
                        ("探索載具", WorkshopUI.Draw, null, true),
                        ("疑難排解", TroubleshootingUI.Draw, null, true),
                        ("統計", DrawStats, null, true),
                        ("關於", CustomAboutTab.Draw, null, true)
                        );
        if(!C.PinWindow)
        {
            C.WindowPos = ImGui.GetWindowPos();
            C.WindowSize = ImGui.GetWindowSize();
        }
    }

    private void DrawStats()
    {
        NuiTools.ButtonTabs([[C.RecordStats ? new("委託", S.VentureStats.DrawVentures) : null, new("金幣", S.GilDisplay.Draw), new("公會資料", S.FCData.Draw)]]);
    }

    public override void OnClose()
    {
        EzConfig.Save();
        S.VentureStats.Data.Clear();
        MultiModeUI.JustRelogged = false;
    }

    public override void OnOpen()
    {
        MultiModeUI.JustRelogged = true;
    }
}
