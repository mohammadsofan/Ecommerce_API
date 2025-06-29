using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Request;
using Mshop.Api.DTOs.ResponseDTOs;
using Mshop.Api.Services;
using Mshop.Api.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Mshop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =$"{ApplicationRoles.Admin},{ApplicationRoles.SuperAdmin}")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categoies = await categoryService.GetAsync();
                return Ok(categoies.Adapt<IEnumerable<CategoryResponse>>());

            }catch(Exception ex)
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
                var category = await categoryService.GetOneAsync(c => c.Id == id);
                if (category == null) { 
                    return NotFound();
                }
                return Ok(category.Adapt<CategoryResponse>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CategoryRequest categoryRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var categoryInDB = await categoryService.AddAsync(categoryRequest.Adapt<Category>(), cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = categoryInDB.Id }, categoryInDB.Adapt<CategoryResponse>());
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
                var result = await categoryService.DeleteAsync(id);
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
        public async Task<IActionResult> Update([FromRoute] Guid id,[FromBody] CategoryRequest categoryRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await categoryService.EditAsync(id, categoryRequest.Adapt<Category>(),cancellationToken);
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

        [HttpPatch("toggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await categoryService.ToggleStatusAsync(id,cancellationToken);
            if(result == false)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
