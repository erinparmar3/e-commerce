using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: false);
        return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.Ok(categories));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateCategoryAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CategoryResponseDto>.Ok(category, "Category created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Update(
        [FromRoute] int id,
        [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, dto);
        return Ok(ApiResponse<CategoryResponseDto>.Ok(category, "Category updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return Ok(ApiResponse<bool>.Ok(result, "Category deleted or deactivated successfully."));
    }
}
