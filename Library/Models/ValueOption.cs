using System;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ValueOption : Option
    {
        public bool IsInStock { get; set; }
    }
}
