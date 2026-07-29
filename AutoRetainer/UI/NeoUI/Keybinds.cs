namespace AutoRetainer.UI.NeoUI;
public class Keybinds : NeoUIEntry
{
    public override string Path => "快捷鍵";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("使用傳喚鈴／管制台時的快捷鍵")
        .Widget("使用傳喚鈴／管制台時，暫時阻止 AutoRetainer 自動啟用", (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.Suppress);
        })
        .Widget("暫時設為「只領取」模式，本輪不指派探險／暫時將探索載具模式設為「僅完成」", (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.TempCollectB);
        })

        .Section("雇員快速操作")
        .Widget("出售道具", (x) => UIUtils.QRA(x, ref C.SellKey))
        .Widget("委託保管道具", (x) => UIUtils.QRA(x, ref C.EntrustKey))
        .Widget("取回道具", (x) => UIUtils.QRA(x, ref C.RetrieveKey))
        .Widget("委託出售", (x) => UIUtils.QRA(x, ref C.SellMarketKey));
}
