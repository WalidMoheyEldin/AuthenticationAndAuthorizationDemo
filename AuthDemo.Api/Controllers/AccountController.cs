using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthDemo.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AuthDemo.WebShared.Data;

namespace AuthDemo.Api.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;

        public AccountController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<object> Login(LoginViewModel model)
        {
            try
            {
                IdentityUser user = await userManager.FindByEmailAsync(model.Email);
                bool isSuccess = await userManager.CheckPasswordAsync(user, model.Password);

                if (!isSuccess) return Unauthorized();

                List<Claim> claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                IList<string> userRoles = await userManager.GetRolesAsync(user);
                claims.AddRange(userRoles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

                IList<Claim> userClaims = await userManager.GetClaimsAsync(user);
                claims.AddRange(userClaims);

                foreach (string role in userRoles)
                {
                    IdentityRole identityRole = await roleManager.FindByNameAsync(role);
                    IList<Claim> roleClaim = await roleManager.GetClaimsAsync(identityRole);
                    claims.AddRange(roleClaim);
                }

                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthSettings:key"]));

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: configuration["AuthSettings:ValidIssuer"],
                    audience: configuration["AuthSettings:ValidAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(1),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                ApiLoginResult result = new ApiLoginResult
                {
                    Token = tokenString,
                    ExpiredOn = token.ValidTo.ToLocalTime()
                };

                return result;
            }
            catch (Exception exp)
            {
                return exp;
            }
        }
    }
}
