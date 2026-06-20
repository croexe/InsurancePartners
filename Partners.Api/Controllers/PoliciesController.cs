using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Partners.Core.Contracts;
using Partners.Core.DTOs.Requests;
using Partners.Core.DTOs.Responses;

namespace Partners.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PoliciesController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // POST api/policies
        [HttpPost]
        [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePolicyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _policyService.CreateAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Created(string.Empty, result.Policy);
        }
    }
}
