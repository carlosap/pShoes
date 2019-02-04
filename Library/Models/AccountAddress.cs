using System;
using Library.DemandWare.Models.DTOs;
using Newtonsoft.Json;

namespace Library.Models
{
    [Serializable]
    public class AccountAddress : Address
    {
        public string Name { get; set; }

        public AccountAddress()
        {
        }

        public AccountAddress(CustomerAddress address)
        {
            if (address != null)
            {
                Id = address.Id;
                Name = address.Name;
                Address1 = address.Address1;
                Address2 = address.Address2;
                City = address.City;
                FirstName = address.FirstName;
                LastName = address.LastName;
                Phone = address.Phone;
                Zip = address.Zip;
                State = address.State;

                Title = string.Format("({0}) {1}, {2}, {3}, {4}", Name, Address1, City, State, Zip);
            }
        }

        public AccountAddress(SavedAddressDTO address) : this()
        {
            PopulateAccountAddressFromDTO(address);
        }

        public AccountAddress(string json) : this()
        {
            if (!string.IsNullOrEmpty(json))
            {
                var dto = JsonConvert.DeserializeObject<SavedAddressDTO>(json);
                PopulateAccountAddressFromDTO(dto);
            }
        }

        private void PopulateAccountAddressFromDTO(SavedAddressDTO address)
        {
            if (address != null)
            {
                Id = address.UUID;
                Name = address.Key;
                Address1 = address.Address1;
                Address2 = address.Address2;
                City = address.City;
                FirstName = address.FirstName;
                LastName = address.LastName;
                Phone = address.Phone;
                Zip = address.PostalCode;
                State = address.StateCode;

                Title = string.Format("({0}) {1}, {2}, {3}, {4}", Name, Address1, City, State, Zip);
            }
        }
    }
}
