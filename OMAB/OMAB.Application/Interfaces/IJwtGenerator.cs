using System;
using OMAB.Domain.Entities;

namespace OMAB.Application.Interfaces;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}
