
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
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            if (name.Length > 255)
                throw new ArgumentException("Name must be max 255 characters", nameof(name));

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone is required", nameof(phone));

            if (string.IsNullOrWhiteSpace(ownerName))
                throw new ArgumentException("Owner name is required", nameof(ownerName));
            if (ownerName.Length > 255)
                throw new ArgumentException("Owner name must be max 255 characters", nameof(ownerName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (email.Length > 255)
                throw new ArgumentException("Email must be max 255 characters", nameof(email));
            if (!email.Contains("@") || !email.Contains("."))
                throw new ArgumentException("Invalid email format", nameof(email));

            if (string.IsNullOrWhiteSpace(createdBy))
                throw new ArgumentException("CreatedBy is required", nameof(createdBy));

            var barbershop = new Barbershop
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Document = document,
                Phone = phone,
                OwnerName = ownerName.Trim(),
                Email = email.ToLowerInvariant().Trim(),
                Address = address,
                AddressId = address.Id,
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
            AddressId = address.Id;
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
