using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class StorageStore : ObservableObject
    {
        [ObservableProperty] public StorageGroupModel storageGroup = new();
        [ObservableProperty] public DateTime updataTime = new();


        public void ApplyStorageDto(List<StorageDto> dtos)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dtos.ApplyStorageDto(StorageGroup.Storage);
            if (disp != null && !disp.CheckAccess())
            {
                // disp.Invoke(apply);
            }
            else apply();
        }
        public void ApplyNullDto(int storageIndex, int slotIindex)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                if (StorageGroup.Storage[storageIndex].Slots[slotIindex] == null) return;
                StorageGroup.Storage[storageIndex].Slots[slotIindex].Name = "";
                StorageGroup.Storage[storageIndex].Slots[slotIindex].ShortName = "";
                StorageGroup.Storage[storageIndex].Slots[slotIindex].MaterialStatus = "";
                StorageGroup.Storage[storageIndex].Slots[slotIindex].MaterialRestriction = false;
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();

        }
        public void ApplyElectrodeDto(ElectrodeDto dto, int storageIndex, int slotIindex)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dto.ApplyElectrodeDto(StorageGroup.Storage[storageIndex].Slots[slotIindex]);
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }

        public void ApplyWorkpieceDto(WorkpieceDto dto, int storageIndex, int slotIindex)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dto.ApplyWorkpieceDto(StorageGroup.Storage[storageIndex].Slots[slotIindex]);
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyProbeDto(ProbeDto dtos, int storageIndex, int slotIindex)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dtos.ApplyProbeDto(StorageGroup.Storage[storageIndex].Slots[slotIindex]);
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplyStatusCount()
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                for (int i = 0; i < StorageGroup.Storage.Count; i++)
                {
                    //計算總數
                    StorageGroup.Storage[i].WaitingCount = StorageGroup.Storage[i].Slots.Count(s => s.MaterialStatus == "Verified");
                    StorageGroup.Storage[i].ProcessingCount = StorageGroup.Storage[i].Slots.Count(s => s.MaterialStatus == "Working");
                    StorageGroup.Storage[i].ErrorCount = StorageGroup.Storage[i].Slots.Count(s => s.MaterialStatus == "Error");
                    StorageGroup.Storage[i].CompletedCount = StorageGroup.Storage[i].Slots.Count(s => s.MaterialStatus == "Completed");
                    StorageGroup.Storage[i].FailCount = StorageGroup.Storage[i].Slots.Count(s => s.MaterialStatus == "Failure" || s.MaterialStatus == "Faulty");
                    StorageGroup.Storage[i].RestrictionCount = StorageGroup.Storage[i].Slots.Count(s => s.StorageRestriction == true);
                    StorageGroup.Storage[i].BookedCount = StorageGroup.Storage[i].Slots.Count(s => s.StorageStatus == "Booked");
                }
                StorageGroup.WaitingTotal = StorageGroup.Storage.Sum(s => s.WaitingCount);
                StorageGroup.CompletedTotal = StorageGroup.Storage.Sum(s => s.CompletedCount);
                StorageGroup.FailTotal = StorageGroup.Storage.Sum(s => s.FailCount);
                StorageGroup.ErrorTotal = StorageGroup.Storage.Sum(s => s.ErrorCount);
                StorageGroup.ProcessingTotal = StorageGroup.Storage.Sum(s => s.ProcessingCount);
                StorageGroup.RestrictionTotal = StorageGroup.Storage.Sum(s => s.RestrictionCount);
                StorageGroup.BookedTotal = StorageGroup.Storage.Sum(s => s.BookedCount);

            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
        public void ApplySelectStorage(string SelectTitle)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                if (string.IsNullOrWhiteSpace(SelectTitle) && StorageGroup.Storage.Count > 0)
                {
                    SelectTitle = StorageGroup.Storage[0].Title;
                }
                if (!string.IsNullOrWhiteSpace(SelectTitle))
                {
                    var matched = StorageGroup.Storage.FirstOrDefault(s => s.Title == SelectTitle);
                    if (matched == null) return;
                    if (matched.Title != StorageGroup.SelectStorage.Title)
                    {
                        StorageGroup.SelectStorage = matched;
                    }
                    else
                    {
                        StorageGroup.SelectStorage.Name = matched.Name;                       // 庫名稱
                        StorageGroup.SelectStorage.Number = matched.Number;                   // 庫編號
                        StorageGroup.SelectStorage.Rows = matched.Rows;          //最大行數
                        StorageGroup.SelectStorage.Columns = matched.Columns;    //最大列數

                        for (int i = 0; i < matched.Slots.Count; i++)
                        {
                            if (StorageGroup.SelectStorage.Slots.Count == i)
                                StorageGroup.SelectStorage.Slots.Add(new Slot());
                            StorageGroup.SelectStorage.Slots[i].Name = matched.Slots[i].Name;
                            StorageGroup.SelectStorage.Slots[i].StorageRestriction = matched.Slots[i].StorageRestriction;
                            StorageGroup.SelectStorage.Slots[i].ShortName = matched.Slots[i].ShortName;
                            StorageGroup.SelectStorage.Slots[i].Serial = matched.Slots[i].Serial;
                            StorageGroup.SelectStorage.Slots[i].Kind = matched.Slots[i].Kind;
                            StorageGroup.SelectStorage.Slots[i].MaterialStatus = matched.Slots[i].MaterialStatus;
                            StorageGroup.SelectStorage.Slots[i].MaterialRestriction = matched.Slots[i].MaterialRestriction;
                            StorageGroup.SelectStorage.Slots[i].StorageStatus = matched.Slots[i].StorageStatus;
                        }
                        while (StorageGroup.SelectStorage.Slots.Count > matched.Slots.Count)
                        {
                            StorageGroup.SelectStorage.Slots.RemoveAt(StorageGroup.SelectStorage.Slots.Count - 1);
                        }
                    }
                    StorageGroup.SelectStorage.WaitingCount = StorageGroup.SelectStorage.Slots.Count(s => s.MaterialStatus == "Verified");
                    StorageGroup.SelectStorage.ProcessingCount = StorageGroup.SelectStorage.Slots.Count(s => s.MaterialStatus == "Working");
                    StorageGroup.SelectStorage.ErrorCount = StorageGroup.SelectStorage.Slots.Count(s => s.MaterialStatus == "Error");
                    StorageGroup.SelectStorage.CompletedCount = StorageGroup.SelectStorage.Slots.Count(s => s.MaterialStatus == "Completed");
                    StorageGroup.SelectStorage.RestrictionCount = StorageGroup.SelectStorage.Slots.Count(s => s.StorageRestriction == true);
                    StorageGroup.SelectStorage.BookedCount = StorageGroup.SelectStorage.Slots.Count(s => s.StorageStatus == "Booked");
                }
            }
            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
