using EmployeeDemo.Application.Commons;
using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.ViewModels.ProductViewModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EmployeeDemo.API.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService) 
        { 
            _productService = productService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(Guid id)
        {
            try
            {
                return Ok(await _productService.GetProductAsync(id));
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProductPaginationAsync([FromQuery]PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _productService.GetProductPaginationAsync(paginationParameter);
                var metadata = new
                {
                    result.TotalCount,
                    result.PageSize,
                    result.CurrentPage,
                    result.TotalPages,
                    result.HasNext,
                    result.HasPrevious
                };
                Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateProductAsync(CreateProductViewModel product)
        {
            try
            {
                var result = await _productService.CreateProductAsync(product);
                return result != null ? Ok(result) : BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
