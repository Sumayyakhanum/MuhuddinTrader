using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    public class MtUnitMst
    {
        [Key]
        [Required(ErrorMessage = "Unit code required hai")]
        [StringLength(2)]
        public string UnitCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit description likhen")]
        [StringLength(100)]
        public string UnitDesc { get; set; } = string.Empty;

        [StringLength(1)]
        public string? UnitStatus { get; set; } = "A";
    }
}
