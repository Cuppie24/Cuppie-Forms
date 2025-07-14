using System.Security.Claims;
using Cuppie.Application.DTOs;
using Cuppie.Application.Interfaces.DAO;
using Cuppie.Application.Interfaces.Services;
using Cuppie.Domain.Entities;
using Cuppie.Infrastructure.DAO;
using Cuppie.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ILogger = Serilog.ILogger;

namespace Cuppie.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICryptoService _cryptoService;
    private readonly ILogger _logger;
    private readonly IUserDao _userDao;
    private readonly IRefreshTokenDao _refreshTokenDao;
    private const int RefreshTokenLength = 64;

    // Внедряем логгер через DI
    

    public async Task<OperationResult<AuthResponseDto>> RegisterAsync(AuthRequestDto authRequest)
    { 
        if(string.IsNullOrWhiteSpace(authRequest.Username) || string.IsNullOrWhiteSpace(authRequest.Password) 
                                                           || string.IsNullOrWhiteSpace(authRequest.Email))
            return OperationResult<AuthResponseDto>.Failure("Неверные входные данные", ErrorCode.BadRequest);
        
        //получаем пользователя по имени
        var fetchResult = await _userDao.GetUserByUsernameAsync(authRequest.Username);
        
        if (fetchResult.IsSuccess)
        {
            _logger.Warning("Попытка зарегистрировать пользователя с уже существующим именем: {Username}", 
                authRequest.Username);
            return OperationResult<AuthResponseDto>.Failure("Пользователь с таким именем уже существует", 
                ErrorCode.Conflict);
        }//return если пользователь с таким именем уже существует

        if (fetchResult.ErrorCode != ErrorCode.NotFound)
        {
            return OperationResult<AuthResponseDto>.Failure(fetchResult.ErrorMessage,
                fetchResult.ErrorCode);
        }//другие случаи ошибки при получении пользователя из БД
        
        _logger.Information("Попытка регистрация нового пользователя: {Username}", authRequest.Username);

        var passSalt = _cryptoService.GenerateSalt();
        var userEntity = new UserEntity
        {
            Username = authRequest.Username,
            Salt = passSalt,
            PasswordHash = _cryptoService.HashPassword(authRequest.Password, passSalt),
            Email = authRequest.Email
        };
        
        var addUserResult = await _userDao.AddUserAsync(userEntity);
        if (!addUserResult.IsSuccess)
            return OperationResult<AuthResponseDto>.Failure(addUserResult.ErrorMessage, addUserResult.ErrorCode);
        _logger.Information("Пользователь успешно зарегистрирован: {Username} Id: {Id}",
            authRequest.Username, addUserResult.Data.Id);
        var user = addUserResult.Data;

        var getTokensRequestDto = new GetTokensRequestDto()
        {
            UserId = user.Id,
            Ip = authRequest.Ip
        };
        if (string.IsNullOrWhiteSpace(authRequest.OldRefreshToken))
            getTokensRequestDto.OldRefreshToken = authRequest.OldRefreshToken;
        
        var getTokensResult = await GetNewTokens(getTokensRequestDto);
        if (!getTokensResult.IsSuccess)
            return OperationResult<AuthResponseDto>.Failure(getTokensResult.ErrorMessage, getTokensResult.ErrorCode);
        ;
        var result = new AuthResponseDto()
        {
            User = new SafeUserDataDto()
            {
                Id = addUserResult.Data.Id,
                Username = addUserResult.Data.Username,
                Email = addUserResult.Data.Email
            },
            JwtToken = getTokensResult.Data.JwtToken,
            RefreshToken = getTokensResult.Data.RefreshToken
        };
        return OperationResult<AuthResponseDto>.Success(result);
    }

    public async Task<OperationResult<AuthResponseDto>> LoginAsync(AuthRequestDto authRequest)
    {
        // проверяем валидность данных
        if (string.IsNullOrWhiteSpace(authRequest.Username) || string.IsNullOrWhiteSpace(authRequest.Password))
            return OperationResult<AuthResponseDto>.Failure("Неверные входные данные", ErrorCode.ValidationError);
        // Получаем пользователя из БД по имени
        var fetchResult = await _userDao.GetUserByUsernameAsync(authRequest.Username);
        
        if (fetchResult.ErrorCode == ErrorCode.NotFound) 
        {
            _logger.Warning("Неверный логин или пароль для пользователя: {Username}", authRequest.Username);
            return OperationResult<AuthResponseDto>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
        }// Если пользователь не найден возвращаем сообщение, что неверный логин или пароль
        
        if (!fetchResult.IsSuccess)
        {
            return OperationResult<AuthResponseDto>.Failure(
                fetchResult.ErrorMessage, ErrorCode.UnknownError);
        }
        
        var user = fetchResult.Data;
        var isPasswordValid = _cryptoService.VerifyPassword(authRequest.Password, user.PasswordHash, user.Salt);
        if (!isPasswordValid)
        {
            _logger.Warning("Неверный логин или пароль для пользователя: {Username}", authRequest.Username);
            return OperationResult<AuthResponseDto>.Failure("Неверный логин или пароль", ErrorCode.Unauthorized);
        } //если пароль не верный возвращаем соответствующее сообщение

        var getTokensRequestDto = new GetTokensRequestDto()
        {
            UserId = user.Id,
            Ip = authRequest.Ip,
        };
        if (!string.IsNullOrWhiteSpace(authRequest.OldRefreshToken))
            getTokensRequestDto.OldRefreshToken = authRequest.OldRefreshToken;
        
        var getTokensResult = await GetNewTokens(getTokensRequestDto);
        if (!getTokensResult.IsSuccess)
            return OperationResult<AuthResponseDto>.Failure(getTokensResult.ErrorMessage, getTokensResult.ErrorCode);
        var result = new AuthResponseDto()
        {
            User = new SafeUserDataDto()
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            },
            JwtToken = getTokensResult.Data.JwtToken,
            RefreshToken = getTokensResult.Data.RefreshToken
        };
        return OperationResult<AuthResponseDto>.Success(result);
    }

    private async Task<OperationResult<AuthResponseDto>> GetNewTokens(GetTokensRequestDto requestDto)
    {
        //получаем пользователя по имени пользователя
        var getUserResult = await _userDao.GetUserByIdAsync(requestDto.UserId);
        if (!getUserResult.IsSuccess)
            return OperationResult<AuthResponseDto>.Failure(getUserResult.ErrorMessage, getUserResult.ErrorCode);
        var user = getUserResult.Data;
        
        AuthResponseDto result = new AuthResponseDto();
        
        //генерируем access и refresh токены
        result.JwtToken = _jwtTokenService.GetJwtAccessToken(user);
        result.RefreshToken = _jwtTokenService.GetRefreshToken(RefreshTokenLength);
        if (string.IsNullOrEmpty(result.JwtToken.Token))
        {
            _logger.Error("Не удалось сгенерировать JWT токен для пользователя с id: {id}", user.Id);
            return OperationResult<AuthResponseDto>.Failure("Не удалось сгенерировать JWT токен", ErrorCode.UnknownError);
        }
        if (string.IsNullOrEmpty(result.RefreshToken.Token))
        {
            _logger.Error("Не удалось сгенерировать refresh token для пользователя с id: {id}", user.Id);
            return OperationResult<AuthResponseDto>.Failure("Не удалось сгенерировать refresh token", ErrorCode.UnknownError);
        }
        
        //отзываем старый refresh токен если он был
        if (!string.IsNullOrEmpty(requestDto.OldRefreshToken))
        {
            var revokeOldRefreshTokinResult =
                await _refreshTokenDao.RevokeRefreshTokenAsync(requestDto.OldRefreshToken, requestDto.Ip);
            if (!revokeOldRefreshTokinResult.IsSuccess)
                return OperationResult<AuthResponseDto>.Failure(
                    revokeOldRefreshTokinResult.ErrorMessage, revokeOldRefreshTokinResult.ErrorCode);
        }
        
        //записываем новый refresh токен
        var refreshTokenEntity = new RefreshTokenEntity()
        {
            UserId = user.Id,
            Token = result.RefreshToken.Token,
            CreatedByIp = requestDto.Ip,
            CreatedAt = DateTimeOffset.UtcNow,
            Expires = DateTimeOffset.UtcNow.AddMinutes(result.RefreshToken.ExpiresInMinutes)
        };
        var refreshTokenAddResult = await _refreshTokenDao.AddRefreshTokenAsync(refreshTokenEntity);
        if (!refreshTokenAddResult.IsSuccess)
        {
            return OperationResult<AuthResponseDto>.Failure(refreshTokenAddResult.ErrorMessage,
                refreshTokenAddResult.ErrorCode);
        }
        
        _logger.Information("Сгенерированы токены для пользователя с ID {id}. Длина Jwt токена: {JwtLength}, Длина Refresh токена: {RefreshLength}",
            user.Username,
            result.JwtToken.Token.Length,
            result.RefreshToken.Token.Length);
        return OperationResult<AuthResponseDto>.Success(result);
    }
    public async Task<OperationResult<AuthResponseDto>> RefreshAccessTokenAsync(string refreshToken, string ip)
    {
        try
        {
            var fetchRefreshTokenResult = await _refreshTokenDao.GetByTokenAsync(refreshToken);
            if (fetchRefreshTokenResult.ErrorCode == ErrorCode.NotFound)
                return OperationResult<AuthResponseDto>.Failure("Refresh token не найден", ErrorCode.NotFound);
            
            var refreshTokenEntity = fetchRefreshTokenResult.Data;
            if (refreshTokenEntity.Expires < DateTimeOffset.UtcNow)
            {
                _logger.Warning("Попытка использовать просроченный refresh token с IP {IP}.  Истёк: {IsExpired}", 
                    ip ,refreshTokenEntity.Expires < DateTimeOffset.UtcNow);
                return OperationResult<AuthResponseDto>.Failure("Просроченный refresh token", ErrorCode.Unauthorized);
            }

            if (refreshTokenEntity.IsRevoked)
            {
                _logger.Warning("Попытка использовать отозванный refresh token с IP. {IP}", ip);
                //логика обработки(потенциальная атака)
                return OperationResult<AuthResponseDto>.Failure("Попытка использовать отозванный refresh token",
                    ErrorCode.BadRequest);
            }
            
            var fetchUserResult = await _userDao.GetUserByIdAsync(refreshTokenEntity.UserId);
            if (fetchUserResult.ErrorCode == ErrorCode.NotFound)
            {
                return OperationResult<AuthResponseDto>.Failure(
                    fetchUserResult.ErrorMessage, ErrorCode.NotFound);
            }
            if (!fetchUserResult.IsSuccess)
            {
                return OperationResult<AuthResponseDto>.Failure(
                    fetchUserResult.ErrorMessage, ErrorCode.UnknownError);;
            }
            var user = fetchUserResult.Data;

            var getTokensRequestDto = new GetTokensRequestDto()
            {
                UserId = user.Id,
                OldRefreshToken = refreshToken,
                Ip = ip
            };
            var getTokensResult = await GetNewTokens(getTokensRequestDto);
            if(!getTokensResult.IsSuccess)
                return OperationResult<AuthResponseDto>.Failure(getTokensResult.ErrorMessage, getTokensResult.ErrorCode);
            
            var newJwtToken = getTokensResult.Data.JwtToken;
            var newRefreshToken = getTokensResult.Data.RefreshToken;
            AuthResponseDto result = new()
            {
                User = new SafeUserDataDto()
                {
                    Id = fetchUserResult.Data.Id,
                    Username = fetchUserResult.Data.Username,
                    Email = fetchUserResult.Data.Email
                },
                JwtToken = newJwtToken,
                RefreshToken = newRefreshToken
            };
            
            return OperationResult<AuthResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            var message = $"Произошла ошибка при обновлении токена: {ex.Message}";
            _logger.Error(message);
            return OperationResult<AuthResponseDto>.Failure(message, ErrorCode.UnknownError);
        }
    }

    public OperationResult<SafeUserDataDto> GetCurrentUserData(string token)
    {
        try
        {
            var extractResult = _jwtTokenService.ExtractClaimsPrincipal(token);
            if (extractResult.ErrorCode == ErrorCode.Unauthorized)
                return OperationResult<SafeUserDataDto>.Failure("Невалидный jwt токен", ErrorCode.Unauthorized);
            if (!extractResult.IsSuccess)
                return OperationResult<SafeUserDataDto>.Failure("Ошибка при извлечении данных из jwt токена",
                    ErrorCode.UnknownError);

            if (Int32.TryParse(extractResult.Data.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                var username = extractResult.Data.FindFirst(ClaimTypes.Name)?.Value;
                var email = extractResult.Data.FindFirst(ClaimTypes.Email)?.Value;
                if(username is null || email is null)
                    return OperationResult<SafeUserDataDto>.Failure("Данные полученные из токена невалидны", 
                        ErrorCode.ValidationError);
            
                var result = new SafeUserDataDto
                {
                    Id = userId,
                    Username = username,
                    Email = email
                };
                return OperationResult<SafeUserDataDto>.Success(result);
            }
            else
                return OperationResult<SafeUserDataDto>.Failure("Неверный формат Id пользователя", ErrorCode.ValidationError);
        }
        catch (Exception e)
        {
            var message = $"Ошибка при извлечении данных из токена {e.Message}";
            _logger.Error(message + "\r\n" + e.StackTrace, e);;
            return OperationResult<SafeUserDataDto>.Failure(message, ErrorCode.UnknownError);;
        }
    }
    
    public AuthService(IJwtTokenService jwtTokenService,
        ICryptoService cryptoService,
        ILogger logger,  
        IUserDao userDao, 
        IRefreshTokenDao refreshTokenDao)
    {
        _cryptoService = cryptoService;
        _userDao = userDao;
        _refreshTokenDao = refreshTokenDao;
        _jwtTokenService = jwtTokenService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Чтобы убедиться, что логгер не null
    }
}
