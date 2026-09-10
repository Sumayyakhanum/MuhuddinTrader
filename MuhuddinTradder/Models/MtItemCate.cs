using System.ComponentModel.DataAnnotations;

namespace MTDBMVC.Models
{
    public class MtItemCate
    {
        [Key]
        [Required(ErrorMessage = "Category code required hai")]
        [StringLength(2)]
        public string CatCd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category description likhen")]
        [StringLength(100)]
        public string CatDesc { get; set; } = string.Empty;

        [StringLength(1)]
        public string? CatStatus { get; set; } = "A";
    }
}
