using Microsoft.AspNetCore.Mvc;
using QR_SCAN_PROFILE_LINK.Models;
using QRCoder;
using System.Diagnostics;

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

            // Add scanFileName in QR_Verification_Table, the ProfileId and Status not be populated now, they will be added when the user scans the QR.
            qrVerifications.Add(new QrVerification() { ScanFileName = scanFileName });

            // Create the QR Code using the scanFileName.
            // The Desktop Applicatoin will create the QR code using scan id.

            // Create a QRCodeData Object for the given payload
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(scanFileName, QRCodeGenerator.ECCLevel.Q);

            // Create a byte array that will store the QR code image
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            int pixelSize = 20;
            var qrCodeImage = qrCode.GetGraphic(pixelSize);

            // Convert the byte array to a base64 string
            string base64Image = Convert.ToBase64String(qrCodeImage);

            ViewBag.QRCodeImage = $"data:image/png;base64,{base64Image}";
            ViewBag.ScanFileName = scanFileName;    
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// This API will take scanId and ProfileId and will Map both.
        /// </summary>
        /// <param name="ScanFileName"></param>
        /// <returns></returns>
        //[HttpPost("map-scan-to-profile")]
        public IActionResult MapScanToProfile([FromQuery] string scanFileName)
        {
            var scanToMap = qrVerifications.Find(opts => opts.ScanFileName == scanFileName);
            scanToMap!.ProfileId = Guid.NewGuid().ToString();
            scanToMap.Status = true;
            ViewBag.ScanFileName = scanToMap.ScanFileName;
            ViewBag.ProfileId = scanToMap.ProfileId;
            return View();
        }
        //[HttpGet("ReadScanMapping")]
        public IActionResult ReadScanMapping([FromQuery] string scanFileName)
        {
            var mappedScan = qrVerifications.Find(opts => opts.ScanFileName == scanFileName);

            var isScanMappedSuccessfully = !string.IsNullOrEmpty(mappedScan!.ProfileId) && mappedScan.Status == true;
            return Ok(isScanMappedSuccessfully);
        }

        public IActionResult ScanMappingSuccess()
        {
            return View();
        }
    }
}
