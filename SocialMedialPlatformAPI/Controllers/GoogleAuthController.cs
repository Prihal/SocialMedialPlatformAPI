
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;

namespace SocialMedialPlatformAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public GoogleAuthController(AppDbContext context,IConfiguration configuration,IJwtService jwtService)
        {
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [HttpGet("SignIn-Google")]
        
        public IActionResult SignInGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", null,Request.Scheme);
            var property = new AuthenticationProperties
            {
                RedirectUri=redirectUrl
            };
            return Challenge(property,GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return BadRequest("Google authentication failed.");
            }

            var emailClaim = authenticateResult.Principal?.FindFirst(ClaimTypes.Email);
            var providerKeyClaim = authenticateResult.Principal?.FindFirst(ClaimTypes.NameIdentifier);

            if (emailClaim == null || providerKeyClaim == null)
            {
                return BadRequest("Failed to retrieve Google claims.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == emailClaim.Value);

            if (user == null)
            {
                // Register the user if they don't exist
                user = new User
                {
                    Email = emailClaim.Value,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);

                await _context.SaveChangesAsync();
            }

            var token = _jwtService.GenerateJwtToken(user);

            return Ok(new { token });
        }

        
    }
}
