using System.Collections.Generic;
using System.Windows;                              
using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Models;
using System.Reflection;                               // ✅ ProductionLinesPage
using System.Linq;
using FMSFrontend.Features.Services; // needed for FirstOrDefault
using System.Collections.ObjectModel;
using System;
using System.Windows.Threading;
using System.Diagnostics;

namespace FMSFrontend.Features.Singleton
{
    public partial class StorageStore : ObservableObject
    {
        [ObservableProperty] public StorageGroupModel storageGroup = new();
        public void ApplyStorageGroupDto(StorageGroupModel data)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                // 群組統計
                StorageGroup.WaitingTotal = data.WaitingTotal;
                StorageGroup.ProcessingTotal = data.ProcessingTotal;
                StorageGroup.ErrorTotal = data.ErrorTotal;
                StorageGroup.CompletedTotal = data.CompletedTotal;
                StorageGroup.RestrictionTotal = data.RestrictionTotal;
                StorageGroup.BookedTotal = data.BookedTotal;

                // 整體清單直接替換
                StorageGroup.Storage = data.Storage;

                // 對齊選取的 Storage 參考
                if (data.SelectStorage != null)
                {
                    // 優先指向清單內相同項目，否則就用來源的 SelectStorage
                    var fromSel = data.SelectStorage;
                    var matched = StorageGroup.Storage?
                        .FirstOrDefault(s => s.Name == fromSel.Name && s.Number == fromSel.Number)
                        ?? StorageGroup.Storage?.FirstOrDefault(s => s.Title == fromSel.Title)
                        ?? fromSel;

                    StorageGroup.SelectStorage = matched;
                }
                else if (StorageGroup.SelectStorage != null)
                {
                    // 嘗試以目前選擇在新清單中找到對應實例
                    var cur = StorageGroup.SelectStorage;
                    var matched = StorageGroup.Storage?
                        .FirstOrDefault(s => s.Name == cur.Name && s.Number == cur.Number)
                        ?? StorageGroup.Storage?.FirstOrDefault(s => s.Title == cur.Title);
                    if (matched != null)
                        StorageGroup.SelectStorage = matched;
                }

                if (StorageGroup.SelectStorage != null)
                {
                    var target = StorageGroup.SelectStorage;
                    var source = data.SelectStorage ?? target; // 若無來源選擇，使用目前選擇做就地刷新

                    // 同步基本屬性與統計
                    target.Name = source.Name;
                    target.Number = source.Number;
                    target.Serial = source.Serial;
                    target.Rows = source.Rows;
                    target.Columns = source.Columns;

                    target.WaitingCount = source.WaitingCount;
                    target.ProcessingCount = source.ProcessingCount;
                    target.ErrorCount = source.ErrorCount;
                    target.CompletedCount = source.CompletedCount;
                    target.RestrictionCount = source.RestrictionCount;
                    target.BookedCount = source.BookedCount;

                    // 對齊 Slots 長度
                    var targetSlots = target.Slots ?? new ObservableCollection<Slot>();
                    var sourceSlots = source.Slots ?? new ObservableCollection<Slot>();

                    if (target.Slots != targetSlots)
                        target.Slots = targetSlots;

                    if (targetSlots.Count != sourceSlots.Count)
                    {
                        targetSlots.Clear();
                        foreach (var s in sourceSlots)
                            targetSlots.Add(s);
                    }
                    else
                    {
                        // 就地更新（Slot 為可通知屬性）
                        for (int i = 0; i < sourceSlots.Count; i++)
                        {
                            var t = targetSlots[i];
                            var s = sourceSlots[i];
                            t.Name = s.Name;
                            t.ShortName = s.ShortName;
                            t.Serial = s.Serial;
                            t.Kind = s.Kind;
                            t.MaterialStatus = s.MaterialStatus;
                            t.MaterialRestriction = s.MaterialRestriction;
                            t.StorageStatus = s.StorageStatus;
                            t.StorageRestriction = s.StorageRestriction;
                        }
                    }
                }
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
