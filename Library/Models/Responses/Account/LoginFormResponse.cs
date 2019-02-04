using System;
using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "LoginFormResponse")]
    public class LoginFormResponse
    {
        public string Action { get; set; }
        public string DWSecureKey { get; set; }
        public string DWLoginParam { get; set; }
        public string Email { get; set; }
    }
}
