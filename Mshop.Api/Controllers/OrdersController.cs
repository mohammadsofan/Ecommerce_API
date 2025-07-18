using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using Mshop.Api.Utilities;
using System.Security.Claims;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService orderService;
        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        public string? GetCurrentUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return id;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllForCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                if(userId is null)
                {
                    return NotFound();
                }
                var orders = await orderService.GetAsync(o => o.ApplicationUserId == userId,false,o=>o.OrderItems);
                return Ok(new { orders = orders.Adapt<IEnumerable<OrderResponse>>()});
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
   
        }
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetOneForCurrentUser([FromRoute] Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId is null)
                {
                    return NotFound();
                }
                var order = await orderService.GetOneAsync(o => o.ApplicationUserId == userId && o.Id == id, false);
                return Ok(new { order = order.Adapt<OrderResponse>() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [HttpGet("")]
        [Authorize(Roles =$"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orders = await orderService.GetAsync(null,false);
                return Ok(new { orders = orders.Adapt<IEnumerable<OrderResponse>>() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }

        }
        [HttpGet("{id}")]
        [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> GetOne([FromRoute] Guid id)
        {
            try
            { 
                var order = await orderService.GetOneAsync(o=>o.Id == id, false);
                return Ok(new { order = order.Adapt<OrderResponse>() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [HttpPost("")]
        [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> Create([FromBody] OrderRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                var order = request.Adapt<Order>();
                await orderService.AddAsync(order,cancellationToken);
                return CreatedAtAction(nameof(GetOne),new { order.Id },order.Adapt<OrderResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id,CancellationToken cancellationToken = default)
        {
            try
            {
               var result = await orderService.DeleteAsync(id,cancellationToken);
                if (!result)
                {
                    NotFound();
                }
                return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id,[FromBody] OrderRequest request,CancellationToken cancellationToken = default)
        {
            try
            {

                var order = request.Adapt<Order>();
                var result = await orderService.EditAsync(id,order,cancellationToken);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
        [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
        [HttpPatch("ChangeStatus/{id}")]
        public async Task<IActionResult> ChangeStatus([FromRoute] Guid id,[FromBody]ChangeOrderStatusRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await orderService.ChangeStatus(id,request.OrderStatus, cancellationToken);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

    }
}
