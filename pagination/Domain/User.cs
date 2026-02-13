namespace pagination.Domain
{
    public class User
    {
        public long Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public required string UpdatedBy { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public required string Role { get; set; }
    }
}
