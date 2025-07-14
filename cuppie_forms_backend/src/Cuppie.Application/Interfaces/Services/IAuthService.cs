using Cuppie.Application.DTOs;
using Cuppie.Domain.Entities;

namespace Cuppie.Application.Interfaces.Services;

public interface IAuthService
{
    Task<OperationResult<AuthResponseDto>> RegisterAsync(AuthRequestDto authRequest);
    Task<OperationResult<AuthResponseDto>> LoginAsync(AuthRequestDto authRequest);
    Task<OperationResult<AuthResponseDto>> RefreshAccessTokenAsync(string refreshToken, string ip);
    OperationResult<SafeUserDataDto> GetCurrentUserData(string token);
}