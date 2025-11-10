// Models/Token.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSN.Models
{
    [Table("tokens")]
    public class Token
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = "";

        [Required]
        [Column("symbol")]
        public string Symbol { get; set; } = "";

        [Required]
        [Display(Name = "Contract Address")]
        [Column("contract_address")]
        public string ContractAddress { get; set; } = "";

        [Required]
        [Display(Name = "Total Supply")]
        [Column("total_supply")]
        public int TotalSupply { get; set; }

        [Required]
        [Display(Name = "Total Holders")]
        [Column("total_holders")]
        public long TotalHolders { get; set; }  // BIGINT in DB

        [Column("price")]
        [Display(Name = "Price (USD)")]
        public decimal? Price { get; set; }  // will be set by Part 3, default 0
    }
}