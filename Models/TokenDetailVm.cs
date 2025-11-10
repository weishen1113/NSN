namespace NSN.Models
{
    public class TokenDetailVm
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public string ContractAddress { get; set; } = "";
        public long TotalHolders { get; set; }
        public int TotalSupply { get; set; }
        public decimal Price { get; set; }   // USD
    }
}
