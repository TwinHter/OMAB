using System;
using Microsoft.AspNetCore.Http;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;
using OMAB.Infrastructure.Persistence;

namespace OMAB.Infrastructure.Services;

public class DevUserAccessor(AppDbContext context, IHttpContextAccessor httpContextAccessor) : IUserAccessor
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
        var headerId = httpContextAccessor.HttpContext?.Request.Headers["x-user-id"].ToString();

        if (int.TryParse(headerId, out int userId))
        {
            return userId;
        }

        return 1;
    }
}
