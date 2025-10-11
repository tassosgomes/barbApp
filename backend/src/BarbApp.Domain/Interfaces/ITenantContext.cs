// BarbApp.Domain/Interfaces/ITenantContext.cs
namespace BarbApp.Domain.Interfaces;

public interface ITenantContext
{
    Guid? BarbeariaId { get; }
    string? BarbeariaCode { get; }
    bool IsAdminCentral { get; }
    string UserId { get; }
    string Role { get; }
    
    void SetContext(string userId, string role, Guid? barbeariaId, string? barbeariaCode);
    void Clear();
}