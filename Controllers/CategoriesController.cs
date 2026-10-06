using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
        return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.Ok(categories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetById([FromRoute] int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(ApiResponse<CategoryResponseDto>.Ok(category));
    }

    [HttpGet("{id}/products")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ProductResponseDto>>>> GetProducts(
        [FromRoute] int id,
        [FromQuery] ProductQueryParameters query)
    {
        var products = await _categoryService.GetCategoryProductsAsync(id, query);
        return Ok(ApiResponse<PaginatedResult<ProductResponseDto>>.Ok(products));
    }
}
