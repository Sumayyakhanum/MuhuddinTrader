using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    public class MtSaleMst
    {
        [Key]
        [StringLength(20)]
        public string InvCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Invoice date select karen")]
        [DataType(DataType.Date)]
        public DateTime? InvDt { get; set; }

        [Required(ErrorMessage = "Customer select karen")]
        [StringLength(4)]
        public string TrdCd { get; set; } = string.Empty;

        public MtTraderMst? Trader { get; set; }
        public List<MtSaleDtl>? Details { get; set; }
    }
}
