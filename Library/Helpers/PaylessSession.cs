using Library.Models;
using MadServ.Core.Interfaces;


namespace Library.Helpers
{
    public class PaylessSession : Session
    {
        public PaylessSession(ICore core) : base(core)
        {
        }

        public CheckoutResponse GetCheckout()
        {
            return Get<CheckoutResponse>(Config.Keys.Checkout);
        }

        public void SetCheckout(CheckoutResponse checkout)
        {
            Add(Config.Keys.Checkout, checkout);
        }

        public void RemoveCheckout()
        {
            Remove(Config.Keys.Checkout);
        }
    }
}