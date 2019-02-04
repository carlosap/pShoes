using System;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class AccountOrderHistoryItem
    {
        public string Date { get; set; }
        public string Id { get; set; }
        public Price Total { get; set; }
        public string Status { get; set; }
        public string TrackingNum { get; set; }

        public AccountOrderHistoryItem()
        {
            Total = new Price();
        }
    }
}
