using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Request;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.ResponseDTOs;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using Mshop.Api.Utilities;
using System.Threading.Tasks;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService brandService;

        public BrandsController(IBrandService brandService)
        {
            this.brandService = brandService;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var brands = await brandService.GetAsync(null,false);
                return Ok(brands.Adapt<IEnumerable<BrandResponse>>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var brand = await brandService.GetOneAsync(c => c.Id == id,false);
                return Ok(brand.Adapt<BrandResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] BrandRequest brandRequest,CancellationToken cancellationToken = default)
        {
            try
            {
                var brandInDB = await brandService.AddAsync(brandRequest.Adapt<Brand>(), cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = brandInDB.Id }, brandInDB.Adapt<BrandResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await brandService.DeleteAsync(id, cancellationToken);
                if (result == false)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] BrandRequest brandRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await brandService.EditAsync(id, brandRequest.Adapt<Brand>(), cancellationToken);
                if (result == false) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("toggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await brandService.ToggleStatusAsync(id,cancellationToken);
            if (result == false)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
