using Microsoft.AspNetCore.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Security.Cryptography;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(password))
            return false;

        var result = _hasher.VerifyHashedPassword(new object(), hashedPassword, password);
        return result != PasswordVerificationResult.Failed;
    }
}
