﻿using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "CheckoutUserRequest")]
    public class CheckoutUserRequest : IRequestParameter
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsRememberMe { get; set; }
    }
}
