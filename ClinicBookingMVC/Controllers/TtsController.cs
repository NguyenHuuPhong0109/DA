using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBookingMVC.Controllers;

[Authorize(Roles = "BacSi,LeTan,Admin")]
[ApiController]
[Route("api/[controller]")]
public class TtsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TtsController> _logger;

    public TtsController(IHttpClientFactory httpClientFactory, ILogger<TtsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Proxy phát giọng đọc tiếng Việt qua Google TTS (client=tw-ob).
    /// Hỗ trợ fallback cho các thiết bị di động / desktop thiếu gói giọng vi-VN cục bộ.
    /// </summary>
    [HttpGet("/api/tts/speak")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Speak([FromQuery] string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new { message = "Nội dung văn bản không được để trống." });
        }

        var trimmed = text.Trim();
        if (trimmed.Length > 220)
        {
            return BadRequest(new { message = "Văn bản vượt quá giới hạn tối đa 220 ký tự." });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("GoogleTts");
            client.Timeout = TimeSpan.FromSeconds(6);

            var encodedText = Uri.EscapeDataString(trimmed);
            var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl=vi&client=tw-ob&q={encodedText}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TTS upstream returned HTTP {StatusCode}", response.StatusCode);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Dịch vụ phát âm trực tuyến tạm thời không phản hồi." });
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync();
            Response.Headers.CacheControl = "public, max-age=3600";
            return File(audioBytes, "audio/mpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi upstream TTS cho văn bản: {Text}", trimmed);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Lỗi kết nối dịch vụ phát âm thanh: " + ex.Message });
        }
    }
}
