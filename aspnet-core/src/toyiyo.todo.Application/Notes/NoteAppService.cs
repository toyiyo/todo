using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Notifications;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Jobs;
using toyiyo.todo.Notes.Dto;
using toyiyo.todo.Notifications.NotificationData;
using Abp;

namespace toyiyo.todo.Notes
{
    /// <summary>
    /// Application service for managing notes.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Jobs)]
    public class NoteAppService : todoAppServiceBase, INoteAppService
    {
        private readonly INoteManager _noteManager;
        private readonly IJobManager _jobManager;
        private readonly INotificationPublisher _notificationPublisher;

        public NoteAppService(
            INoteManager noteManager,
            IJobManager jobManager,
            INotificationPublisher notificationPublisher)
        {
            _noteManager = noteManager;
            _jobManager = jobManager;
            _notificationPublisher = notificationPublisher;
        }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <param name="input">The input containing note details.</param>
        /// <returns>The created note DTO.</returns>
        public async Task<NoteDto> CreateAsync(CreateNoteInput input)
        {
            var job = await _jobManager.Get(input.JobId);
            var author = await GetCurrentUserAsync();
            Note parentNote = null;
            
            if (input.ParentNoteId.HasValue)
            {
                parentNote = await _noteManager.Get(input.ParentNoteId.Value);
            }

            var note = Note.Create(job, input.Content, author, parentNote);
            await _noteManager.Create(note);

            var mentions = ExtractMentions(input.Content);
            if (mentions.Any())
            {
                await NotifyMentionedUsers(mentions, job, note);
            }

            return ObjectMapper.Map<NoteDto>(note);
        }

        /// <summary>
        /// Gets all notes for a specific job with optional filtering and pagination.
        /// </summary>
        /// <param name="input">Input parameters containing JobId, Keyword and paging information.</param>
        /// <returns>Paged list of notes.</returns>
        public async Task<PagedResultDto<NoteDto>> GetAllAsync(GetNotesInput input)
        {
            var managerInput = new GetAllNotesInput
            {
                JobId = input.JobId,
                Keyword = input.Keyword,
                MaxResultCount = input.MaxResultCount,
                SkipCount = input.SkipCount
            };

            var notes = await _noteManager.GetAll(managerInput);
            var totalCount = await _noteManager.GetAllCount(managerInput);

            return new PagedResultDto<NoteDto>(totalCount, ObjectMapper.Map<List<NoteDto>>(notes));
        }

        /// <summary>
        /// Deletes a note by its ID.
        /// </summary>
        /// <param name="id">The ID of the note to delete.</param>
        public async Task DeleteAsync(Guid id)
        {
            await _noteManager.Delete(id, await GetCurrentUserAsync());
        }

        /// <summary>
        /// Updates the content of a note.
        /// </summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="content">The new content of the note.</param>
        /// <returns>The updated note DTO.</returns>
        public async Task<NoteDto> UpdateAsync(Guid id, string content)
        {
            var note = await _noteManager.Get(id);
            var currentUser = await GetCurrentUserAsync();
            var updatedNote = Note.SetContent(note, content, currentUser);
            await _noteManager.Update(updatedNote);
            return ObjectMapper.Map<NoteDto>(updatedNote);
        }

        /// <summary>
        /// Extracts mentioned usernames from the note content.
        /// </summary>
        /// <param name="content">The content of the note.</param>
        /// <returns>List of mentioned usernames.</returns>
        private static List<string> ExtractMentions(string content)
        {
            return content
            .Split(' ')
            .Where(word => word.StartsWith('@'))
            .Select(word => word.Substring(1))
            .ToList();
        }

        /// <summary>
        /// Notifies the mentioned users in a note.
        /// </summary>
        /// <param name="mentions">List of mentioned usernames.</param>
        /// <param name="job">The job associated with the note.</param>
        /// <param name="note">The note containing the mentions.</param>
        private async Task NotifyMentionedUsers(List<string> mentions, Job job, Note note)
        {
            var currentUser = await GetCurrentUserAsync();
            
            var users = await UserManager.Users
                .Where(u => mentions.Contains(u.UserName))
                .ToListAsync();

            // Using Parallel.ForEachAsync (requires .NET 6+)
            await Parallel.ForEachAsync(users, 
                new ParallelOptions { MaxDegreeOfParallelism = 3 }, // Limit concurrent operations
                async (user, token) =>
                {
                    await _notificationPublisher.PublishAsync(
                        "Note.Mention",
                        new NoteMentionNotificationData(
                            currentUser.UserName,
                            job.Title,
                            note.Content,
                            job.Id,
                            note.Id
                        ),
                        userIds: new[] { new UserIdentifier(user.TenantId, user.Id) }
                    );
                });
        }
    }
}
