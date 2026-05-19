using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class CreateCustomerRequest
{
    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(100, ErrorMessage = "Name must be 100 characters or less.")]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(30)]
    public string City { get; set; } = string.Empty;

    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}

public class CountryRegion
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
