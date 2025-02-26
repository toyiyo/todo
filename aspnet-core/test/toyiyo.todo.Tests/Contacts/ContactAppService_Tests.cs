using System;
using System.Threading.Tasks;
using Shouldly;
using toyiyo.todo.Contacts;
using toyiyo.todo.Contacts.Dto;
using Xunit;

namespace toyiyo.todo.Tests.Contacts
{
    public class ContactAppService_Tests : todoTestBase
    {
        private readonly IContactAppService _contactAppService;

        public ContactAppService_Tests()
        {
            _contactAppService = Resolve<IContactAppService>();
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Should_Create_Contact()
        {
            // Arrange
            var input = new CreateContactDto
            {
                Name = "Test Contact",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Company = "Test Company"
            };

            // Act
            var result = await _contactAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(input.Name);
            result.Email.ShouldBe(input.Email);
            result.PhoneNumber.ShouldBe(input.PhoneNumber);
            result.Company.ShouldBe(input.Company);
        }

        [Fact]
        public async Task Should_Get_Contact()
        {
            // Arrange
            var contact = await CreateTestContact();

            // Act
            var result = await _contactAppService.GetAsync(contact.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(contact.Name);
            result.Email.ShouldBe(contact.Email);
        }

        [Fact]
        public async Task Should_Update_Contact()
        {
            // Arrange
            var contact = await CreateTestContact();
            var input = new UpdateContactDto
            {
                Id = contact.Id,
                Name = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "+9876543210",
                Company = "Updated Company",
                Notes = "Updated Notes"
            };

            // Act
            var result = await _contactAppService.UpdateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(input.Name);
            result.Email.ShouldBe(input.Email);
            result.Notes.ShouldBe(input.Notes);
        }

        [Fact]
        public async Task Should_Delete_Contact()
        {
            // Arrange
            var contact = await CreateTestContact();

            // Act & Assert
            await _contactAppService.DeleteAsync(contact.Id);
            await Assert.ThrowsAsync<Exception>(async () =>
                await _contactAppService.GetAsync(contact.Id)
            );
        }

        private async Task<ContactDto> CreateTestContact()
        {
            return await _contactAppService.CreateAsync(new CreateContactDto
            {
                Name = "Test Contact",
                Email = "test@example.com",
                PhoneNumber = "+1234567890",
                Company = "Test Company"
            });
        }
    }
}
