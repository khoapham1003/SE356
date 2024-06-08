using _1015bookstore.application.Catalog.Orders;
using _1015bookstore.viewmodel.Catalog.Orders;
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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly string _logFilePath;

        public OrderController(IOrderService orderService, IConfiguration config)
        {
            _orderService = orderService;

            // Ensure the log file path is correctly combined
            var logFileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logFileDirectory))
            {
                Directory.CreateDirectory(logFileDirectory);
            }
            _logFilePath = Path.Combine(logFileDirectory, "OrderActivityLogs.txt");

        }

        [HttpPut("buy")]
        public async Task<IActionResult> Order_Buy([FromBody] OrderBuyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _orderService.Order_Buy(request);

                // Log the action
                WriteLog($"{DateTime.Now}: User {request.sOrder_name_receiver} bought order. Status: {response.Status}, Message: {response.Message}");

                return StatusCode(response.CodeStatus, response.Message);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while buying order. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        public async Task<IActionResult> Order_Create([FromBody] OrderCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _orderService.Order_Create(request);

                // Log the action
                WriteLog($"{DateTime.Now}: User {request.gUser_id} created order. Status: {response.Status}, Message: {response.Message}");

                if (!response.Status)
                    return StatusCode(response.CodeStatus, response.Message);

                return StatusCode(response.CodeStatus, response.Data);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while creating order. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{iOrder_id}")]
        public async Task<IActionResult> Order_GetById([Required] int iOrder_id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _orderService.Order_GetById(iOrder_id);

                // Log the action
                WriteLog($"{DateTime.Now}: Get order by ID ({iOrder_id}). Status: {response.Status}, Message: {response.Message}");

                if (!response.Status)
                    return StatusCode(response.CodeStatus, response.Message);

                return StatusCode(response.CodeStatus, response.Data);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while getting order by ID ({iOrder_id}). Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> Order_HistoryOfUser([Required] Guid gUser_id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _orderService.Order_HistoryOfUser(gUser_id);

                // Log the action
                WriteLog($"{DateTime.Now}: Get order history of user {gUser_id}. Status: {response.Status}, Message: {response.Message}");

                if (!response.Status)
                    return StatusCode(response.CodeStatus, response.Message);

                return StatusCode(response.CodeStatus, response.Data);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while getting order history of user {gUser_id}. Exception: {ex.Message}");
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
