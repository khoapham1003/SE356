using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using System;
using System.IO;

namespace _1015bookstore.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerPingController : ControllerBase
    {
        private readonly string logFilePath = "wwwroot/logs/PingLogs.txt";

        [HttpGet]
        public IActionResult PingServer()
        {
            string serverIpAddress = "localhost";
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(serverIpAddress);
                    if (reply.Status == IPStatus.Success)
                    {
                        string logMessage = $"[{DateTime.Now}] - Ping successful. Response time: {reply.RoundtripTime}ms";
                        WriteLog(logMessage);

                        return Ok(new { Message = "Server is alive. Response time: " + reply.RoundtripTime + "ms" });
                    }
                    else
                    {
                        string logMessage = $"[{DateTime.Now}] - Server unreachable.";
                        WriteLog(logMessage);

                        return StatusCode(500, new { Message = "Server is unreachable." });
                    }
                }
            }
            catch (PingException ex)
            {
                string logMessage = $"[{DateTime.Now}] - Error occurred: {ex.Message}";
                WriteLog(logMessage);

                return StatusCode(500, new { Message = "Error occurred: " + ex.Message });
            }
        }

        private void WriteLog(string logMessage)
        {
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            string combinedContent;
            using (StreamReader reader = new StreamReader(logFilePath))
            {
                string currentContent = reader.ReadToEnd();
                combinedContent = logMessage + Environment.NewLine + currentContent;

            }
            using (StreamWriter writer = new StreamWriter(logFilePath, false))
            {
                writer.WriteLine(combinedContent);
            }
        }
    }
}