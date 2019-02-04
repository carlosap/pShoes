using System;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class SavedAddressOption : Option
    {
        public AccountAddress AccountAddress { get; set; }
    }
}
