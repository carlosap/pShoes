using MadServ.Core.Interfaces;

namespace Library.Helpers
{
    public static class RequestHeaderHelper
    {
        public static string GetClientIP(ICore core)
        {
            var result = Config.Params.DefaultClientIP;

            var session = new Session(core);

            if (session.Exists(Config.Keys.ClientIP))
            {
                result = session.Get<string>(Config.Keys.ClientIP);
            }
            else
            {
                var clientIP = MadServ.Core.Helpers.IPAddressHelper.GetClientIPAddress(core.Context.Request);

                if (!string.IsNullOrEmpty(clientIP))
                {
                    result = clientIP;
                }

                session.Add<string>(Config.Keys.ClientIP, result);
            }

            return result;
        }
    }
}
