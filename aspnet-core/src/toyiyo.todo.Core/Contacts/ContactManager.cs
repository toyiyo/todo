using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;

namespace toyiyo.todo.Contacts
{
    public class ContactManager : DomainService, IContactManager
    {
        private readonly IRepository<Contact, Guid> _contactRepository;

        public ContactManager(IRepository<Contact, Guid> contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<Contact> CreateAsync(int tenantId, string name, string email, string phoneNumber, string company)
        {
            var contact = Contact.Create(tenantId, name, email, phoneNumber, company);
            return await _contactRepository.InsertAsync(contact);
        }

        public async Task<Contact> UpdateAsync(Contact contact, string name, string email, string phoneNumber, string company, string notes)
        {
            contact.Update(name, email, phoneNumber, company, notes);
            return await _contactRepository.UpdateAsync(contact);
        }

        public async Task DeleteAsync(Contact contact)
        {
            await _contactRepository.DeleteAsync(contact);
        }

        public async Task<Contact> GetByIdAsync(Guid id)
        {
            return await _contactRepository.GetAsync(id);
        }
    }
}
