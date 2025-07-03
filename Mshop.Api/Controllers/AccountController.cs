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
        private readonly IResetPasswordCodeService resetPasswordCodeService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser>  signInManager,
            RoleManager<IdentityRole> roleManager
            ,IEmailSender emailSender,
            ITokenService tokenService,
            IResetPasswordCodeService resetPasswordCodeService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
            this.tokenService = tokenService;
            this.resetPasswordCodeService = resetPasswordCodeService;
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
        //Reset-Password using Link with GeneratePasswordResetTokenAsync() from Identity
        /*
        [HttpGet("ResetPasswordRequest")]
        public async Task<IActionResult> ResetPasswordRequest([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if(user is null)
            {
                return Accepted(new {message="user not found!"});
            }
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var resetLink = $"https://frontend.com/reset-password?token={encodedToken}&email={email}&userId={user.Id}";
            await emailSender.SendEmailAsync(email, "Reset Password Link", $@"
                    <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                        <h2 style=""color: #333;"">Reset Your Password</h2>
                        <p>Hello,</p>
                        <p>We received a request to reset your password. Click the link below to choose a new password:</p>
                        <p>
                            <a href=""{resetLink}"" 
                               style=""display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;"">
                               Reset Password
                            </a>
                        </p>
                        <p>If you didn’t request a password reset, you can ignore this email.</p>
                        <p style=""color: #777; font-size: 0.9em;"">This link will expire in 15 minutes.</p>
                        <hr />
                        <p style=""font-size: 0.8em; color: #999;"">© {DateTime.UtcNow.Year} MShop. All rights reserved.</p>
                    </div>
                ");
            return Accepted(new {message="Email sended",email});
        }
        [HttpPost("ConfirmResetPassword")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest confirmReset)
        {
            try
            {
                var user = await userManager.FindByIdAsync(confirmReset.UserId);
                if(user is null)
                {
                    return NotFound(new { message = "user not found." });
                }
                var decodedToken = WebUtility.UrlDecode(confirmReset.Token);
                var result = await userManager.ResetPasswordAsync(user, decodedToken, confirmReset.NewPassword);
                if (result.Succeeded)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { 
                        message = "Password reset failed.",
                        errors = result.Errors.Select(e => e.Description) 
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new {ex.Message});
            }
        }
        */

        //Reset-Password using code
        [HttpGet("SendResetPasswordCode")]
        public async Task<IActionResult> SendResetPasswordCode([FromQuery] string email,CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    return Accepted();
                }
                var resetPasswordCode = new ResetPasswordCode()
                {
                    ApplicationUserId = user.Id,
                    Code = new Random().Next(1000, 9999).ToString(),
                    ExpirationDate = DateTime.UtcNow.AddMinutes(15)
                };
                await resetPasswordCodeService.AddAsync(resetPasswordCode,cancellationToken);
                var htmlMessage = $@"
                <div style=""font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: auto;"">
                    <h2 style=""color: #333;"">Reset Your Password</h2>
                    <p>Hello,</p>
                    <p>We received a request to reset your password for your account associated with this email: <strong>{email}</strong>.</p>
                    <p>Please use the verification code below to reset your password:</p>
                    <div style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 2px; border-radius: 8px; color: #333; margin: 20px 0;"">
                        {resetPasswordCode.Code}
                    </div>
                    <p>This code will expire in <strong>15 minutes</strong>.</p>
                    <p>If you didn't request this, you can safely ignore this email.</p>
                    <hr style=""margin: 30px 0;"" />
                    <p style=""font-size: 0.8em; color: #999;"">© {DateTime.UtcNow.Year} MShop. All rights reserved.</p>
                </div>";

                await emailSender.SendEmailAsync(email, "Reset Password Verification Code", htmlMessage);
                return Accepted();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("ConfirmResetPassword")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest confirmReset,CancellationToken cancellationToken =default)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(confirmReset.Email);
                if (user is null)
                {
                    return NotFound();
                }
                var resetPasswordCode = await resetPasswordCodeService.GetOneAsync(c => c.Code == confirmReset.Code && c.ApplicationUserId == user.Id);
                if (resetPasswordCode == null)
                {
                    return BadRequest(new { message = "Invalid Code." });
                }
                if(resetPasswordCode.IsUsed == true)
                {
                    return BadRequest(new { message = "Code Already Used." });

                }
                if (DateTime.UtcNow > resetPasswordCode.ExpirationDate)
                {
                    return BadRequest(new { message = "Code Expired." });
                }
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, confirmReset.NewPassword);
                if (result.Succeeded)
                {
                    resetPasswordCode.IsUsed = true;
                    await resetPasswordCodeService.EditAsync(resetPasswordCode.Id, resetPasswordCode,cancellationToken);
                    var htmlMessage = $@"
                    <div style=""font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: auto;"">
                        <h2 style=""color: #28a745;"">Your Password Has Been Reset</h2>
                        <p>Hello,</p>
                        <p>This is a confirmation that your password has been successfully reset for your account associated with this email: <strong>{confirmReset.Email}</strong>.</p>
                        <p>If you made this change, no further action is needed.</p>
                        <p>If you did not request this change, please contact our support team immediately to secure your account.</p>
                        <hr style=""margin: 30px 0;"" />
                        <p style=""font-size: 0.8em; color: #999;"">© {DateTime.UtcNow.Year} MShop. All rights reserved.</p>
                    </div>";
                    await emailSender.SendEmailAsync(confirmReset.Email, "Your Password Has Been Reset", htmlMessage);

                    return Ok(new { message = "Password has been reset successfully." });
                }
                else
                {
                    return BadRequest(new { 
                        message = "Password reset failed.",
                        errors = result.Errors.Select(e => e.Description) 
                    });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
