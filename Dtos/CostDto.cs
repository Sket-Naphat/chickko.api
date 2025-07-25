
using System.ComponentModel.DataAnnotations;
using chickko.api.Models;

namespace chickko.api.Dtos
{
    public class CostDto
    {
        [Required]
        public int CostCategoryID { get; set; } // id ประเภทค่าใช้จ่าย
        [Required]
        public int CostPrice { get; set; } = 0; //ราคาที่ซื้อ
        public string CostDescription { get; set; } = string.Empty; //รายละเอียดการซื้อ
        public DateOnly CostDate { get; set; } // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public TimeOnly CostTime { get; set; }
        public DateOnly UpdateDate { get; set; } = DateOnly.FromDateTime(System.DateTime.Now);
        public TimeOnly UpdateTime { get; set; } = TimeOnly.FromDateTime(System.DateTime.Now);
        public bool isFinish { get; set; } = false; //ใชเพื่อบันทึกกรณีที่ยังไม่กดบันทึกแค่กรอกไว้เฉยๆ ให้เก็บค่าที่เคยกรอกไว้ไปแสดง
    }

}