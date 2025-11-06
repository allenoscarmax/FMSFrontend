using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FMSFrontend.Features.Mappings
{
    public static class PlcMapping
    {
        private static readonly SolidColorBrush LightOn = new(Color.FromRgb(0x61, 0xB4, 0x55));
        private static readonly SolidColorBrush LightOff = new(Color.FromRgb(0x00, 0x00, 0x00));

        public static void ApplyMagazineParaDto(this MagazineParaDto dto, MagazinePara magazinePara)
        {
            magazinePara.IsDoorLightOn = dto.door_to_light[0];
            magazinePara.UpperScanStatus[0] = dto.shouldScanEle ? LightOn : LightOff;
            magazinePara.UpperDoorStatus[0] = dto.eleMagzineDoorOpen[0] ? LightOn : LightOff;
            magazinePara.LowerScanStatus[0] = dto.shouldScanPart ? LightOn : LightOff;
            magazinePara.LowerDoorStatus[0] = dto.partMagzineDoorOpen[0] ? LightOn : LightOff;
            /*
            Random rnd = new Random();
            bool[] b = new bool[4];
            for (int i = 0; i < b.Length; i++)  b[i] = rnd.Next(2) == 1;
            magazinePara.UpperScanStatus[0] = b[0] ? LightOn : LightOff;
            magazinePara.UpperDoorStatus[0] = b[1] ? LightOn : LightOff;
            magazinePara.LowerScanStatus[0] = b[2] ? LightOn : LightOff;
            magazinePara.LowerDoorStatus[0] = b[3] ? LightOn : LightOff; 
            */
        }
    }
}
