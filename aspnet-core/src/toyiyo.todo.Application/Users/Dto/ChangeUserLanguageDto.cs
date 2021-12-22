using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}