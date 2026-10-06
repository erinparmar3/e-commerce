using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ProductResponseDto>>>> GetProducts(
        [FromQuery] ProductQueryParameters query)
    {
        var result = await _productService.GetProductsAsync(query);
        return Ok(ApiResponse<PaginatedResult<ProductResponseDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetProduct([FromRoute] int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(ApiResponse<ProductResponseDto>.Ok(product));
    }
}
