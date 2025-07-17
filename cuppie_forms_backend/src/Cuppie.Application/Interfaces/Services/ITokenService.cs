using System.Security.Claims;
using Cuppie.Application.DTOs;
using Cuppie.Domain.Entities;

namespace Cuppie.Application.Interfaces.Services;

public interface ITokenService
{
    TokenData GetJwtAccessToken(UserEntity userEntity);
    OperationResult<ClaimsPrincipal> ExtractClaimsPrincipal(string token);
    TokenData GetRefreshToken(int size);
}