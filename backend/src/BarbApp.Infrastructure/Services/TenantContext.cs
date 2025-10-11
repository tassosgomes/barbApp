// BarbApp.Infrastructure/Services/TenantContext.cs
using BarbApp.Domain.Interfaces;

namespace BarbApp.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid? BarbeariaId { get; private set; }
    public string? BarbeariaCode { get; private set; }
    public bool IsAdminCentral => string.IsNullOrEmpty(BarbeariaCode);
    public string UserId { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    public void SetContext(string userId, string role, Guid? barbeariaId, string? barbeariaCode)
    {
        UserId = userId;
        Role = role;
        BarbeariaId = barbeariaId;
        BarbeariaCode = barbeariaCode;
    }
}