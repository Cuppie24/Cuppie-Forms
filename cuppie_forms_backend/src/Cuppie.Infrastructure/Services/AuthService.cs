﻿using System.Security.Claims;
using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces;
using Cuppie.Domain.Entities;
using Cuppie.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ILogger = Serilog.ILogger;

namespace Cuppie.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly CuppieDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger _logger;

    // Внедряем логгер через DI
    public AuthService(CuppieDbContext dbContext, IConfiguration config, IJwtTokenService jwtTokenService, ILogger logger)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Чтобы убедиться, что логгер не null
    }

    public async Task<OperationResult<UserEntity>> AddUser(RegisterModelDto registerModel)
    {
        var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == registerModel.Username);

        if (user != null)
        {
            _logger.Warning("Попытка зарегистрировать пользователя с уже существующим именем: {Username}", registerModel.Username);
            return OperationResult<UserEntity>.Failure("Пользователь с таким именем уже существует", ErrorCode.Conflict);
        }

        _logger.Information("Регистрация нового пользователя: {Username}", registerModel.Username);

        CryptoService cryptoService = new();
        var passSalt = cryptoService.GenerateSalt();
        user = new UserEntity
        {
            Username = registerModel.Username,
            Salt = passSalt,
            PasswordHash = cryptoService.HashPassword(registerModel.Password, passSalt),
            Email = registerModel.Email
        };

        _dbContext.User.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.Information("Пользователь успешно зарегистрирован: {Username}", registerModel.Username);
        return OperationResult<UserEntity>.Success(user);
    }

    public async Task<OperationResult<string>> IsAuthenticated(LoginModelDto loginModel)
    {
        var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Username == loginModel.Username);

        if (user == null)
        {
            _logger.Warning("Неверный логин или пароль для пользователя: {Username}", loginModel.Username);
            return OperationResult<string>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
        }

        CryptoService cryptoService = new();

        // Проверяем пароль
        var isPasswordValid = cryptoService.VerifyPassword(loginModel.Password, user.PasswordHash, user.Salt);

        if (!isPasswordValid)
        {
            _logger.Warning("Неверный логин или пароль для пользователя: {Username}", loginModel.Username);
            return OperationResult<string>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
        }

        var jwtToken = _jwtTokenService.GenerateJwtToken(user);
        if (string.IsNullOrEmpty(jwtToken))
        {
            _logger.Error("Не удалось сгенерировать JWT токен для пользователя: {Username}", loginModel.Username);
            return OperationResult<string>.Failure("Не удалось сгенерировать JWT токен", ErrorCode.InternalServerError);
        }

        _logger.Information("Сгенерирован токен для пользователя {Username}, длина токена: {TokenLength}", loginModel.Username, jwtToken.Length);

        return OperationResult<string>.Success(jwtToken);
    }

    public OperationResult<string> RefreshToken(string refreshToken)
    {
        _logger.Information("Попытка обновления токена для refreshToken: {RefreshToken}", refreshToken);

        var token = _jwtTokenService.RefreshToken(refreshToken);
        if (!string.IsNullOrEmpty(token.Data))
        {
            _logger.Information("Токен успешно обновлен, новый токен: {NewTokenLength}", token.Data.Length);
            return OperationResult<string>.Success(token.Data);
        }

        _logger.Error("Ошибка при обновлении токена: {ErrorMessage}", token.ErrorCode);
        return OperationResult<string>.Failure("Ошибка при обновлении токена", token.ErrorCode);
    }

    public OperationResult<SafeUserDataDto> GetCurrentUserData(string token)
    {
        try
        {
            var principal = _jwtTokenService.ExtractClaimsPrincipal(token);
            var result = new SafeUserDataDto
            {
                Username = principal.FindFirst(ClaimTypes.Name)?.Value,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value
            };
        
            return OperationResult<SafeUserDataDto>.Success(result);
        }
        catch (Exception e)
        {
            string message = $"Ошибка при извлечении данных из токена {e.Message}";
            _logger.Error(message + "\r\n" + e.StackTrace, e);;
            return OperationResult<SafeUserDataDto>.Failure(message, ErrorCode.InternalServerError);;
        }
    }
}
