namespace _301153142_301137955_Soto_Ko_Lab3.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorMessage { get; internal set; }
    }
}