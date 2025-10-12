
using BarbApp.Domain.ValueObjects;

namespace BarbApp.Domain.Entities
{
    public class Barbershop
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Document Document { get; private set; } // Value Object
        public string Phone { get; private set; }
        public string OwnerName { get; private set; }
        public string Email { get; private set; }
        public UniqueCode Code { get; private set; } // Value Object
        public bool IsActive { get; private set; }
        public Guid AddressId { get; private set; }
        public Address Address { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; }
        public string UpdatedBy { get; private set; }

        private Barbershop() 
        {
            Name = null!;
            Document = null!;
            Phone = null!;
            OwnerName = null!;
            Email = null!;
            Code = null!;
            Address = null!;
            CreatedBy = null!;
            UpdatedBy = null!;
        } // EF Core

        public static Barbershop Create(
            string name,
            Document document,
            string phone,
            string ownerName,
            string email,
            Address address,
            UniqueCode code,
            string createdBy)
        {
            // Validações de domínio
            var barbershop = new Barbershop
            {
                Id = Guid.NewGuid(),
                Name = name,
                Document = document,
                Phone = phone,
                OwnerName = ownerName,
                Email = email,
                Address = address,
                Code = code,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };
            return barbershop;
        }

        public void Update(
            string name,
            string phone,
            string ownerName,
            string email,
            Address address,
            string updatedBy)
        {
            Name = name;
            Phone = phone;
            OwnerName = ownerName;
            Email = email;
            Address = address;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {            
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
