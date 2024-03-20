using drip_chip_api.Context;
using drip_chip_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System.Linq;
using System.Net.Mime;
using System.Security.Principal;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace drip_chip_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class accountsController : ControllerBase
    {

        private readonly Context.DBContext dbContext;
        public Authorization authorization = new Authorization();

        public accountsController(Context.DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET: api/<accountsController>/search
        [HttpGet("search")]
        public ActionResult<BaseAccount[]> GetAccountSearch(string? firstName, string? lastName, string? email, int size = 9, int from = 0)
        {
            if (Request.Headers.Authorization.ToString() != "")
            {
                if (!authorization.isAuthorized(Request.Headers.Authorization))
                {
                    return Unauthorized();
                }
            }

            if (size <= 0 || from < 0)
            {
                return BadRequest();
            }

            var accounts = dbContext.Accounts.ToList();
            
            if(firstName != null)
            {
                accounts = accounts.Where(account => account.firstName.ToLower().IndexOf(firstName) != -1).ToList();
            }
            if(lastName != null)
            {
                accounts = accounts.Where(account => account.lastName.ToLower().IndexOf(lastName) != -1).ToList();
            }
            if(email != null)
            {
                accounts = accounts.Where(account => account.email.ToLower().IndexOf(email) != -1).ToList();
            }

            var selectedAccounts = accounts.Select(account => new BaseAccount { id = account.id, firstName = account.firstName, lastName = account.lastName, email = account.email }).ToArray();

            var rangedSelectedAccouts = new List<BaseAccount> {};

            for(int i = from; i < selectedAccounts.Count(); i++)
            {
                rangedSelectedAccouts.Add(selectedAccounts[i]);
            }

            if(size > rangedSelectedAccouts.Count())
            {
                return rangedSelectedAccouts.OrderBy(acc => acc.id).ToArray();
            }

            return rangedSelectedAccouts.OrderBy(acc => acc.id).ToArray()[..(size)];

        }

        // GET api/<accountsController>/5
        [HttpGet("{id}")]
        public ActionResult<BaseAccount> GetAccountById(int id)
        {
            if (Request.Headers.Authorization.ToString() != "")
            {
                if (!authorization.isAuthorized(Request.Headers.Authorization))
                {
                    return Unauthorized();
                }
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            var account = dbContext.Accounts.Where(account => account.id == id).Select(account => new BaseAccount { id = account.id, firstName = account.firstName, lastName = account.lastName, email = account.email }).ToArray();
            if(account.Length == 0)
            {
                return NotFound();
            }

            return account[0];
        }

        // POST api/<AccountsController>

        [HttpPost("/api/registration")]
        public ActionResult<BaseAccount> Registration( Account newAccount)
        {

            if (authorization.isAuthorized(Request.Headers.Authorization))
            {
                return StatusCode(403);
            }


            if (newAccount.firstName == null || newAccount.lastName == null || newAccount.email == null || newAccount.password == null)
            {
                return BadRequest();
            }
            if(newAccount.firstName.Trim() == "" || newAccount.lastName.Trim() == "" || newAccount.email.Trim() == "" || newAccount.password.Trim() == "" || authorization.isEmail(newAccount.email) == false)
            {
                return BadRequest();
            }
            if(dbContext.Accounts.Where(account => account.email == newAccount.email).ToArray().Length != 0)
            {
                return Conflict();
            }

            dbContext.Accounts.Add(newAccount);
            dbContext.SaveChanges();
            return StatusCode(201, new BaseAccount { id = newAccount.id, firstName = newAccount.firstName, lastName = newAccount.lastName, email = newAccount.email });
        }

        [HttpPut("{id}")]
        public ActionResult<BaseAccount> UpdateAccount([FromBody] Account account, int id )
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }
            if (id <= 0 || account.firstName == null || account.password == null  || account.password.Trim() == "" || account.lastName == null || account.email == null || account.firstName.Trim() == "" || account.lastName.Trim() == "" || account.email.Trim() == "" || authorization.isEmail(account.email) == false)
            {
                return BadRequest();
            }
            if(dbContext.Accounts.Where(acc => acc.id == id).ToList().Count() == 0)
            {
                return StatusCode(403);
            }
            if (dbContext.Accounts.Where(acc => acc.email == account.email).ToArray().Length > 1)
            {
                return Conflict();
            }

            var creditance = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Request.Headers.Authorization.ToString().Substring(6))).Split(':');
            var user = new { email = creditance[0], password = creditance[1] };
            var updateAccount = dbContext.Accounts.Where(acc => acc.id == id).ToList();

            if (updateAccount[0].email != user.email || updateAccount[0].password != user.password)
            {
                return StatusCode(403);
            }

            updateAccount[0].firstName= account.firstName;
            updateAccount[0].lastName= account.lastName;
            updateAccount[0].password= account.password;
            updateAccount[0].email = account.email;

            dbContext.SaveChanges();

            return StatusCode(200, new BaseAccount { id = updateAccount[0].id, firstName = updateAccount[0].firstName, lastName = updateAccount[0].lastName, email = updateAccount[0].email });
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteAccount(int id) {

            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }

           var account = dbContext.Accounts.Where(account => account.id == id).ToList();
            if (account.Count() == 0)
            {
                return StatusCode(403);
            }

            var creditance = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Request.Headers.Authorization.ToString().Substring(6))).Split(':');
            var user = new { email = creditance[0], password = creditance[1] };

            if (account[0].email != user.email || account[0].password != user.password)
            {
                return StatusCode(403);
            }

            dbContext.Accounts.Remove(account[0]);
            dbContext.SaveChanges();

            return Ok();
        }
    }
}
