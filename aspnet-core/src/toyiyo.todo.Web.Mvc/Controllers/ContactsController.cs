using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Controllers;
using toyiyo.todo.Contacts;
using toyiyo.todo.Contacts.Dto;

namespace toyiyo.todo.Web.Controllers
{
    public class ContactsController : todoControllerBase
    {
        private readonly IContactAppService _contactAppService;

        public ContactsController(IContactAppService contactAppService)
        {
            _contactAppService = contactAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View(new CreateContactDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateContactDto input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            await _contactAppService.CreateAsync(input);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var contact = await _contactAppService.GetAsync(id);
            var model = ObjectMapper.Map<UpdateContactDto>(contact);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateContactDto input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            await _contactAppService.UpdateAsync(input);
            return RedirectToAction(nameof(Index));
        }
    }
}
