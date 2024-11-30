(function ($) {
  const _invitationService = abp.services.app.userInvitation;
  const l = abp.localization.getSource("todo");
  const _$table = $("#InvitationsTable");
  const _advancedSearchKeyword = document.getElementById("keyword");
  const _btnAdvancedSearch = document.getElementById("btnAdvancedSearch");
  const _btnClearSearch = document.getElementById("btnClearSearch");
  const _brnSendInvitations = document.getElementById("sendInvitations");
  const emailInput = document.getElementById("emailInput");
  const emailPills = document.getElementById("emailPills");
  const emailError = document.getElementById("emailError");
  let emails = new Set();

  const invitationsTable = _$table.DataTable({
    paging: true,
    serverSide: true,
    select: true,
    listAction: {
      ajaxFunction: _invitationService.getAll,
      inputFilter: function () {
        return {
          keyword: _advancedSearchKeyword.value,
        };
      },
    },
    buttons: [],
    columnDefs: [
      {
        targets: 0,
        data: "email",
        className: "email",
      },
      {
        targets: 1,
        data: "expirationDate",
        defaultContent: "",
        render: (data, type, row, meta) => {
          if (!data) {
            return "never";
          }
          const expirationDate = moment(data).format("LLL");
          return `<span title="expires on ${expirationDate}">${expirationDate}</span>`;
        },
      },
      {
        targets: 2,
        data: "acceptedDate",
        render: (data, type, row, meta) => {
          if (!data) {
            return l("NotAccepted");
          }
          const acceptedDate = moment(data).format("LLL");
          return `<span title="accepted on ${acceptedDate}">${acceptedDate}</span>`;
        },
      },
    ],
  });

  _advancedSearchKeyword.addEventListener("keypress", (e) => {
    if (e.key === "Enter") {
      e.preventDefault();
      invitationsTable.ajax.reload();
    }
  });

  _btnAdvancedSearch.addEventListener("click", (e) => {
    e.preventDefault();
    invitationsTable.ajax.reload();
  });

  _btnClearSearch.addEventListener("click", (e) => {
    e.preventDefault();
    _advancedSearchKeyword.value = "";
    invitationsTable.ajax.reload();
  });

  const validateEmail = (email) => {
    return email.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/);
  };

  const addEmailPill = (email) => {
    const pill = document.createElement("div");
    pill.className = "email-pill";
    pill.innerHTML = `
          <span>${_.escape(email)}</span>
          <span class="email-pill-remove">&times;</span>
      `;
    pill.querySelector(".email-pill-remove").onclick = () => {
      emails.delete(email);
      pill.remove();
    };
    emailPills.appendChild(pill);
  };

  const processInput = (input) => {
    const rawEmails = input.split(/[,;\s]+/);
    rawEmails.forEach((email) => {
      email = email.trim();
      if (email && validateEmail(email)) {
        if (!emails.has(email)) {
          emails.add(email);
          addEmailPill(email);
        }
      }
    });
    emailInput.value = "";
  };

  emailInput.addEventListener("paste", (e) => {
    e.preventDefault();
    const paste = e.clipboardData.getData("text");
    processInput(paste);
  });

  emailInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter" || e.key === ",") {
      e.preventDefault();
      processInput(emailInput.value);
    }
  });

  _brnSendInvitations.addEventListener("click", (e) => {
    if (emails.size === 0) {
      abp.notify.error(l("PleaseEnterAtLeastOneEmail"));
      return;
    }

    const invitations = Array.from(emails).map((email) => ({ email }));

    abp.ui.setBusy();
    _invitationService.createInvitations(invitations)
      .done(function (result) {
        if (result.errors && result.errors.length > 0) {
          abp.message.warn(
            l("SomeInvitationsFailed") +
              "\n" +
              _.escape(result.errors.join("\n"))
          );
        }
        if (result.invitations && result.invitations.length > 0) {
          abp.notify.success(l("InvitationsSentSuccessfully"));
          emails.clear();
          emailPills.innerHTML = "";
          invitationsTable.ajax.reload();
        }
      })
      .fail(function (err) {
        abp.message.error(err.message || l("ErrorOccurred"));
      })
      .always(function () {
        abp.ui.clearBusy();
      });
  });
})(jQuery);