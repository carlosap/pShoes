﻿using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "PowerReviewFaceoffRequest")]
    public class PowerReviewFaceoffRequest : IRequestParameter
    {
        public string ProductId { get; set; }
    }
}


