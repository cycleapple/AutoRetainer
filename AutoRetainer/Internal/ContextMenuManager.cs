using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.ChatMethods;
using ECommons.ExcelServices;
using ECommons.EzContextMenu;
using ECommons.Interop;
using Lumina.Excel.Sheets;
using UIColor = ECommons.ChatMethods.UIColor;

namespace AutoRetainer.Internal;

internal unsafe class ContextMenuManager
{
    private SeString Prefix = new SeStringBuilder().AddUiForeground(" ", 539).Build();

    public ContextMenuManager()
    {
        ContextMenuPrefixRemover.Initialize();
        Svc.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        if(Data == null) return;
        if(!Data.GetIMSettings().IMEnableContextMenu) return;
        if(args.MenuType == ContextMenuType.Inventory && args.Target is MenuTargetInventory inv && inv.TargetItem != null)
        {
            var id = inv.TargetItem.Value.ItemId % 1_000_000;
            if(id != 0 && inv.TargetItem.Value.ItemId < 2_000_000)
            {
                if(Data.GetIMSettings(true).IMProtectList.Contains(id))
                {
                    args.AddMenuItem(new MenuItem()
                    {
                        Name = new SeStringBuilder().Append(Prefix).AddText("= Item has been protected =").Build(),
                        OnClicked = (a) =>
                        {
                            if(IsKeyPressed([LimitedKeys.LeftControlKey, LimitedKeys.RightControlKey]) && IsKeyPressed([LimitedKeys.RightShiftKey, LimitedKeys.LeftShiftKey]))
                            {
                                var t = $"Item {ExcelItemHelper.GetName(id)} removed from protection list";
                                Notify.Success(t);
                                ChatPrinter.Red("[AutoRetainer] " + t);
                                Data.GetIMSettings(true).IMProtectList.Remove(id);
                            }
                            else
                            {
                                Notify.Error("按住 Ctrl＋Shift 並點擊，才能解除物品保護");
                            }
                        }
                    }.RemovePrefix());
                }
                else
                {
                    var data = Svc.Data.GetExcelSheet<Item>().GetRow(id);
                    if(Data.GetIMSettings(true).IMAutoVendorSoft.Contains(id))
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from Quick Venture sell list", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                Data.GetIMSettings(true).IMAutoVendorSoft.Remove(id);
                                Notify.Info($"已從快速探險出售清單移除 {ExcelItemHelper.GetName(id)}");
                            }
                        }.RemovePrefix());
                    }
                    else if(data.PriceLow > 0)
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to Quick Venture sell list", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                if(Data.GetIMSettings(true).AddItemToList(IMListKind.SoftSell, id, out var error))
                                {
                                    Notify.Success($"已將 {ExcelItemHelper.GetName(id)} 加入快速探險出售清單");
                                }
                                else
                                {
                                    Notify.Error(error);
                                }
                            }
                        }.RemovePrefix());
                    }

                    if(Data.GetIMSettings(true).IMAutoVendorHard.Contains(id))
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from Unconditional sell list", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                Data.GetIMSettings(true).IMAutoVendorHard.Remove(id);
                                Notify.Success($"已從無條件出售清單移除 {ExcelItemHelper.GetName(id)}");
                            }
                        }.RemovePrefix());
                    }
                    else if(data.PriceLow > 0)
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to Unconditional sell list", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                if(Data.GetIMSettings(true).AddItemToList(IMListKind.HardSell, id, out var error))
                                {
                                    Notify.Success($"已將 {ExcelItemHelper.GetName(id)} 加入無條件出售清單");
                                }
                                else
                                {
                                    Notify.Error(error);
                                }
                            }
                        }.RemovePrefix());
                    }

                    if(Data.GetIMSettings(true).IMDiscardList.Contains(id))
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- Remove from Discard list", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                Data.GetIMSettings(true).IMDiscardList.Remove(id);
                                Notify.Success($"已從丟棄清單移除 {ExcelItemHelper.GetName(id)}");
                            }
                        }.RemovePrefix());
                    }
                    else
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ Add to Discard list", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                if(Data.GetIMSettings(true).AddItemToList(IMListKind.Discard, id, out var error))
                                {
                                    Notify.Success($"已將 {ExcelItemHelper.GetName(id)} 加入丟棄清單");
                                }
                                else
                                {
                                    Notify.Error(error);
                                }
                            }
                        }.RemovePrefix());
                    }

                    args.AddMenuItem(new MenuItem()
                    {
                        Name = new SeStringBuilder().Append(Prefix).AddText("Protect item from auto actions").Build(),
                        OnClicked = (a) =>
                        {
                            if(Data.GetIMSettings(true).AddItemToList(IMListKind.Protect, id, out var error))
                            {
                                Notify.Success($"{ExcelItemHelper.GetName(id)} added to protection list");
                            }
                            else
                            {
                                Notify.Error(error);
                            }
                        }
                    }.RemovePrefix());
                }
            }
        }
    }

    public void Dispose()
    {
        Svc.ContextMenu.OnMenuOpened -= ContextMenu_OnMenuOpened;
    }
}
