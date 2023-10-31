using Microsoft.Identity.Web;

namespace ChessInWeb.Service
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var resp = connection.User.FindFirst(ClaimConstants.PreferredUserName)?.Value;
            return resp is null ? "" : resp;
        }
    }
}
