using ChatMentor.Backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIRepository _iaiRepository;

    public AIController(IAIRepository iaiRepository)
    {
        _iaiRepository = iaiRepository;
    }

    [HttpPost("generate-outline")]
    public async Task<IActionResult> GenerateOutline([FromBody] string courseTitle)
    {
        var content = await _iaiRepository.GenerateCourseOutlineAsync(courseTitle);
        return Ok(new
        {
            status = "Success",
            data = content
        });
    }

    [HttpPost("generate-quiz")]
    public async Task<IActionResult> GenerateQuiz([FromBody] string topic)
    {
        var content = await _iaiRepository.GenerateQuizAsync(topic);
        return Ok(new
        {
            status = "Success",
            data = content
        });
    }
}