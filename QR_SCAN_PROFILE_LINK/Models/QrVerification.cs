namespace QR_SCAN_PROFILE_LINK.Models;
public class QrVerification
{
    public required string ScanFileName { get; set; }
    public string? ProfileId { get; set;}
    public bool Status { get; set; } = false;
}

