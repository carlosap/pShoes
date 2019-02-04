using System;
using System.Collections.Generic;
using MadServ.Core.Models.Responses;

namespace Library.Models.Responses
{
    [Serializable]
    public class AccountOrderHistoryResponse : AccountOrderHistoryResponseBase
    {
        public List<AccountOrderHistoryItem> Items { get; set; }

        public AccountOrderHistoryResponse()
        {
            Items = new List<AccountOrderHistoryItem>();
        }
    }
}