using System.ComponentModel.DataAnnotations;

namespace TeacherPaymentManagement.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Teacher Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Contact number must be 11 digits")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Contact number must contain only digits")]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "Teacher Type")]
        public TeacherType TeacherType { get; set; }
    }

    public enum TeacherType
    {
        Regular,
        Expert
    }
}
