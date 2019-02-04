using System;
using Library.DemandWare.Models.DTOs;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class Address : AddressBase
    {
        public Address()
        {
        }

        public Address(OrderAddress address)
        {
            if (address != null)
            {
                Address1 = address.Address1;
                Address2 = address.Address2;
                City = address.City;
                FirstName = address.FirstName;
                LastName = address.LastName;
                Phone = address.Phone;
                Zip = address.Zip;
                State = address.State;
            }
        }
    }
}
