namespace NexaCommerce.Contracts.Identity.Responses;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    UserResponseDto User
);
