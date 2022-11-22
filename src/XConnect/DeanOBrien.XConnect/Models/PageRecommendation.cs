using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.Models
{
    public class PageRecommendation
    {
        public string PageId { get; set; }
        public double Score { get; set; }
        public DateTime DateRecommended { get; set; }
    }
}
