using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using Mshop.Api.Utilities;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =$"{ApplicationRoles.SuperAdmin}")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService) {
            this.userService = userService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                return Ok(await userService.GetAllAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOneUser([FromRoute] string id)
        {
            try
            {
                var user = await userService.GetOneAsync(id);
                if (user is null)
                {
                    return NotFound();
                }
                return Ok(user.Adapt<ApplicationUserResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("changeRole/{id}")]
        public async Task<IActionResult> ChangeRole([FromRoute] string id, [FromQuery] string role)
        {
            try
            {
                var result = await userService.ChangeRoleAsync(id, role);
                if (result==UserService.ChangeRoleResult.USER_NOT_FOUND)
                {
                    return NotFound();
                }
                else if (result == UserService.ChangeRoleResult.DELETING_ROLES_FAILED)
                {
                        return BadRequest("Failed to remove old roles.");
                }
                else if (result ==UserService.ChangeRoleResult.ASSIGN_ROLE_FAILED)
                {
                    return BadRequest("Failed to assign new role.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("lockUser/{id}")]
        public async Task<IActionResult> LockUser([FromRoute]string id,[FromBody]LockUserRequest LockRequest)
        {
            try
            {
                var result = await userService.LockUserAsync(id, LockRequest.LockEndDate);
                if(result is null)
                {
                    return NotFound();
                }
                if(result == false)
                {
                    return BadRequest(new { message = "Failed to lock user. Please ensure the provided date is in the future."});
                }
                return Ok(new {
                    message = $"User locked successfully",
                    lockedUntil = LockRequest.LockEndDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("unlockUser/{id}")]
        public async Task<IActionResult> UnlockUser([FromRoute] string id)
        {
            try
            {
                var result = await userService.UnlockUserAsync(id);
                if (result is null)
                {
                    return NotFound();
                }
                if (result == false)
                {
                    return BadRequest(new { message = "Fail to unlock user." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
