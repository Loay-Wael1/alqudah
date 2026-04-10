using System.ComponentModel.DataAnnotations;

namespace JudgesTournament.Web.ViewModels;

public class RegisterTeamViewModel
{
    [Required(ErrorMessage = "اسم الفريق مطلوب")]
    [StringLength(100, ErrorMessage = "اسم الفريق لا يتجاوز 100 حرف")]
    [Display(Name = "اسم الفريق")]
    public string TeamName { get; set; } = string.Empty;

    [Required(ErrorMessage = "المحافظة مطلوبة")]
    [Display(Name = "المحافظة")]
    public string Governorate { get; set; } = string.Empty;

    [Required(ErrorMessage = "عدد اللاعبين مطلوب")]
    [Range(7, 25, ErrorMessage = "عدد اللاعبين يجب أن يكون بين 7 و 25")]
    [Display(Name = "عدد اللاعبين")]
    public int PlayersCount { get; set; }

    [Required(ErrorMessage = "لون الزي مطلوب")]
    [StringLength(50, ErrorMessage = "لون الزي لا يتجاوز 50 حرف")]
    [Display(Name = "لون الزي")]
    public string UniformColor { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم مسؤول الفريق مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
    [Display(Name = "اسم مسؤول الفريق")]
    public string ContactPersonName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "رقم الهاتف غير صحيح. يجب أن يكون رقم مصري مكون من 11 رقم")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم واتساب مطلوب")]
    [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "رقم واتساب غير صحيح")]
    [Display(Name = "رقم واتساب")]
    public string WhatsAppNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم التحويل مطلوب")]
    [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "رقم التحويل غير صحيح")]
    [Display(Name = "الرقم الذي تم التحويل منه")]
    public string TransferFromNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم صاحب التحويل مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
    [Display(Name = "اسم صاحب التحويل")]
    public string TransferName { get; set; } = string.Empty;

    [Required(ErrorMessage = "مبلغ التحويل مطلوب")]
    [Range(1, 10000, ErrorMessage = "مبلغ التحويل غير صحيح")]
    [Display(Name = "مبلغ التحويل")]
    public decimal TransferAmount { get; set; }

    [Required(ErrorMessage = "تاريخ التحويل مطلوب")]
    [Display(Name = "تاريخ التحويل")]
    public DateTime? TransferDate { get; set; }

    [Required(ErrorMessage = "صورة إيصال التحويل مطلوبة")]
    [Display(Name = "صورة إيصال التحويل")]
    public IFormFile? ReceiptImage { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "يجب الموافقة على الشروط والأحكام")]
    [Display(Name = "أوافق على الشروط والأحكام")]
    public bool AgreedToTerms { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "يجب تأكيد أن التسجيل مبدئي")]
    [Display(Name = "أؤكد أن هذا تسجيل مبدئي فقط")]
    public bool ConfirmedPreliminary { get; set; }

    // For view state
    public bool IsRegistrationOpen { get; set; } = true;
}
