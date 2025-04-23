using System.ComponentModel.DataAnnotations;

namespace TeacherPaymentManagement.Models
{
    public class PaymentSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Setting Name")]
        public string SettingName { get; set; }

        [Required]
        [Display(Name = "Effective Date")]
        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; }

        [Required]
        [Display(Name = "Closing Date")]
        [DataType(DataType.Date)]
        public DateTime ClosingDate { get; set; }

        [Required]
        [Display(Name = "Honorium Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Honorium amount must be positive")]
        public decimal HonoriumAmount { get; set; }

        [Required]
        [Display(Name = "Additional Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Additional amount must be positive")]
        public decimal AdditionalAmount { get; set; }

        [Display(Name = "Is Used")]
        public bool IsUsed { get; set; } = false;
    }
}
