// BarbApp.Infrastructure/Services/TenantContext.cs
using BarbApp.Domain.Interfaces;

namespace BarbApp.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private static readonly AsyncLocal<TenantInfo?> _currentTenant = new();

    public Guid? BarbeariaId => _currentTenant.Value?.BarbeariaId;
    public string? UniqueCode => _currentTenant.Value?.UniqueCode;
    public bool IsAdminCentral => string.IsNullOrEmpty(_currentTenant.Value?.UniqueCode);
    public string UserId => _currentTenant.Value?.UserId ?? string.Empty;
    public string Role => _currentTenant.Value?.Role ?? string.Empty;

    public void SetContext(string userId, string role, Guid? barbeariaId, string? barbeariaCode)
    {
        _currentTenant.Value = new TenantInfo
        {
            UserId = userId,
            Role = role,
            BarbeariaId = barbeariaId,
            UniqueCode = barbeariaCode
        };
    }

    public void Clear()
    {
        _currentTenant.Value = null;
    }

    private class TenantInfo
    {
        public Guid? BarbeariaId { get; init; }
        public string? UniqueCode { get; init; }
        public string UserId { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}