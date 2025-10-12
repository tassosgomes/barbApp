
namespace BarbApp.Domain.Exceptions
{
    public class InvalidDocumentException : DomainException
    {
        public InvalidDocumentException(string message) : base(message)
        {
        }
    }
}
