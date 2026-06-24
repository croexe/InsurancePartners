using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;

namespace Partners.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnersController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        // GET api/partners
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PartnerListItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var partners = await _partnerService.GetAllAsync();
            return Ok(partners);
        }

        // GET api/partners/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PartnerDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var partner = await _partnerService.GetByIdAsync(id);

            if (partner is null)
            {
                return NotFound(new { message = $"Partner with Id '{id}' was not found." });
            }

            return Ok(partner);
        }

        // POST api/partners
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _partnerService.CreateAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.PartnerId }, new { id = result.PartnerId });
        }
    }
}
