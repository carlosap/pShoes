using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Interfaces;

namespace Library.Models.Responses
{
    [Serializable]
    [Export(typeof(IResponse))]
    [ResponseAttributes(Name = "GetAccountAddressesResponse")]
    public class GetAccountAddressesResponse : IResponse
    {
        public List<AccountAddress> Addresses { get; set; }
        
        public GetAccountAddressesResponse()
        {
            Addresses = new List<AccountAddress>();
        }

        public GetAccountAddressesResponse(CustomerAddressResult apiResult) : this()
        {
            apiResult.CustomerAddresses.ForEach(apiAddress => Addresses.Add(new AccountAddress(apiAddress)));
        }
    }
}