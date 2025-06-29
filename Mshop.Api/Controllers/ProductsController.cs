using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Requests;
using Mshop.Api.DTOs.Responses;
using Mshop.Api.Services;
using Mshop.Api.Utilities;
using System;
using System.Threading.Tasks;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =$"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IProductService productService;

        public ProductsController(ApplicationDbContext context,IProductService productService)
        {
            this.context = context;
            this.productService = productService;
        }
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? query, [FromQuery] int page=1, [FromQuery] int limit = 10)
        {
            try {
                var products = await productService.GetAsync(query, page,limit,false);
                if(products is null)
                {
                    return NotFound();
                }
                return Ok(new { products = products.Adapt<IEnumerable<ProductResponse>>(),page,skip=(page-1)*limit,count=products.Count() });
            }
            catch(Exception ex)
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
                var product = await productService.GetOneAsync(p=>p.Id==id,false);
                if (product is null)
                {
                    return NotFound();
                }
                return Ok(product.Adapt<ProductResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("")]
        public async Task<IActionResult> Create([FromForm] ProductRequest productRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await productService.AddAsync(productRequest.Adapt<Product>(), productRequest.MainImage, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { product.Id }, product.Adapt<ProductResponse>());
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id,CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await productService.DeleteAsync(id, cancellationToken);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] ProductUpdateRequest productRequest,CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await productService.EditAsync(id, productRequest.Adapt<Product>(), productRequest.MainImage,cancellationToken);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPatch("toggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await productService.ToggleStatusAsync(id, cancellationToken);
            if (result == false)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
