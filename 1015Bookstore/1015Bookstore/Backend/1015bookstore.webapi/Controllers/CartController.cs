using _1015bookstore.application.Catalog.Carts;
using _1015bookstore.viewmodel.Catalog.Carts;
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
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly string _logFilePath;

        public CartController(ICartService cartService, IConfiguration config)
        {
            _cartService = cartService;

            // Ensure the log file path is correctly combined
            var logFileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logFileDirectory))
            {
                Directory.CreateDirectory(logFileDirectory);
            }
            _logFilePath = Path.Combine(logFileDirectory, "CartActivityLogs.txt");

        }

        [HttpPost("set")]
        public async Task<IActionResult> Cart_SetProduct([FromBody] CartAddProduct product, [FromQuery][Required] Guid gUser_id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _cartService.Cart_SetProduct(product, gUser_id);

                // Log the action
                WriteLog($"{DateTime.Now}: User {gUser_id} set product to cart. Status: {response.Status}, Message: {response.Message}");

                return StatusCode(response.CodeStatus, response.Message);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while setting product to cart. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{iCart_id}")]
        public async Task<IActionResult> Cart_DeleteProduct([Required] int iCart_id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _cartService.Cart_DeleteProduct(iCart_id);

                // Log the action
                WriteLog($"{DateTime.Now}: Deleted product from cart. Status: {response.Status}, Message: {response.Message}");

                return StatusCode(response.CodeStatus, response.Message);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while deleting product from cart. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get/{gUser_id}")]
        public async Task<IActionResult> Cart_GetCart([Required] Guid gUser_id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _cartService.Cart_GetCart(gUser_id);

                // Log the action
                WriteLog($"{DateTime.Now}: Get cart for user {gUser_id}. Status: {response.Status}, Message: {response.Message}");

                if (!response.Status)
                    return StatusCode(response.CodeStatus, response.Message);

                return StatusCode(response.CodeStatus, response.Data);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while getting cart for user {gUser_id}. Exception: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("updateamount/{iCart_id}")]
        public async Task<IActionResult> Cart_UpdateAmount([Required] int iCart_id, [FromQuery][Required] int amountadd)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _cartService.Cart_UpdateAmount(iCart_id, amountadd);

                // Log the action
                WriteLog($"{DateTime.Now}: Updated product amount in cart (Cart ID: {iCart_id}). Status: {response.Status}, Message: {response.Message}");

                return StatusCode(response.CodeStatus, response.Message);
            }
            catch (Exception ex)
            {
                WriteLog($"{DateTime.Now}: Exception occurred while updating product amount in cart (Cart ID: {iCart_id}). Exception: {ex.Message}");
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
