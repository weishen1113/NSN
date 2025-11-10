namespace NSN.Models
{
    public class TokenRowVm
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public string ContractAddress { get; set; } = "";
        public long TotalHolders { get; set; }
        public int TotalSupply { get; set; }

        // Computed
        public double Percent { get; set; }  // 0..100
        public int Rank { get; set; }
    }
}