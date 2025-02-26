using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace toyiyo.todo.Contacts
{
    public interface IContactManager : IDomainService
    {
        Task<Contact> CreateAsync(int tenantId, string name, string email, string phoneNumber, string company);
        Task<Contact> UpdateAsync(Contact contact, string name, string email, string phoneNumber, string company, string notes);
        Task DeleteAsync(Contact contact);
        Task<Contact> GetByIdAsync(Guid id);
    }
}
