
namespace BarbApp.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; private set; }
        public string ZipCode { get; private set; }
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string? Complement { get; private set; }
        public string Neighborhood { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }

        private Address() 
        {
            ZipCode = null!;
            Street = null!;
            Number = null!;
            Neighborhood = null!;
            City = null!;
            State = null!;
        } // EF Core

        public static Address Create(
            string zipCode,
            string street,
            string number,
            string? complement,
            string neighborhood,
            string city,
            string state)
        {
            return new Address
            {
                Id = Guid.NewGuid(),
                ZipCode = zipCode,
                Street = street,
                Number = number,
                Complement = complement,
                Neighborhood = neighborhood,
                City = city,
                State = state
            };
        }

        public void Update(
            string zipCode,
            string street,
            string number,
            string? complement,
            string neighborhood,
            string city,
            string state)
        {
            ZipCode = zipCode;
            Street = street;
            Number = number;
            Complement = complement;
            Neighborhood = neighborhood;
            City = city;
            State = state;
        }
    }
}
