using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    // Composite primary key (InvCd + ItmCd) DbContext ki OnModelCreating mein set hai
    public class MtPurDtl
    {
        [Required]
        [StringLength(20)]
        public string InvCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item select karen")]
        [StringLength(4)]
        public string ItmCd { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? Dom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Doe { get; set; }

        [Required(ErrorMessage = "Quantity likhen")]
        public decimal? RcvgQty { get; set; }

        [Required(ErrorMessage = "Rate likhen")]
        public decimal? Rate { get; set; }

        public decimal? Disc { get; set; }

        public decimal? Cost { get; set; }

        public MtPurMst? PurMst { get; set; }
        public MtItmMst? Item { get; set; }
    }
}
