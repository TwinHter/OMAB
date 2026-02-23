using System;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;
using Microsoft.AspNetCore.Http;
using OMAB.Infrastructure.Persistence;
using System.Security.Claims;
using OMAB.Domain.Enums;


namespace OMAB.Infrastructure.Persistence.Services;

public class UserAccessor(IHttpContextAccessor httpContextAccessor, AppDbContext context) : IUserAccessor
{
    public bool CanViewUser(UserRole targetUserRole, UserRole? currentUserRole = null)
    {
        if (currentUserRole == null)
            currentUserRole = GetCurrentUserAsync().Result.UserRole;

        if (currentUserRole == UserRole.Admin)
            return true;

        if (currentUserRole == UserRole.Doctor)
            return targetUserRole is UserRole.Doctor or UserRole.Patient;

        if (currentUserRole == UserRole.Patient)
            return targetUserRole == UserRole.Doctor;

        return false;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        return await context.Users.FindAsync(userId) ?? throw new UnauthorizedAccessException("Có lỗi khi lấy thông tin người dùng.");
    }

    public int GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc Token không hợp lệ.");
        }

        return userId;
    }
}
