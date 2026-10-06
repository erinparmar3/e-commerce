using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminDashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(ApiResponse<AdminDashboardDto>.Ok(stats));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AdminUserDto>>>> GetUsers()
    {
        var users = await _adminService.GetUsersAsync();
        return Ok(ApiResponse<IEnumerable<AdminUserDto>>.Ok(users));
    }

    [HttpPut("users/{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateUserStatus(
        [FromRoute] int id,
        [FromBody] UpdateUserStatusDto dto)
    {
        var user = await _adminService.UpdateUserStatusAsync(id, dto.IsActive);
        return Ok(ApiResponse<AdminUserDto>.Ok(user, $"User status updated to {(dto.IsActive ? "Active" : "Inactive")}."));
    }
}
