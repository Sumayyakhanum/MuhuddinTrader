using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    public class MtItmMst
    {
        [Key]
        [StringLength(4)]
        public string ItmCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category select karen")]
        [StringLength(2)]
        public string CatCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item description likhen")]
        [StringLength(100)]
        public string ItmDesc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit select karen")]
        [StringLength(2)]
        public string UnitCd { get; set; } = string.Empty;

        [StringLength(1)]
        public string? ItmStatus { get; set; } = "A";

        public int? ItmShelfLife { get; set; }

        public decimal? ItmMoq { get; set; }

        // Navigation properties - form se nahi aate isliye validation ke liye nullable
        public MtItemCate? Category { get; set; }
        public MtUnitMst? Unit { get; set; }
    }
}
