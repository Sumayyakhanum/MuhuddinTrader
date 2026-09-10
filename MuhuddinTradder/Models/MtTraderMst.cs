using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    public class MtTraderMst
    {
        [Key]
        [Required(ErrorMessage = "Trader Code likhen")]
        [StringLength(10)]
        public string TrdCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trader ka naam likhen")]
        [StringLength(100)]
        public string TrdDesc { get; set; } = string.Empty;

        // S = Supplier, C = Customer
        [Required(ErrorMessage = "Type select karen")]
        [StringLength(1)]
        public string TrdType { get; set; } = string.Empty;

        [StringLength(2)]
        public string? TrdCate { get; set; }

        [StringLength(100)]
        public string? TrdAdd { get; set; }

        // STRN
        [StringLength(50)]
        public string? TrdStr { get; set; }

        // NTN
        [StringLength(50)]
        public string? TrdNtn { get; set; }
    }
}
