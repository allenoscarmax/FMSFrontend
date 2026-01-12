

namespace FMSFrontend.Features.Dtos
{
    public class MagazineParaDto
    {
        public bool[] shouldScanEle { get; set; } = { false, false, false };
        public bool shouldScanPart { get; set; } = false;
        public bool[] robotComing { get; set; } = new bool[3] { false, false, false };
        public bool robotComing_part { get; set; } = new bool();
        public bool[] door_to_light { get; set; } = new bool[2] { false, false }; //亮燈
        public bool[] partWarehouseSensor { get; set; } = new bool[8] { false, false, false, false, false, false, false, false };//工件倉1
        public bool[] eleMagzineDoorOpen { get; set; } = new bool[3] { false, false, false }; // ele門的狀態
        public bool[] eleMagzineDoorSwithOn { get; set; } = new bool[3] { false, false, false };
        public bool[] partMagzineDoorOpen { get; set; } = new bool[1] { false };  // part門的狀態
    }
}
