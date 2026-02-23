using System;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;
namespace OMAB.Application.Interfaces;

public interface IUserAccessor
{
    int GetCurrentUserId();
    Task<User> GetCurrentUserAsync();
    bool CanViewUser(UserRole targetUserRole, UserRole? currentUserRole = null);
}
