using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "GetPaymentMethodsResponse")]
    public class GetPaymentMethodsResponse : IResponse
    {
        public List<PaymentMethod> PaymentMethods { get; set; }
        public List<Option> Cards { get; set; }

        public GetPaymentMethodsResponse()
        {
            PaymentMethods = new List<PaymentMethod>();
            Cards = new List<Option>();
        }

        public GetPaymentMethodsResponse(PaymentMethodResult apiResult) : this()
        {
            foreach (var method in apiResult.PaymentMethods)
            {
                var paymentMethod = new PaymentMethod(method);
                PaymentMethods.Add(paymentMethod);
                if (paymentMethod.Cards.Any())
                {
                    Cards = paymentMethod.Cards;
                }
            }
        }
    }
}
