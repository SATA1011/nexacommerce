namespace NexaCommerce.Contracts.Identity.Requests;

public sealed record LoginRequestDto(
    string Email,
    string Password
);
