using System.ComponentModel.Composition;
using MadServ.Core.Interfaces;

namespace Library.Models.Requests
{
    [Export(typeof(IRequestParameter))]
    [RequestParameterAttributes(ClientId = Config.ClientId, Name = "EmailSignUpRequest")]
    public class EmailSignUpRequest : IRequestParameter
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public int BirthDay { get; set; }
        public int BirthMonth { get; set; }
        public bool IsFemale { get; set; }
    }
}
