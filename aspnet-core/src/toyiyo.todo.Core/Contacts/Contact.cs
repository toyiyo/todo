using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Contacts
{
    public class Contact : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public string Name { get; protected set; }
        public string Email { get; protected set; }
        public string PhoneNumber { get; protected set; }
        public string Company { get; protected set; }
        public string Notes { get; protected set; }
        public virtual ICollection<Job> RelatedJobs { get; protected set; }

        protected Contact()
        {
            RelatedJobs = new HashSet<Job>();
        }

        public static Contact Create(int tenantId, string name, string email, string phoneNumber, string company)
        {
            var contact = new Contact
            {
                TenantId = tenantId,
                Name = name,
                Email = email,
                PhoneNumber = phoneNumber,
                Company = company
            };

            contact.Validate();
            return contact;
        }

        public static Contact SetPhone(Contact contact, string phoneNumber)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));   
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
            if (contact.PhoneNumber == phoneNumber)
                throw new ArgumentException("Phone number is already set", nameof(phoneNumber));
            //validate phone number format
            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\+?[1-9]\d{1,14}$"))
                throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));
            contact.PhoneNumber = phoneNumber;
            return contact;
        }

        public void Update(string name, string email, string phoneNumber, string company, string notes)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            Company = company;
            Notes = notes;

            Validate();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name is required");
            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Email is required");
        }
    }
}
