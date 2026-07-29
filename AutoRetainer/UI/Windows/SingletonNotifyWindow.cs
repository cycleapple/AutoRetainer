namespace AutoRetainer.UI.Windows;
public class SingletonNotifyWindow : NotifyWindow
{
    private bool IAmIdiot = false;
    private WindowSystem ws;
    public SingletonNotifyWindow() : base("AutoRetainer－警告！")
    {
        IsOpen = true;
        ws = new();
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        ws.AddWindow(this);
    }

    public override void OnClose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
    }

    public override void DrawContent()
    {
        ImGuiEx.Text($"AutoRetainer 偵測到另一個插件執行個體正使用相同的資料路徑。");
        ImGuiEx.Text($"為避免資料遺失，已停止載入插件。");
        if(ImGui.Button("關閉此視窗且不載入 AutoRetainer"))
        {
            IsOpen = false;
        }
        if(ImGui.Button("瞭解如何正確執行兩個以上的遊戲執行個體"))
        {
            ShellStart("https://github.com/PunishXIV/AutoRetainer/issues/62");
        }
        ImGui.Separator();
        ImGui.Checkbox($"我瞭解這可能會導致所有 AutoRetainer 資料遺失", ref IAmIdiot);
        if(!IAmIdiot) ImGui.BeginDisabled();
        if(ImGui.Button("載入 AutoRetainer"))
        {
            IsOpen = false;
            new TickScheduler(P.Load);
        }
        if(!IAmIdiot) ImGui.EndDisabled();
    }
}
