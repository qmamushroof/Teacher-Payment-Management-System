using System.ComponentModel.DataAnnotations;

namespace TeacherPaymentManagement.Models
{
    public class TeacherEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        [Required]
        [Display(Name = "Script Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Script quantity must be at least 1")]
        public int ScriptQuantity { get; set; }

        [Required]
        [Display(Name = "Evaluation Date")]
        [DataType(DataType.Date)]
        public DateTime EvaluationDate { get; set; }

        public int? PaymentSettingId { get; set; }
        public PaymentSetting PaymentSetting { get; set; }
    }
}
