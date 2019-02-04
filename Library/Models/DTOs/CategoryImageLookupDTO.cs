using System.Collections.Generic;
using System.Xml.Serialization;

namespace Library.Models
{
    [XmlRoot("Categories")]
    public class CategoryImageLookupDTO
    {
        [XmlElement("Category")]
        public List<CategoryIdToImageDTO> Categories { get; set; }

        public CategoryImageLookupDTO()
        {
            Categories = new List<CategoryIdToImageDTO>();
        }
    }
}