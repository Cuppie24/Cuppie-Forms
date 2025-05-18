using System.Security.Claims;
using Cuppie.Application.DTOs;
using Cuppie.Domain.Entities;

namespace Cuppie.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateJwtToken(UserEntity userEntity);
    ClaimsPrincipal ExtractClaimsPrincipal(string token);
    OperationResult<string> RefreshToken(string refreshToken);
}