using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using toyiyo.todo.Contacts.Dto;
using AutoMapper;

namespace toyiyo.todo.Contacts
{
    /// <summary>
    /// Application service to manage contacts
    /// </summary>
    public class ContactAppService : todoAppServiceBase, IContactAppService
    {
        private readonly IContactManager _contactManager;
        private readonly IRepository<Contact, Guid> _contactRepository;

        public ContactAppService(
            IContactManager contactManager,
            IRepository<Contact, Guid> contactRepository)
        {
            _contactManager = contactManager;
            _contactRepository = contactRepository;
        }

        [UnitOfWork]
        public async Task<ContactDto> CreateAsync(CreateContactDto input)
        {
            var contact = await _contactManager.CreateAsync(
                AbpSession.TenantId.Value,
                input.Name,
                input.Email,
                input.PhoneNumber,
                input.Company
            );

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<ContactDto>(contact);
        }

        [UnitOfWork]
        public async Task<ContactDto> UpdateAsync(UpdateContactDto input)
        {
            var contact = await _contactRepository.GetAsync(input.Id);
            
            await _contactManager.UpdateAsync(
                contact,
                input.Name,
                input.Email,
                input.PhoneNumber,
                input.Company,
                input.Notes
            );

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<ContactDto>(contact);
        }

        [UnitOfWork]
        public async Task DeleteAsync(Guid id)
        {
            var contact = await _contactRepository.GetAsync(id);
            await _contactManager.DeleteAsync(contact);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [UnitOfWork]
        public async Task<ContactDto> GetAsync(Guid id)
        {
            var contact = await _contactRepository.GetAsync(id);
            return ObjectMapper.Map<ContactDto>(contact);
        }
    }
}
