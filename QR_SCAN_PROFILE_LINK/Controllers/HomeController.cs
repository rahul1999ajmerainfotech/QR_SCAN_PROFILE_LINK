using Microsoft.AspNetCore.Mvc;
using QR_SCAN_PROFILE_LINK.Models;
using QRCoder;

namespace QR_SCAN_PROFILE_LINK.Controllers
{
    public class HomeController : Controller
    {
        private List<QrVerification> qrVerifications;
        private QRCodeGenerator qrGenerator;
        public HomeController(QRCodeGenerator qrGenerator, List<QrVerification> qrVerifications)
        {
            this.qrGenerator = qrGenerator;
            this.qrVerifications = qrVerifications;
        }

        public IActionResult Index()
        {
            // Simulate the generation of unique scan file name.
            var scanFileName = Guid.NewGuid().ToString();

            // Add the unique ScanFileName in QrVerification Table
            // The ProfileId and Status will not be populated now, they will be added when the user scans the QR.
            qrVerifications.Add(new QrVerification() { ScanFileName = scanFileName });

            // Generation of the QR Code with the scanFileName.
            // The Desktop Application will create the QR code using the unique scan file name.

            // Create a QRCodeData Object for the given unique scan filename.
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(scanFileName, QRCodeGenerator.ECCLevel.Q);  // QRCodeGenerator.ECCLevel.Q will decide the Accuracy level of the QR.

            // Create a byte array that will store the QR code image.
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            int pixelSize = 20;
            var qrCodeImage = qrCode.GetGraphic(pixelSize);

            // Convert the byte array to a base64 string
            string base64Image = Convert.ToBase64String(qrCodeImage);

            ViewBag.QRCodeImage = $"data:image/png;base64,{base64Image}";
            ViewBag.ScanFileName = scanFileName;

            return View();
        }

        /// <summary>
        /// This API will map scanId and ProfileId and will Map both.
        /// </summary>
        /// <param name="ScanFileName"></param>
        /// <returns></returns>
        public IActionResult MapScanToProfile([FromQuery] string scanFileName)
        {
            // Find the record with given scan file name.
            var scanToMap = qrVerifications.Find(opts => opts.ScanFileName == scanFileName);

            // Map the scan file name with given Profile and mark the status to true.
            scanToMap!.ProfileId = Guid.NewGuid().ToString();
            scanToMap.Status = true;

            // Until the Desktop/Web App Confirms the scan and profile mapping, I am showing a temporary view.
            // The temporary view will show that the mapping is successful and it is waiting for Desktop/Web App to read the mapping.
            ViewBag.ScanFileName = scanToMap.ScanFileName;
            ViewBag.ProfileId = scanToMap.ProfileId;

            return View();
        }

        /// <summary>
        /// This Function will be called from Web/Desktop App at some interval.
        /// We will check if the give scanFileName is mapped to profile.
        /// </summary>
        /// <param name="scanFileName"></param>
        /// <returns></returns>
        public IActionResult ReadScanMapping([FromQuery] string scanFileName)
        {
            // Find The Record With Given scanFileName in QrVerification Table.
            var mappedScan = qrVerifications.Find(opts => opts.ScanFileName == scanFileName);

            // Check if the ProfileId and Status has been added to the table for given scanFileName.
            // If the ProfileId and Status is found then it means that user(profile) has scanned the QR the scan is mapped to his profile.
            var isScanMappedSuccessfully = false;
            if (mappedScan != null)
            {
                isScanMappedSuccessfully = !string.IsNullOrEmpty(mappedScan!.ProfileId) && mappedScan.Status == true;
            }
            return Ok(isScanMappedSuccessfully);
        }

        public IActionResult ScanMappingSuccess()
        {
            return View();
        }
    }
}
