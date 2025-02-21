(function ($) {

    const Editor = toastui.Editor;
    const _$getLinkButton = $('.btn-pane-get-link');
    var _debounceTimer = null;
    const _$subtaskForm = $('#subtasks #AddByTitle');

    document.getElementById("dueDate").setAttribute("min", new Date().toJSON().split('T')[0]);

    const editor = new Editor({
        el: document.querySelector('#Description'),
        initialValue: $('#descriptionFromServer').val(),
        height: '50em',
        initialEditType: 'wysiwyg',
        previewStyle: 'vertical'
    });

    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobEditModal'),
        _$form = _$modal.find('form');


    //JOB
    function save() {
        if (!_$form.valid()) {
            return;
        }

        //serialization works for input types, the description is a div so that we can have markdown functionality.  
        //We need to get the div contents and manually add them to the job object
        var job = _$form.serializeFormToObject();
        job.description = editor.getMarkdown();
        job.dueDate = moment(job.dueDate).endOf('day').utc();
        job.assigneeId = _$form.find('#assigneeSelect').val();;

        abp.ui.setBusy(_$form);

        _jobService.updateAllFields(job).done(() => {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('job.edited', job);
        }).always(() => abp.ui.clearBusy(_$form));
    }

    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });
    //this is a debounce function that will wait 1 second after the user stops typing to update the description
    $('#Description').on('input', function () {
        if (_debounceTimer) {
            clearTimeout(_debounceTimer);
        }
        _debounceTimer = setTimeout(function () {
            var job = _$form.serializeFormToObject();
            job.description = editor.getMarkdown();
            _jobService.setDescription(job);
        }, 700);
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });

    _$modal.on('hidden.bs.modal', function () {
        //when the modal is hidden, change the url history to the listing page
        const projectId = $('#ProjectId').val();
        const nextUrl = '/projects/' + projectId + '/jobs/';
        const nextTitle = 'jobs';
        const nextState = { additionalInformation: 'jobs' };
        // This will create a new entry in the browser's history, without reloading
        window.history.pushState(nextState, nextTitle, nextUrl);
    });

    _$getLinkButton.on('click', function () {
        navigator.clipboard.writeText(window.location.href);
        abp.notify.info('link is in your clipboard');
    });

    //SUBTASKS
    function Subtask(title, dueDate, parentId, id, level) {
        this.title = title;
        this.dueDate = dueDate;
        this.parentId = parentId;
        this.id = id;
        this.level = level;
    }

    function createSubtask() {
        const title = _$subtaskForm.find('#add-by-title-input').val();
        const dueDate = _$subtaskForm.find('#due-date-button').val();
        const parentId = _$form.find('input[name="id"]').val();
        const projectId = $('#ProjectId').val();
        const level = $('#subtasks #level').data('level');

        if (!title) {
            abp.notify.warn(l('TitleIsRequired'));
            return;
        }

        abp.ui.setBusy(_$subtaskForm);

        _jobService.create({
            title: title,
            dueDate: dueDate,
            parentId: parentId,
            projectId: projectId,
            level: level
        }).done(function (data) {
            abp.notify.info(l('SavedSuccessfully'));
            _$subtaskForm.find('#add-by-title-input').val('');
            _$subtaskForm.find('#due-date-button').val('');
            addSubtaskToTable(new Subtask(title, dueDate, parentId, data.id, data.level));
        }).always(function () {
            abp.ui.clearBusy(_$subtaskForm);
        });
    }
    //create a subtask when the button is clicked
    _$subtaskForm.find('.create-by-title-button').on('click', function () {
        createSubtask();
    });
    //create a subtask when the enter key is pressed
    _$subtaskForm.find('#add-by-title-input').on('keydown', function (event) {
        if (event.keyCode === 13) { // Enter key
            event.preventDefault();
            createSubtask();
        }
    });
    //show hide subtask input field
    $('table').on('click', '.subtask-text', function () {
        var $text = $(this);
        var $input = $text.next('.subtask-input');
        $text.addClass('d-none');
        $input.removeClass('d-none');
        $input.focus();
    });

    $('table').on('blur', '.subtask-input', function () {
        var $input = $(this);
        var $text = $input.prev('.subtask-text');
        $input.addClass('d-none');
        $text.removeClass('d-none');
        $text.text($input.val());
    });
    //add subtask to UI when subtask is created
    function addSubtaskToTable(subtask) {
        var $row = $(`
        <tr>
            <td>
                <input type="checkbox" class="subtask-checkbox" data-subtask-id="${subtask.id}" ${subtask.jobStatusId === 2 ? 'checked' : ''} />
            </td> 
            <td class="align-middle w-100">
                <span class="subtask-text form-control border-0 add-todo-input bg-transparent rounded" data-subtask-id="${subtask.id}">${subtask.title}</span>
                <input type="text" class="form-control border-1 add-todo-input bg-transparent rounded subtask-input d-none" value="${subtask.title}" data-subtask-id="${subtask.id}" />
            </td>
            <td>
                <button type="button" class="btn btn-danger btn-sm subtask-delete" data-subtask-id="${subtask.id}">Delete</button>
            </td>
        </tr>
        `);

        $row.appendTo('#subtask-table tbody');
    }
    //set the subtask title when the user stops typing
    $('#subtask-table').on('input', '.subtask-input', function () {
        let $input = $(this);
        if (_debounceTimer) {
            clearTimeout(_debounceTimer);
        }
        _debounceTimer = setTimeout(function () {
            let jobSetSubTaskTitleInputDto = {
                id: $input.data('subtask-id'),
                title: $input.val()
            };
            _jobService.setTitle(jobSetSubTaskTitleInputDto).done(function () { abp.notify.info(l('SavedSuccessfully')); });
        }, 700);
    });

    $('#subtask-table').on('keydown', '.subtask-input', function (event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            let $input = $(this);
            let jobSetSubTaskTitleInputDto = {
                id: $input.data('subtask-id'),
                title: $input.val()
            };
            _jobService.setTitle(jobSetSubTaskTitleInputDto).done(function () { abp.notify.info(l('SavedSuccessfully')); });
        }
    });

    $('#subtask-table').on('change', '.subtask-checkbox', function () {
        let $checkbox = $(this);
        let jobSetSubTaskStatusInputDto = {
            id: $checkbox.data('subtask-id'),
            jobstatus: $checkbox.is(':checked') ? 2 : 0 // Assuming 2 is the ID for "completed" status and 0 is the ID for "open" status
        }

        _jobService.setJobStatus(jobSetSubTaskStatusInputDto).done(function () { abp.notify.info(l('SavedSuccessfully')); });
    });

    $('#subtask-table').on('click', '.subtask-delete', function () {
        let $button = $(this);
        let jobId = $button.data('subtask-id');

        deleteJob(jobId)
            .done(function () {
                abp.notify.info(l('Deleted Successfully'));
                $button.closest('tr').remove();
            });
    });
    //since our service returns IactionResult (httpresponse), it doesn't get ajax wrapping from apb framework, causing a js error if we use the default _jobService.Delete function since it expects a wrapper object
    //https://aspnetboilerplate.com/Pages/Documents/Javascript-API/AJAX?searchKey=MvcAjaxResponse
    const deleteJob = function (id) {
        return $.ajax({
            url: abp.appPath + 'api/services/app/Job/Delete' + abp.utils.buildQueryString([{ name: 'id', value: id }]) + '',
            type: 'DELETE'
        });
    };

    // Notes handling
    function initializeNotes() {
        const _noteService = abp.services.app.note;
        const _$notesContainer = $('#notesContainer');
        const _$addNoteForm = $('#addNoteForm');
        let _currentPage = 1;
        const _pageSize = 10;

        $(document).on('click', '.reply-button', handleReplyButtonClick);
        $(document).on('click', '.cancel-reply', handleCancelReplyClick);
        $(document).on('click', '.delete-button', handleDeleteButtonClick);
        $(document).on('click', '.edit-button', handleEditButtonClick);

        loadNotes(_currentPage);
    }

    // Move loadNotes outside of initializeNotes
    function loadNotes(page = 1) {
        const _noteService = abp.services.app.note;
        const $notesContainer = $('#notesContainer');
        abp.ui.setBusy($notesContainer);

        _noteService.getAll({
            jobId: $('#Id').val(), // Use the correct ID selector
            maxResultCount: 10,
            skipCount: (page - 1) * 10
        }).done(function (result) {
            console.log('Notes loaded:', result); // Debug
            const $notesList = $('#notesList');
            $notesList.empty();
            result.items.forEach(note => {
                const $noteElement = createNoteElement(note);
                $notesList.append($noteElement);
            });
        }).always(function () {
            abp.ui.clearBusy($notesContainer);
        });
    }

    // Update createNoteElement to handle nested replies
    function createNoteElement(note) {
        var $note = $('<div/>', {
            class: 'note mb-3',
            'data-note-id': note.id
        });

        var $header = $('<div/>', {
            class: 'note-header d-flex justify-content-between align-items-center'
        });

        var $authorInfo = $('<div/>');
        $('<strong/>').text(note.authorName).appendTo($authorInfo);
        $('<small/>', {
            class: 'text-muted ml-2'
        }).text(moment(note.creationTime).fromNow()).appendTo($authorInfo);

        var $actions = $('<div/>', {
            class: 'note-actions'
        }).html(getActionsHtml(note));

        var $content = $('<div/>', {
            class: 'note-content'
        }).html(formatNoteContent(note.content));

        $header.append($authorInfo, $actions);
        $note.append($header, $content);

        // Add replies section if there are replies
        if (note.replies && note.replies.length > 0) {
            var $repliesSection = $('<div/>', {
                class: 'replies-section ml-4 mt-2'
            });

            note.replies.forEach(reply => {
                $repliesSection.append(createNoteElement(reply));
            });

            $note.append($repliesSection);
        }

        return $note;
    }

    // Add these functions after the createNoteElement function
    function handleReplyButtonClick(e) {
        e.preventDefault();
        e.stopPropagation();

        const $note = $(this).closest('.note');
        const $replyForm = $(`
            <div class="reply-form mt-2">
                <textarea class="form-control" rows="2" placeholder="${l('WriteReplyHere')}"></textarea>
                <div class="mt-2">
                    <button type="button" class="btn btn-primary btn-sm submit-reply">${l('Reply')}</button>
                    <button type="button" class="btn btn-secondary btn-sm cancel-reply">${l('Cancel')}</button>
                </div>
            </div>
        `);

        // Remove existing reply form if any
        $note.find('.reply-form').remove();

        // Add new reply form
        $note.append($replyForm);
        $replyForm.find('textarea').focus();

        return false;
    }

    function handleCancelReplyClick() {
        $(this).closest('.reply-form').remove();
    }

    $(document).on('click', '.submit-reply', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $replyForm = $(this).closest('.reply-form');
        const $note = $replyForm.closest('.note');
        const content = $replyForm.find('textarea').val();

        if (!content) {
            abp.notify.warn(l('PleaseEnterReply'));
            return;
        }

        const _noteService = abp.services.app.note;
        const noteData = {
            jobId: $('#Id').val(),
            parentNoteId: $note.data('note-id'),
            content: content
        };

        abp.ui.setBusy($replyForm);

        _noteService.create(noteData)
            .done(function (result) {
                const $replyElement = createNoteElement(result);
                // Add the reply before the reply form
                $replyForm.before($replyElement);
                // Remove the reply form
                $replyForm.remove();
                abp.notify.success(l('ReplySaved'));
            })
            .fail(function () {
                abp.notify.error(l('ErrorSavingReply'));
            })
            .always(function () {
                abp.ui.clearBusy($replyForm);
            });
    });

    function refreshNotesList() {
        var $notesList = $('#notesList');
        $notesList.empty();

        _noteService.getAll({
            jobId: $('#JobId').val(),
            maxResultCount: 10,
            skipCount: 0
        }).done(function (result) {
            result.items.forEach(function (note) {
                var $noteElement = createNoteElement(note);
                $notesList.append($noteElement);
            });
        });
    }

    function formatNoteContent(content) {
        try {
            if (typeof marked === 'undefined') {
                console.warn('Marked library not available, falling back to plain text');
                return $('<div/>').text(content).html();
            }

            const escapedContent = $('<div/>').text(content).html();
            return marked.parse(escapedContent);
        } catch (error) {
            console.error('Error formatting note content:', error);
            return $('<div/>').text(content).html();
        }
    }

    function getActionsHtml(note) {
        let currentUserId = abp.session.userId;
        let isAuthor = note.authorId === currentUserId;

        let actions = [];
        actions.push('<button class="btn btn-sm btn-link reply-button">Reply</button>');

        if (isAuthor) {
            actions.push('<button class="btn btn-sm btn-link edit-button">Edit</button>');
            actions.push('<button class="btn btn-sm btn-link delete-button">Delete</button>');
        }

        return actions.join('');
    }

    function handleDeleteButtonClick(e) {
        e.preventDefault();
        e.stopPropagation();
        const $note = $(this).closest('.note');
        const noteId = $note.data('note-id');

        abp.message.confirm(
            l('DeleteNoteConfirmationMessage'),
            l('AreYouSure'),
            function (isConfirmed) {
                if (isConfirmed) {
                    const _noteService = abp.services.app.note;
                    abp.ui.setBusy($note);

                    _noteService.delete(noteId)
                        .done(function () {
                            $note.fadeOut(function () {
                                $(this).remove();
                            });
                            abp.notify.success(l('NoteDeleted'));
                        })
                        .fail(function () {
                            abp.notify.error(l('ErrorDeletingNote'));
                        })
                        .always(function () {
                            abp.ui.clearBusy($note);
                        });
                }
            }
        );
    }

    function handleEditButtonClick(e) {
        e.preventDefault();
        e.stopPropagation();

        const $note = $(this).closest('.note');
        
        // If already editing, return
        if ($note.find('.edit-form').length > 0) {
            return;
        }

        const noteId = $note.data('note-id');
        const initialContent = $note.find('.note-content').text().trim();

        // Hide original content while editing
        $note.find('.note-content').hide();

        // Create an inline edit form
        const $editForm = $(`
            <div class="edit-form mt-2">
                <textarea class="form-control" rows="2">${initialContent}</textarea>
                <div class="mt-2">
                    <button type="button" class="btn btn-primary btn-sm save-edit">Save</button>
                    <button type="button" class="btn btn-secondary btn-sm cancel-edit">Cancel</button>
                </div>
            </div>
        `);

        // Remove any existing handlers before adding new ones
        $note.off('click.noteEdit', '.save-edit, .cancel-edit');

        // Handle cancel
        $note.on('click.noteEdit', '.cancel-edit', function() {
            cleanup();
        });

        // Handle save
        $note.on('click.noteEdit', '.save-edit', function() {
            const updatedContent = $editForm.find('textarea').val();
            if (!updatedContent) {
                abp.notify.warn(l('PleaseEnterNote'));
                return;
            }
            const _noteService = abp.services.app.note;
            abp.ui.setBusy($editForm);

            _noteService.update(noteId, updatedContent)
                .done(function() {
                    $note.find('.note-content').html(formatNoteContent(updatedContent)).show();
                    cleanup();
                    abp.notify.success(l('NoteUpdated'));
                })
                .fail(function(error) {
                    console.error('Update error:', error);
                    abp.notify.error(l('ErrorUpdatingNote'));
                })
                .always(function() {
                    abp.ui.clearBusy($editForm);
                });
        });

        function cleanup() {
            $editForm.remove();
            $note.find('.note-content').show();
            $note.off('click.noteEdit', '.save-edit, .cancel-edit');
        }

        $note.append($editForm);
        $editForm.find('textarea').focus();
    }

    $('#addNoteForm').on('submit', function (e) {
        e.preventDefault();
        let $form = $(this);
        let content = $form.find('textarea').val();

        if (!content) return;

        _noteService.create({
            jobId: $('#JobId').val(),
            content: content
        }).done(function () {
            $form.find('textarea').val('');
            refreshNotesList();
        });
    });

    // Replace the existing note form submit handler with these two handlers
    $('.add-note-button').click(function () {
        handleAddNote();
    });

    $('#addNoteForm textarea').keydown(function (e) {
        if (e.ctrlKey && e.keyCode === 13) {  // Ctrl + Enter
            handleAddNote();
        }
    });

    document.addEventListener('DOMContentLoaded', function () {
        if (typeof marked === 'undefined') {
            console.error('Marked library not loaded');
            return;
        }

        marked.setOptions({
            breaks: true,
            gfm: true,
            sanitize: true
        });
    });
    function handleAddNote() {
        const _noteService = abp.services.app.note;
        const $modal = $('#JobEditModal');
        const $form = $modal.find('#addNoteForm');  // Updated selector
        const $textarea = $('#noteContent');
        const content = $textarea.val();
        const jobId = $('#Id').val();

        // Prevent double submission
        if ($form.data('submitting')) {
            return;
        }

        if (!content) {
            abp.notify.warn(l('PleaseEnterNote'));
            return;
        }

        $form.data('submitting', true);
        abp.ui.setBusy($form);

        let noteData = {
            jobId: jobId,
            content: content
        };

        _noteService.create(noteData)
            .done(function (result) {
                $textarea.val('');
                var $noteElement = createNoteElement(result);
                $('#notesList').prepend($noteElement);
                abp.notify.success(l('NoteSaved'));
            })
            .fail(function (error) {
                console.error('Error creating note:', error);
                abp.notify.error(l('ErrorSavingNote'));
            })
            .always(function () {
                abp.ui.clearBusy($form);
                $form.data('submitting', false);
            });
    }

    // Update the initializeMentions function with a retry mechanism
    function initializeMentions() {
        try {
            const tribute = new Tribute({
                trigger: '@',
                values: function (text, cb) {
                    const userLookupService = abp.services.app.userLookup;
                    userLookupService.searchUsers(text).done(function (users) {
                        const menuItems = users.map(user => ({
                            key: user.userName,
                            value: user.displayName,
                            email: user.emailAddress
                        }));
                        cb(menuItems);
                    });
                },
                menuItemTemplate: function (item) {
                    return `<span class="user-mention">
                        <strong>${item.original.key}</strong>
                        <small>${item.original.value}</small>
                    </span>`;
                },
                selectTemplate: function (item) {
                    return `@${item.original.key}`;
                },
                noMatchTemplate: function () {
                    return '<span style="visibility: hidden;"></span>';
                },
                searchOpts: {
                    pre: '',
                    post: '',
                    skip: true,
                    limit: 10
                }
            });

            // Attach to note textarea and any existing reply textareas
            const noteContent = document.getElementById('noteContent');
            if (noteContent && !noteContent.tribute) {
                tribute.attach(noteContent);
                console.log('Tribute attached to noteContent');
            }

            $('.reply-form textarea').each(function () {
                if (!this.tribute) {
                    tribute.attach(this);
                    console.log('Tribute attached to reply textarea');
                }
            });

            // Attach to new reply textareas as they're created
            $(document).on('focus', '.reply-form textarea', function () {
                if (!this.tribute) {
                    tribute.attach(this);
                    console.log('Tribute attached to new reply textarea');
                }
            });
        } catch (error) {
            console.error('Error initializing mentions:', error);
        }
    }

    // Update document ready handler
    $(document).ready(function () {
        console.log('Document ready, initializing components...');
        initializeNotes();
        initializeMentions();

        // Remove duplicate event handlers
        $('.add-note-button').off('click').on('click', function (e) {
            e.preventDefault();
            handleAddNote();
        });

        $('#noteContent').off('keydown').on('keydown', function (e) {
            if (e.ctrlKey && e.keyCode === 13) {
                e.preventDefault();
                handleAddNote();
            }
        });
    });

})(jQuery);
