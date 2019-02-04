using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;
using Library.Models.PowerReview;
using MadServ.Core.Models;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "PromosResponse")]
    public class PromosResponse : IResponse
    {
        public string TargetUrl { get; set; }
        public Image Image { get; set; }
        public PromosResponse()
        {
            TargetUrl = string.Empty;
            Image = new Image();
        }
    }
}