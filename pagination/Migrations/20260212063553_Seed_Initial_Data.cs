using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pagination.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Initial_Data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const int batchSize = 50;
            int totalUsers = 10000;

            for (int start = 1; start <= totalUsers; start += batchSize)
            {
                int end = Math.Min(start + batchSize - 1, totalUsers);
                var sql = "";

                for (int i = start; i <= end; i++)
                {
                    var now = DateTimeOffset.UtcNow;
                    sql += $@" INSERT INTO Pagination.Users
                    (Username, Email, FirstName, LastName, DateOfBirth, PhoneNumber, Address, City, State, ZipCode, Country, ProfilePictureUrl, IsEmailVerified, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, LastLoginAt, Role)
                    VALUES
                    ('user{i}', 'user{i}@example.com', 'First{i}', 'Last{i}', '1990-01-01', '1234567890', 'Address {i}', 'City {i}', 'State {i}', '0000{i}', 'Country {i}', NULL, 1, 1, '{now}', 'Seeder', '{now}', 'Seeder', NULL, 'User');";
                }

                migrationBuilder.Sql(sql);
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Users WHERE CreatedBy = 'Seeder';");
        }
    }
}
