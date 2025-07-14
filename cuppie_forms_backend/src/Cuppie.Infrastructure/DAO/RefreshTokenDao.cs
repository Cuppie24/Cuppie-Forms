using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces.DAO;
using Cuppie.Domain.Entities;
using Cuppie.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cuppie.Infrastructure.DAO;

public class RefreshTokenDao(CuppieDbContext dbContext, Serilog.ILogger logger) : IRefreshTokenDao
{
    public async Task<OperationResult<string>> AddRefreshTokenAsync(RefreshTokenEntity tokenEntity)
    {
        try
        {
            dbContext.RefreshToken.Add(tokenEntity);
            await dbContext.SaveChangesAsync();
            var successMessage = $"Refresh token для пользователя {tokenEntity.UserId} по Ip {tokenEntity.CreatedByIp}";
            logger.Information(successMessage);
            return OperationResult<string>.Success(successMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Ошибка при попытке записи refresh токена в БД: {ex.Message}";
            logger.Error(errorMessage);
            return OperationResult<string>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<string>> RevokeRefreshTokenAsync(string token, string revokedByIp)
    {
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult<string>.Failure("Token не может быть пустым", ErrorCode.ValidationError);

        if (string.IsNullOrWhiteSpace(revokedByIp))
            return OperationResult<string>.Failure("IP адрес не может быть пустым", ErrorCode.ValidationError);

        try
        {
            var refreshTokenToRevoke = await dbContext.RefreshToken.FirstOrDefaultAsync(t => t.Token == token);
            if (refreshTokenToRevoke is null)
                return OperationResult<string>.Failure("Refresh token с таким значением не найден", ErrorCode.NotFound);

            if (refreshTokenToRevoke.IsRevoked)
                return OperationResult<string>.Failure("Токен уже отозван", ErrorCode.Conflict);

            refreshTokenToRevoke.IsRevoked = true;
            refreshTokenToRevoke.RevokedByIp = revokedByIp;
            refreshTokenToRevoke.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();

            var successMessage = $"Refresh token отозван успешно для пользователя {refreshTokenToRevoke.UserId} по IP {revokedByIp}";
            logger.Information(successMessage);
            return OperationResult<string>.Success(successMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка при попытке отозвать refresh token в БД: {ex.Message}";
            logger.Error(errorMessage);
            return OperationResult<string>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<RefreshTokenEntity>> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult<RefreshTokenEntity>.Failure("Token не может быть пустым", ErrorCode.ValidationError);

        try
        {
            var tokenEntity = await dbContext.RefreshToken.AsNoTracking().FirstOrDefaultAsync(t => t.Token == token);
            if (tokenEntity is null)
                return OperationResult<RefreshTokenEntity>.Failure("Refresh token с таким значением не найден", ErrorCode.NotFound);

            return OperationResult<RefreshTokenEntity>.Success(tokenEntity);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка при получении refresh token из БД: {ex.Message}";
            logger.Error(errorMessage);
            return OperationResult<RefreshTokenEntity>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }

    public async Task<OperationResult<IEnumerable<RefreshTokenEntity>>> GetAllByUserIdAsync(int userId)
    {
        if (userId <= 0)
            return OperationResult<IEnumerable<RefreshTokenEntity>>.Failure("Некорректный идентификатор пользователя", ErrorCode.ValidationError);

        try
        {
            var refreshTokens = await dbContext.RefreshToken
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (refreshTokens.Count == 0)
                return OperationResult<IEnumerable<RefreshTokenEntity>>.Failure("Refresh token с таким значением не найдены", ErrorCode.NotFound);

            return OperationResult<IEnumerable<RefreshTokenEntity>>.Success(refreshTokens);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Произошла ошибка при извлечении refresh token из БД: {ex.Message}";
            logger.Error(errorMessage);
            return OperationResult<IEnumerable<RefreshTokenEntity>>.Failure(errorMessage, ErrorCode.UnknownError);
        }
    }
}
