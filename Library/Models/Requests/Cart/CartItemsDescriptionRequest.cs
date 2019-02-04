using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using MadServ.Core.Models;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CartItemsDescriptionRequest")]
    public class CartItemsDescriptionRequest : IRequestParameter
    {
        public List<CartItem> CartItems { get; set; }

        public CartItemsDescriptionRequest()
        {
            CartItems = new List<CartItem>();
        }
    }
}