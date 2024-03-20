using drip_chip_api.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace drip_chip_api
{
    public class Authorization
    {
        public DbContextOptions<DBContext> contextOptions = new DbContextOptionsBuilder<DBContext>()
        .UseInMemoryDatabase("drip-chip-db")
        .Options;

        public bool isAuthorized(string authHeader)
        {
            using var dbContext = new DBContext(contextOptions);

            if (!String.IsNullOrEmpty(authHeader))
            {
                var cred = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Substring(6)))
                    .Split(':');
                var user = new { login = cred[0], pass = cred[1] };

                var account = dbContext.Accounts.ToList().Where(account => account.email == user.login && account.password == user.pass).ToArray();
                if (account.Length != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool isEmail(string email)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            return regex.Match(email.Trim()).Success;
        }
    }
}
