using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "ForgotPasswordFormResponse")]
    public class ForgotPasswordFormResponse
    {
        public string Action { get; set; }
        public string DWSecureKey { get; set; }
    }
}
