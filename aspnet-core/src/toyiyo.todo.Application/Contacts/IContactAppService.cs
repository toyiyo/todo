using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using toyiyo.todo.Contacts.Dto;

namespace toyiyo.todo.Contacts
{
    public interface IContactAppService : IApplicationService
    {
        Task<ContactDto> CreateAsync(CreateContactDto input);
        Task<ContactDto> UpdateAsync(UpdateContactDto input);
        Task DeleteAsync(Guid id);
        Task<ContactDto> GetAsync(Guid id);
    }
}
