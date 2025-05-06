using Cuppie.Domain.Entities;
using Cuppie.Application.DTOs;

namespace Cuppie.Application.Interfaces;

public interface IAuthService
{
    Task<OperationResult<UserEntity>> AddUser(RegisterModelDto registerModel);
    Task<OperationResult<string>> IsAuthenticated(LoginModelDto loginModel);
    OperationResult<string> RefreshToken(string refreshToken);
    OperationResult<SafeUserDataDto> GetCurrentUserData(string token);
}