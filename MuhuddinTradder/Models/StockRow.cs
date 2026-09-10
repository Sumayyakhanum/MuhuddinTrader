namespace MTDBMVC.Models
{
    public class StockRow
    {
        public string ItmCd { get; set; } = string.Empty;
        public string? ItmDesc { get; set; }
        public string? ItmStatus { get; set; }
        public decimal? ItmMoq { get; set; }

        public decimal Purchased { get; set; }
        public decimal Sold { get; set; }
        public decimal Stock { get; set; }

        public decimal ProfitLoss { get; set; }

        public bool LowQuantity { get; set; }
    }
}
