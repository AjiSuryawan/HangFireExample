using System;
using System.IO;
using System.Text;
using Hangfire;
using HangFireExample;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{

    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    [HttpPost("schedule-job")]
    public IActionResult ScheduleJob([FromBody] JobMessage jobMessage)
    {
        var folderPath = _configuration["HangfireJobFolderConfig:FolderPath"];
        if (string.IsNullOrEmpty(folderPath))
        {
            return BadRequest("Folder path is missing or empty in app settings.");
        }

        // Schedule a job to run periodically using Hangfire
        //var jobId = BackgroundJob.Schedule(() => ProcessJob(jobData, folderPath), TimeSpan.FromMinutes(1));
        //return Ok($"Job scheduled successfully with ID: {jobId}");
        RecurringJob.AddOrUpdate(() => ProcessJob(jobMessage, folderPath), Cron.MinuteInterval(1));

        return Ok("Recurring job scheduled successfully.");
    }

    public void ProcessJob(JobMessage jobMessage, string folderPath)
    {
        // Your background job logic goes here
        // This method will be executed periodically by Hangfire

        // Example: Reading the contents of the specified folder directory
        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath);

            foreach (var filePath in files)
            {
                try
                {
                    // Read the contents of each file
                    string fileContents = System.IO.File.ReadAllText(filePath);
                    Console.WriteLine($"Contents of file '{Path.GetFileName(filePath)}': {fileContents}");
                    // Make a POST request to another API with fileContents as JSON parameter
                    //PostApiRequest(fileContents);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file '{Path.GetFileName(filePath)}': {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine($"The specified folder path '{folderPath}' does not exist.");
        }
    }

    private void PostApiRequest(string fileContents)
    {
        // Make a POST request to another API with fileContents as JSON parameter
        using (var client = new HttpClient())
        {
            var apiUrl = _configuration["HangfireJobFolderConfig:url"];

            // Build the request content with fileContents as the JSON parameter
            var content = new StringContent($"{{ \"fileContents\": {fileContents} }}", Encoding.UTF8, "application/json");

            var response = client.PostAsync(apiUrl, content).Result;

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"POST request successful. Response: {response.Content.ReadAsStringAsync().Result}");
            }
            else
            {
                Console.WriteLine($"POST request failed. Status code: {response.StatusCode}");
            }
        }
    }

}
