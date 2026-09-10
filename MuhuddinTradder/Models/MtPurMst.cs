using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    public class MtPurMst
    {
        [Key]
        [StringLength(20)]
        public string InvCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Invoice date select karen")]
        [DataType(DataType.Date)]
        public DateTime? InvDt { get; set; }

        [Required(ErrorMessage = "Supplier select karen")]
        [StringLength(4)]
        public string TrdCd { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? RcvdDt { get; set; }

        // Purchase invoice ki scanned copy / bill ka attachment (path wwwroot/uploads/purchase ke andar)
        [StringLength(255)]
        public string? AttachmentPath { get; set; }

        public MtTraderMst? Trader { get; set; }
        public List<MtPurDtl>? Details { get; set; }
    }
}
