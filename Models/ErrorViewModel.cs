namespace FoodTruck.Web.Models;

public class ErrorViewModel
{
    // ViewModel for displaying error information.
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
