using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace toyiyo.todo.Contacts.Dto
{
    [AutoMap(typeof(Contact))]
    public class ContactDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }
        public string Notes { get; set; }
    }

    public class CreateContactDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }
    }

    public class UpdateContactDto : CreateContactDto
    {
        public Guid Id { get; set; }
        public string Notes { get; set; }
    }
}
