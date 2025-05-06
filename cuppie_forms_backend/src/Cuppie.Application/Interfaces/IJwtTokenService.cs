using System.Security.Claims;
using Cuppie.Application.DTOs;
using Cuppie.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Cuppie.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateJwtToken(UserEntity userEntity);
    string GenerateJwtToken(Claim[] claims);
    SigningCredentials GetSigningCredentials();
    ClaimsPrincipal ExtractClaimsPrincipal(string token);
    OperationResult<string> RefreshToken(string refreshToken);
}