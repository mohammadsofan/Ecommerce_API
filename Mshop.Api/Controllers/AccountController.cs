using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using Mshop.Api.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailSender emailSender;
        private readonly ITokenService tokenService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser>  signInManager,
            RoleManager<IdentityRole> roleManager
            ,IEmailSender emailSender,
            ITokenService tokenService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
            this.tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                var applicationUser = registerRequest.Adapt<ApplicationUser>();
                var result = await userManager.CreateAsync(applicationUser, registerRequest.Password);
                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                    var encodedToken = WebUtility.UrlEncode(token);
                    var confirmationUrl = Url.Action(nameof(ConfirmEmail),"Account",
                        new {token=encodedToken,userId=applicationUser.Id},Request.Scheme,Request.Host.Value);

                    await emailSender.SendEmailAsync(applicationUser.Email!, "Welcome to MShop!", $@"
                        <h2>Welcome to MShop, {applicationUser.FirstName} {applicationUser.LastName}!</h2>
                        <p>We're excited to have you on board.</p>
                        <p>Your account has been successfully created. You can now log in and start shopping after confirming your email!</p>
                        <a href='{confirmationUrl}'>Confirm Email</a>
                        <p>If you have any questions, feel free to reach out to us at <a href='mailto:support@mshop.com'>support@mshop.com</a>.</p>
                        <br/>
                        <p>Happy Shopping!<br/>The MShop Team</p>
                        "
                        );
                    await userManager.AddToRoleAsync(applicationUser, ApplicationRoles.Customer);
                    return Ok(new {Message="your account is created! please check your email to confirm your account, thanks!"});
                }

                return BadRequest(result.Errors);
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                    return BadRequest(new { message = "Invalid email confirmation request." });

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                var decodedToken = WebUtility.UrlDecode(token);

                var result = await userManager.ConfirmEmailAsync(user, decodedToken);
                if (result.Succeeded)
                    return Ok(new { message = "Email confirmed successfully." });

                return BadRequest(new { message = "Email confirmation failed.", errors = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var applicationUser = await userManager.FindByEmailAsync(loginRequest.Email);
                if (applicationUser is null)
                {
                    return Unauthorized(new { Message = "Invalid email or password" });
                }
                var passwordCheck= await userManager.CheckPasswordAsync(applicationUser,loginRequest.Password);
                if (!passwordCheck)
                {
                    return Unauthorized(new { Message = "Invalid email or password" });

                }
                var result = await signInManager.CheckPasswordSignInAsync(applicationUser, loginRequest.Password,true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return BadRequest(new { message = $"Your account is locked.",applicationUser.LockoutEnd});
                    if (result.RequiresTwoFactor)
                        return BadRequest(new { message = "Two-factor authentication required." });
                    if (result.IsNotAllowed)
                        return Unauthorized(new { message = "Please confirm your email before logging in." });
                    return Unauthorized(new { Message = "Invalid email or password" });

                }

                var userRoles = await userManager.GetRolesAsync(applicationUser);
                var token = tokenService.GetToken(applicationUser.Id, applicationUser.UserName!, userRoles);
                return Ok(new { token });
                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            try
            {
            var applicationUser = await userManager.GetUserAsync(User);
            if(applicationUser is not null)
            {

                  var res= await userManager.ChangePasswordAsync(applicationUser, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);
                    if (res.Succeeded)
                    {
                        return NoContent();
                    }
                    return BadRequest(res.Errors);
            }
            return Unauthorized(new { message = "User no longer exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

    }
}
