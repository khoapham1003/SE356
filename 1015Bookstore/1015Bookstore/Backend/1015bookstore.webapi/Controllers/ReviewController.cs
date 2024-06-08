using _1015bookstore.application.Catalog.Reviews;
using _1015bookstore.viewmodel.Catalog.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace _1015bookstore.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly string _logFilePath;

        public ReviewController(IReviewService reviewService, IConfiguration config)
        {
            _reviewService = reviewService;

            // Đảm bảo rằng đường dẫn tới file log được kết hợp đúng cách
            var logFileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logFileDirectory))
            {
                Directory.CreateDirectory(logFileDirectory);
            }
            _logFilePath = Path.Combine(logFileDirectory, "ReviewActivityLogs.txt");

        }

        [HttpPost]
        public async Task<IActionResult> Review_Create([FromBody] ReviewRequestCreate request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _reviewService.Review_Create(request);

                // Ghi log hành động
                WriteLog($"{DateTime.Now}: Order {request.iOrder_id} created review for product {request.lReview_products}. Status: {result.Status}, Message: {result.Message}");

                return StatusCode(result.CodeStatus, result.Message);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while creating review. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getbyid")]
        [AllowAnonymous]
        public async Task<IActionResult> Review_GetByProductID([FromQuery] int iProduct_id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _reviewService.Review_GetByProductId(iProduct_id);

                if (!result.Status)
                {
                    return StatusCode(result.CodeStatus, result.Message);
                }
                return StatusCode(result.CodeStatus, result.Data);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while getting reviews by product ID ({iProduct_id}). Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        private void WriteLog(string logMessage)
        {
            string logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            string combinedContent;
            using (StreamReader reader = new StreamReader(_logFilePath))
            {
                string currentContent = reader.ReadToEnd();
                combinedContent = logMessage + Environment.NewLine + currentContent;
            }
            using (StreamWriter writer = new StreamWriter(_logFilePath, false))
            {

                writer.WriteLine(combinedContent);

            }
        }
    }
}
