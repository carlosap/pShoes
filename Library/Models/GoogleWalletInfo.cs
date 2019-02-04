using System;

namespace Library.Models
{
    [Serializable]
    public class GoogleWalletInfo
    {
        public string MWRequestJwt { get; set; }
        public bool PreauthFlow { get; set; }
        public string ClientId { get; set; }
        public bool DisableCheckout { get; set; }
        public string TrackJwt { get; set; }

        public string FWRequestJwt { get; set; }
        public string CWRequestJwt { get; set; }

        public bool TabletEnabled { get; set; }
        public bool MobileEnabled { get; set; }

        public GoogleWalletInfo()
        {
            TabletEnabled = Config.GWTabletEnabled;
            MobileEnabled = Config.GWMobileEnabled;
        }
    }
}
