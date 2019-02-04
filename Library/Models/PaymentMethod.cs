using System;
using System.Collections.Generic;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class PaymentMethod : Option
    {
        public List<Option> Cards { get; set; }

        public PaymentMethod()
        {
            Cards = new List<Option>();
        }

        public PaymentMethod(DWPaymentMethod apiMethod) : this()
        {
            Name = apiMethod.Name;
            Value = apiMethod.Id;

            foreach (var card in apiMethod.Cards)
            {
                Cards.Add(new Option
                {
                    Name = card.Name,
                    Value = card.Id
                });
            }
        }
    }
}
