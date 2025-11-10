using System.Collections.Generic;

namespace NSN.Models
{
    public class ChartVm
    {
        public List<string> Labels { get; set; } = new();
        public List<long> Data { get; set; } = new();
    }
}