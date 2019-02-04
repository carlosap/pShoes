using System;
using System.ComponentModel.Composition;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;
using MadServ.Core.Models.Responses;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "CartResponse")]
    public class CartResponse : CartResponseBase
    {
        new public Cart Cart { get; set; }
        
        public dynamic TealiumDataBase { get; set; }
        public dynamic TealiumDataExtended { get; set; }

        public Coupon CouponStatus { get; set; }
        
        public CartResponse()
        {
            Cart = new Cart();
        }

        public CartResponse(Basket apiBasket)
        {
            Cart = new Cart(apiBasket);
        }
    }
}