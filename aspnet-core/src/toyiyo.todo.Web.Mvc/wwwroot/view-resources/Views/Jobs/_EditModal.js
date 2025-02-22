(function ($) {

    const Editor = toastui.Editor;
    const _$getLinkButton = $('.btn-pane-get-link');
    let _debounceTimer = null;
    const _$subtaskForm = $('#subtasks #AddByTitle');

    document.getElementById("dueDate").setAttribute("min", new Date().toJSON().split('T')[0]);

    const editor = new Editor({
        el: document.querySelector('#Description'),
        initialValue: $('#descriptionFromServer').val(),
        height: '50em',
        initialEditType: 'wysiwyg',
        previewStyle: 'vertical'
    });

    let _jobService = abp.services.app.job,
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
        let job = _$form.serializeFormToObject();
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
            let job = _$form.serializeFormToObject();
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
        let $text = $(this);
        let $input = $text.next('.subtask-input');
        $text.addClass('d-none');
        $input.removeClass('d-none');
        $input.focus();
    });

    $('table').on('blur', '.subtask-input', function () {
        let $input = $(this);
        let $text = $input.prev('.subtask-text');
        $input.addClass('d-none');
        $text.removeClass('d-none');
        $text.text($input.val());
    });
    //add subtask to UI when subtask is created
    function addSubtaskToTable(subtask) {
        let $row = $(`
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

        // Remove the cancel-reply event handler
        $(document).on('click', '.reply-button', handleReplyButtonClick);
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
            
            // Sort notes by creation time if needed
            result.items.sort((a, b) => new Date(a.creationTime) - new Date(b.creationTime));
            
            result.items.forEach(note => {
                // Only process parent notes
                if (!note.parentNoteId) {
                    const $noteElement = createNoteElement(note);
                    $notesList.append($noteElement);
                }
            });
        }).always(function () {
            abp.ui.clearBusy($notesContainer);
        });
    }

    // Update createNoteElement to handle nested replies
    function createNoteElement(note) {
        let $note = $('<div/>', {
            class: 'note mb-3',
            'data-note-id': note.id
        });

        let $header = $('<div/>', {
            class: 'note-header d-flex justify-content-between align-items-center'
        });

        let $authorInfo = $('<div/>');
        $('<strong/>').text(note.authorName).appendTo($authorInfo);
        $('<small/>', {
            class: 'text-muted ml-2'
        }).text(moment(note.creationTime).fromNow()).appendTo($authorInfo);

        let $actions = $('<div/>', {
            class: 'note-actions'
        }).html(getActionsHtml(note));

        let $content = $('<div/>', {
            class: 'note-content'
        }).html(formatNoteContent(note.content));

        $header.append($authorInfo, $actions);
        $note.append($header, $content);

        // Only show thread section for parent notes (notes without a parentNoteId)
        if (!note.parentNoteId) {
            // Create thread container
            let $threadContainer = $('<div/>', {
                class: 'thread-container mt-2',
                // Only hide if there are no replies
                style: note.replies && note.replies.length > 0 ? '' : 'display: none;'
            });

            let $repliesSection = $('<div/>', {
                class: 'replies-section pl-4 border-left'
            });

            // If note has replies, add them to the thread
            if (note.replies && note.replies.length > 0) {
                note.replies.forEach(reply => {
                    $repliesSection.append(createReplyElement(reply));
                });
            }

            // Create persistent reply form at the bottom of thread
            const $replyForm = createReplyForm();
            $replyForm.addClass('mt-3'); // Add some spacing above the form

            $threadContainer.append($repliesSection, $replyForm);
            $note.append($threadContainer);
        }

        return $note;
    }

    function createReplyElement(reply) {
        let $reply = $('<div/>', {
            class: 'reply mb-2',
            'data-note-id': reply.id
        });

        let $header = $('<div/>', {
            class: 'reply-header d-flex justify-content-between align-items-center'
        });

        let $authorInfo = $('<div/>');
        $('<strong/>').text(reply.authorName).appendTo($authorInfo);
        $('<small/>', {
            class: 'text-muted ml-2'
        }).text(moment(reply.creationTime).fromNow()).appendTo($authorInfo);

        let $actions = $('<div/>', {
            class: 'reply-actions'
        }).html(getActionsHtml(reply));

        let $content = $('<div/>', {
            class: 'reply-content'
        }).html(formatNoteContent(reply.content));

        $header.append($authorInfo, $actions);
        $reply.append($header, $content);

        return $reply;
    }

    function handleReplyButtonClick(e) {
        e.preventDefault();
        e.stopPropagation();

        const $button = $(this);
        const $note = $button.closest('.note');
        const $threadContainer = $note.find('.thread-container');
        const $icon = $button.find('i');
        
        // Toggle thread container visibility
        $threadContainer.toggle();
        
        // Toggle chevron direction
        $icon.toggleClass('fa-chevron-down fa-chevron-up');
        
        // Focus on the reply textarea when showing thread
        if ($threadContainer.is(':visible')) {
            $threadContainer.find('.reply-form textarea').focus();
        }

        return false;
    }

    // New helper function to create consistent reply forms
    function createReplyForm() {
        const placeholder = escapeHtml(l('WriteReplyHere'));
        const replyText = escapeHtml(l('Reply'));
        
        return $(`
            <div class="reply-form">
                <textarea class="form-control" rows="2" placeholder="${placeholder}"></textarea>
                <div class="mt-2">
                    <button type="button" class="btn btn-primary btn-sm submit-reply">${replyText}</button>
                </div>
            </div>
        `);
    }

    // Update submit reply handler to keep the form
    $(document).on('click', '.submit-reply', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $replyForm = $(this).closest('.reply-form');
        const $note = $replyForm.closest('.note');
        const $textarea = $replyForm.find('textarea');
        const content = $textarea.val();

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
                const $replyElement = createReplyElement(result);
                const $repliesSection = $note.find('.replies-section');
                
                // Add the new reply before the reply form
                $repliesSection.append($replyElement);
                
                // Clear the textarea but keep the form
                $textarea.val('');
                $textarea.focus();
                
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
        let $notesList = $('#notesList');
        $notesList.empty();

        _noteService.getAll({
            jobId: $('#JobId').val(),
            maxResultCount: 10,
            skipCount: 0
        }).done(function (result) {
            result.items.forEach(function (note) {
                let $noteElement = createNoteElement(note);
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

            // Configure marked with security options
            marked.setOptions({
                breaks: true,
                gfm: true,
                headerIds: false, // Disable header IDs to prevent XSS
                mangle: false,    // Disable mangle to prevent XSS
                sanitize: true,   // Enable sanitization
                silent: true      // Don't throw errors on invalid markup
            });

            // Sanitize input before passing to marked
            const sanitizedContent = DOMPurify.sanitize(content);
            const markedContent = marked.parse(sanitizedContent);
            
            // Sanitize output after markdown processing
            return DOMPurify.sanitize(markedContent, {
                ALLOWED_TAGS: ['p', 'br', 'strong', 'em', 'ul', 'ol', 'li', 'code', 'pre'],
                ALLOWED_ATTR: [] // No attributes allowed
            });
        } catch (error) {
            console.error('Error formatting note content:', error);
            return $('<div/>').text(content).html(); // Fallback to plain text
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function getActionsHtml(note) {
        let currentUserId = abp.session.userId;
        let isAuthor = note.authorId === currentUserId;
        let actions = [];

        // Only show reply button for parent notes (notes without a parentNoteId)
        if (!note.parentNoteId) {
            const replyCount = note.replies ? note.replies.length : 0;
            const replyText = replyCount === 0 
                ? escapeHtml(l('Reply'))
                : `${replyCount} ${escapeHtml(replyCount === 1 ? l('Reply') : l('Replies'))}`;
            
            actions.push(`<button class="btn btn-sm btn-link reply-button">
                ${replyText} <i class="fas fa-chevron-${note.replies && note.replies.length > 0 ? 'up' : 'down'}"></i>
            </button>`);
        }

        if (isAuthor) {
            actions.push(`<button class="btn btn-sm btn-link edit-button">${escapeHtml(l('Edit'))}</button>`);
            actions.push(`<button class="btn btn-sm btn-link delete-button">${escapeHtml(l('Delete'))}</button>`);
        }

        return actions.join('');
    }
    
    // Use in your event handler
    function handleDeleteButtonClick(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const $noteElement = $(this).closest('[data-note-id]');
        const noteId = $noteElement.data('note-id');
        
        confirmAndDeleteNote(noteId, $noteElement);
    }

    function handleNoteDeletion(noteId, $noteElement) {
        const _noteService = abp.services.app.note;
        abp.ui.setBusy($noteElement);
    
        _noteService.delete(noteId)
            .done(() => onDeleteSuccess($noteElement))
            .fail(() => abp.notify.error(l('ErrorDeletingNote')))
            .always(() => abp.ui.clearBusy($noteElement));
    }
    
    function onDeleteSuccess($noteElement) {
        $noteElement.fadeOut(() => {
            $noteElement.remove();
            handleThreadVisibility($noteElement);
        });
        abp.notify.success(l('NoteDeleted'));
    }
    
    function handleThreadVisibility($noteElement) {
        if (!$noteElement.hasClass('reply')) {
            return;
        }
    
        const $threadContainer = $noteElement.closest('.thread-container');
        const $remainingReplies = $threadContainer.find('.reply');
        
        if ($remainingReplies.length === 0) {
            $threadContainer.hide();
        }
    }
    
    function confirmAndDeleteNote(noteId, $noteElement) {
        const confirmOptions = {
            message: l('DeleteNoteConfirmationMessage'),
            title: l('AreYouSure'),
            callback: (isConfirmed) => {
                if (isConfirmed) {
                    handleNoteDeletion(noteId, $noteElement);
                }
            }
        };
    
        abp.message.confirm(confirmOptions.message, confirmOptions.title, confirmOptions.callback);
    }

    function handleEditButtonClick(e) {
        e.preventDefault();
        e.stopPropagation();

        // Find the closest element with note-id that's either a .note or .reply
        const $noteElement = $(this).closest('[data-note-id]');
        // Find the specific content element within the current note/reply scope
        const $content = $noteElement.children('.note-content, .reply-content');
        
        // If already editing, return
        if ($content.find('.edit-form').length > 0) {
            return;
        }

        const noteId = $noteElement.data('note-id');
        // Get only this specific content's text, avoiding nested content
        const initialContent = $content.clone()    // Clone to avoid modifying original
            .children('.thread-container').remove()  // Remove thread container from clone
            .end()                                  // Go back to original element
            .text().trim();                         // Get text content

        // Create an inline edit form that replaces the content
        const $editForm = $(`
            <div class="edit-form">
                <textarea class="form-control" rows="2">${initialContent}</textarea>
                <div class="mt-2">
                    <button type="button" class="btn btn-primary btn-sm save-edit">Save</button>
                    <button type="button" class="btn btn-secondary btn-sm cancel-edit">Cancel</button>
                </div>
            </div>
        `);

        // Store original content
        $content.data('original-content', initialContent);
        // Hide the content and show edit form
        $content.hide().after($editForm);

        // Handle cancel
        $editForm.on('click', '.cancel-edit', function() {
            cleanup();
        });

        // Handle save
        $editForm.on('click', '.save-edit', function() {
            const updatedContent = $editForm.find('textarea').val();
            if (!updatedContent) {
                abp.notify.warn(l('PleaseEnterNote'));
                return;
            }

            const _noteService = abp.services.app.note;
            abp.ui.setBusy($editForm);

            _noteService.update(noteId, updatedContent)
                .done(function() {
                    $content.html(formatNoteContent(updatedContent)).show();
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
            $content.show();
            $editForm.remove();
        }

        // Focus on the textarea
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
                let $noteElement = createNoteElement(result);
                // Change from prepend to append for consistency
                $('#notesList').append($noteElement);
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
