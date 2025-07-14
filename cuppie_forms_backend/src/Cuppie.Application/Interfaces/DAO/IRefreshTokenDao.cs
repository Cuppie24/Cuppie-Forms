using Cuppie.Application.DTOs;
using Cuppie.Domain.Entities;

namespace Cuppie.Application.Interfaces.DAO;

public interface IRefreshTokenDao
{
    Task<OperationResult<string>> AddRefreshTokenAsync(RefreshTokenEntity tokenEntity);
    Task<OperationResult<string>> RevokeRefreshTokenAsync(string token, string revokedByIp);
    Task<OperationResult<RefreshTokenEntity>> GetByTokenAsync(string token);
    Task<OperationResult<IEnumerable<RefreshTokenEntity>>> GetAllByUserIdAsync(int userId);
}