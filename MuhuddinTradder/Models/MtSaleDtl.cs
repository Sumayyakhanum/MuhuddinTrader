using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    // Composite primary key (InvCd + ItmCd) DbContext ki OnModelCreating mein set hai
    public class MtSaleDtl
    {
        [Required]
        [StringLength(20)]
        public string InvCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item select karen")]
        [StringLength(4)]
        public string ItmCd { get; set; } = string.Empty;

        // Kis purchase invoice se stock nikla
        [StringLength(20)]
        public string? PurInv { get; set; }

        public decimal? PurRate { get; set; }

        [Required(ErrorMessage = "Quantity likhen")]
        public decimal? RcvgQty { get; set; }

        [Required(ErrorMessage = "Rate likhen")]
        public decimal? Rate { get; set; }

        public decimal? Disc { get; set; }

        public decimal? Cost { get; set; }

        public MtSaleMst? SaleMst { get; set; }
        public MtItmMst? Item { get; set; }
    }
}
