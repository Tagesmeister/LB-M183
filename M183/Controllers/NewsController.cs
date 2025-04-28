using M183.Controllers.Dto;
using M183.Data;
using M183.Logging;
using M183.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace M183.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        private readonly NewsAppContext _context;
        private readonly ILogger<NewsController> _logger;


        public NewsController(NewsAppContext context, ILogger<NewsController> logger)
        {
            _context = context;
            _logger = logger;

        }

        private News SetTimezone(News news)
        {
            news.PostedDate = TimeZoneInfo.ConvertTimeFromUtc(news.PostedDate, tzi);
            return news;
        }

        /// <summary>
        /// Retrieve all news entries ordered by PostedDate descending
        /// </summary>
        /// <response code="200">All news entries</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<List<News>> GetAll()
        {
            var logEntry = new LoggingModel
            {
                Username = User.Identity?.Name ?? "Unknown",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Retrieve All News",
                Detail = "User attempted to retrieve all news entries."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation($"User attempted to retrieve all news entries {User.Identity?.Name ?? "Unknown"}, IP: {HttpContext.Connection.RemoteIpAddress?.ToString()}.");



            var newsList = _context.News
                .Include(n => n.Author)
                .OrderByDescending(n => n.PostedDate)
                .ToList()
                .Select(SetTimezone);

            logEntry.Status = "Success";
            logEntry.Detail = $"Retrieved {newsList.Count()} news entries.";
            LoggingSystem.Log(logEntry);
            _logger.LogInformation($"User got all news entries {User.Identity?.Name ?? "Unknown"}, IP: {HttpContext.Connection.RemoteIpAddress?.ToString()}.");


            return Ok(newsList);
        }

        /// <summary>
        /// Retrieve a specific news entry by id
        /// </summary>
        /// <param name="id" example="123">The news id</param>
        /// <response code="200">News retrieved</response>
        /// <response code="404">News not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<News> GetById(int id)
        {
            var logEntry = new LoggingModel
            {
                Username = User.Identity?.Name ?? "Unknown",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Retrieve News By ID",
                Detail = $"User attempted to retrieve news with ID: {id}."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("Retrieve news attempt for ID {NewsId}", id);


            var news = _context.News
                .Include(n => n.Author)
                .FirstOrDefault(n => n.Id == id);

            if (news == null)
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = $"News with ID: {id} not found.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("Retrieve news failed: News with ID {NewsId} not found.", id);

                return NotFound();
            }

            logEntry.Status = "Success";
            logEntry.Detail = $"News with ID: {id} retrieved successfully.";
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("News with ID {NewsId} retrieved successfully.", id);


            return Ok(SetTimezone(news));
        }

        /// <summary>
        /// Create a news entry
        /// </summary>
        /// <response code="201">News successfully created</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public ActionResult Create(NewsDto request)
        {
            var logEntry = new LoggingModel
            {
                Username = User.Identity?.Name ?? "Unknown",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Create News",
                Input = $"Header: {request.Header}, Detail: {request.Detail}, AuthorId: {request.AuthorId}, IsAdminNews: {request.IsAdminNews}",
                Detail = "User attempted to create a news entry."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("News creation attempt by user {AuthorId}", request.AuthorId);


            if (request == null)
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = "Request was null.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("News creation failed: Request was null.");

                return BadRequest();
            }

            var newNews = new News();

            newNews.Header = request.Header;
            newNews.Detail = request.Detail;
            newNews.AuthorId = request.AuthorId;
            newNews.PostedDate = DateTime.UtcNow;
            newNews.IsAdminNews = request.IsAdminNews;

            _context.News.Add(newNews);
            _context.SaveChanges();

            logEntry.Status = "Success";
            logEntry.Detail = $"News entry created successfully with ID: {newNews.Id}.";
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("News created successfully with ID {NewsId}.", newNews.Id);


            return CreatedAtAction(nameof(GetById), new { id = newNews.Id}, newNews);
        }

        /// <summary>
        /// Update a specific news by id
        /// </summary>
        /// <param name="id" example="123">The news id</param>
        /// <response code="200">News retrieved</response>
        /// <response code="404">News not found</response>
        [HttpPatch("{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult Update(int id, NewsDto request)
        {
            var logEntry = new LoggingModel
            {
                Username = User.Identity?.Name ?? "Unknown",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Update News",
                Input = $"Header: {request.Header}, Detail: {request.Detail}, AuthorId: {request.AuthorId}, IsAdminNews: {request.IsAdminNews}",
                Detail = $"User attempted to update news with ID: {id}."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation($"User attempted to update news with ID: {id}.");


            if (request == null)
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = "Request was null.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("News update failed: Request was null.");
                return BadRequest();
            }

            var news = _context.News.Find(id);
            if (news == null)
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = $"News with ID: {id} not found.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("News update failed: News with ID {NewsId} not found.", id);

                return NotFound($"News {id} not found");
            }

            news.Header = request.Header;
            news.Detail = request.Detail;
            news.AuthorId = request.AuthorId;
            news.IsAdminNews = request.IsAdminNews;

            _context.News.Update(news);
            _context.SaveChanges();

            logEntry.Status = "Success";
            logEntry.Detail = $"News with ID: {id} updated successfully.";
            LoggingSystem.Log(logEntry);
            _logger.LogInformation($"News with ID {id} updated successfully.");

            return Ok();
        }

        /// <summary>
        /// Delete a specific news by id
        /// </summary>
        /// <param name="id" example="123">The news id</param>
        /// <response code="200">News deleted</response>
        /// <response code="404">News not found</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult Delete(int id)
        {
            var logEntry = new LoggingModel
            {
                Username = User.Identity?.Name ?? "Unknown",
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Delete News",
                Detail = $"User attempted to delete news with ID: {id}."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation($"User attempted to delete news with ID: {id}.");

            var news = _context.News.Find(id);
            if (news == null)
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = $"News with ID: {id} not found.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("News deletion failed: News with ID {NewsId} not found.", id);
                return NotFound($"News {id} not found");
            }

            _context.News.Remove(news);
            _context.SaveChanges();

            logEntry.Status = "Success";
            logEntry.Detail = $"News with ID: {id} deleted successfully.";
            LoggingSystem.Log(logEntry);

            return Ok();
        }
    }
}
