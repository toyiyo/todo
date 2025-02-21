using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Notes
{
    public class Note : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public virtual int TenantId { get; set; }

        [Required]
        public string Content { get; protected set; }

        [ForeignKey("JobId")]
        public Job Job { get; protected set; }
        public Guid JobId { get; set; }

        [ForeignKey("ParentNoteId")]
        public Note ParentNote { get; protected set; }
        public Guid? ParentNoteId { get; set; }

        public virtual ICollection<Note> Replies { get; protected set; }
        
        [ForeignKey("CreatorUserId")]
        public User Author { get; protected set; }

        protected Note()
        {
            
        }

        /// <summary>
        /// Creates a new Note instance.
        /// </summary>
        /// <param name="job">The job associated with the note.</param>
        /// <param name="content">The content of the note.</param>
        /// <param name="author">The author of the note.</param>
        /// <param name="parentNote">The parent note, if any.</param>
        /// <returns>A new Note instance.</returns>
        public static Note Create(Job job, string content, User author, Note parentNote = null)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            if (string.IsNullOrEmpty(content)) throw new ArgumentNullException(nameof(content));
            if (author == null) throw new ArgumentNullException(nameof(author));

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Job = job,
                JobId = job.Id,
                Content = content,
                Author = author,
                TenantId = job.TenantId,
                ParentNote = parentNote,
                ParentNoteId = parentNote?.Id,
                Replies = new HashSet<Note>()
            };
            SetLastModified(note, author);

            return note;
        }

        public static Note SetContent(Note note, string content, User user)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            if (string.IsNullOrEmpty(content)) throw new ArgumentNullException(nameof(content));
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // Validate that only the creator can edit the note
            if (note.CreatorUserId != user.Id)
            {
                throw new UnauthorizedAccessException("Only the creator of the note can edit it.");
            }

            note.Content = content;
            SetLastModified(note, user);
            return note;
        }

        public static Note Delete(Note note, User user)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Only the author can delete the note
            if (note.CreatorUserId != user.Id)
            {
                throw new UnauthorizedAccessException("Only the creator of the note can delete it.");
            }

            note.IsDeleted = true;
            note.DeletionTime = Clock.Now;
            note.DeleterUserId = user.Id;

            return note;
        }

        private static void SetLastModified(Note note, User user)
        {
            note.LastModificationTime = Clock.Now;
            note.LastModifierUserId = user.Id;
        }
    }
}
