
namespace BarbApp.Domain.Exceptions
{
    public class DuplicateDocumentException : DomainException
    {
        public DuplicateDocumentException(string message) : base(message)
        {
        }
    }
}
