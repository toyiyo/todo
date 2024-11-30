(function ($) {
  const _invitationService = abp.services.app.userInvitation;
  const l = abp.localization.getSource('todo');
  const _$table = $('#InvitationsTable');
  const _advancedSearchKeyword = document.getElementById('keyword');
  const _btnAdvancedSearch = document.getElementById('btnAdvancedSearch');
  const _btnClearSearch = document.getElementById('btnClearSearch');

  const invitationsTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    select: true,
    listAction: {
      ajaxFunction: _invitationService.getAll,
      inputFilter: function () {
        return {
            keyword: _advancedSearchKeyword.value
        };
      },
    },
    buttons: [],
    columnDefs: [
      {
        targets: 0,
        data: 'email',
        className: "email",
      },
      {
        targets: 1,
        data: 'expirationDate',
        defaultContent: '',
        render: (data, type, row, meta) => {
            if (!data) {
                return 'never';
            }
            const expirationDate = moment(data).format('LLL');
            return `<span title="expires on ${expirationDate}">${expirationDate}</span>`;
        }
      },
      {
        targets: 2,
        data: "acceptedDate",
        render: (data, type, row, meta) => {
            if (!data) {
                return l('NotAccepted');
            }
            const acceptedDate = moment(data).format('LLL');
            return `<span title="accepted on ${acceptedDate}">${acceptedDate}</span>`;
        }
      }
    ],
  });

  _advancedSearchKeyword.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      invitationsTable.ajax.reload();
    }
  });

 _btnAdvancedSearch.addEventListener('click', (e) => {
    e.preventDefault();
    invitationsTable.ajax.reload();
  });

  _btnClearSearch.addEventListener('click', (e) => {
    e.preventDefault();
    _advancedSearchKeyword.value = '';
    invitationsTable.ajax.reload();
  });

})(jQuery);
