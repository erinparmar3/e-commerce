using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/seller/products")]
[Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
public class SellerProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public SellerProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> Create([FromBody] CreateProductDto dto)
    {
        var userId = User.GetUserId();
        var product = await _productService.CreateSellerProductAsync(userId, dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ProductResponseDto>.Ok(product, "Product listed successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponseDto>>>> GetAll()
    {
        var userId = User.GetUserId();
        var products = await _productService.GetSellerProductsAsync(userId);
        return Ok(ApiResponse<IEnumerable<ProductResponseDto>>.Ok(products));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetById([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var product = await _productService.GetSellerProductByIdAsync(userId, id);
        return Ok(ApiResponse<ProductResponseDto>.Ok(product));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductDto dto)
    {
        var userId = User.GetUserId();
        var product = await _productService.UpdateSellerProductAsync(userId, id, dto);
        return Ok(ApiResponse<ProductResponseDto>.Ok(product, "Product updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var result = await _productService.DeleteSellerProductAsync(userId, id);
        return Ok(ApiResponse<bool>.Ok(result, "Product deleted or deactivated successfully."));
    }
}
