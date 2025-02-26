(function () {
    $(function () {
        var _$contactsTable = $('#ContactsTable');
        var _contactService = abp.services.app.contact;

        var dataTable = _$contactsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _contactService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#ContactsSearchForm').serializeFormToObject()
                    };
                }
            },
            columnDefs: [
                {
                    targets: 0,
                    data: 'name'
                },
                {
                    targets: 1,
                    data: 'email'
                },
                {
                    targets: 2,
                    data: 'phoneNumber'
                },
                {
                    targets: 3,
                    data: 'company'
                },
                {
                    targets: 4,
                    data: null,
                    sortable: false,
                    autoWidth: false,
                    defaultContent: '',
                    render: (data, type, row, meta) => {
                        return [
                            `<div class="dropdown">`,
                            `   <button class="btn btn-secondary dropdown-toggle" type="button" data-toggle="dropdown">`,
                            `       <i class="fas fa-ellipsis-h"></i>`,
                            `   </button>`,
                            `   <div class="dropdown-menu">`,
                            `       <a class="dropdown-item edit-contact" href="/Contacts/Edit/${row.id}">`,
                            `           <i class="fas fa-pencil-alt"></i> ${app.localize('Edit')}`,
                            `       </a>`,
                            `       <a class="dropdown-item delete-contact" href="#" data-contact-id="${row.id}" data-contact-name="${row.name}">`,
                            `           <i class="fas fa-trash"></i> ${app.localize('Delete')}`,
                            `       </a>`,
                            `   </div>`,
                            `</div>`
                        ].join('');
                    }
                }
            ]
        });

        _$contactsTable.on('click', '.delete-contact', function () {
            var contactId = $(this).attr("data-contact-id");
            var contactName = $(this).attr('data-contact-name');

            deleteContact(contactId, contactName);
        });

        function deleteContact(contactId, contactName) {
            abp.message.confirm(
                abp.utils.formatString(
                    app.localize('AreYouSureWantToDelete'),
                    contactName
                ),
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _contactService.delete({
                            id: contactId
                        }).done(function () {
                            abp.notify.info(app.localize('SuccessfullyDeleted'));
                            dataTable.ajax.reload();
                        });
                    }
                }
            );
        }
    });
})();
