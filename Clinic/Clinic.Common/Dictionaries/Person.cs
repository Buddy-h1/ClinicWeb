namespace Clinic.Common.Dictionaries
{
    public class Person
    {
        public int? Id { get; set; }
        public string FirstName { get; set; } = String.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = String.Empty;
    }
}