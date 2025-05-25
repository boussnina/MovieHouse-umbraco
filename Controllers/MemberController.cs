using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using MovieHouse.Models;

namespace MovieHouse.Controllers
{
    [ApiController]
    [Route("api/member")]
    public class MemberApiController : ControllerBase
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;

        public MemberApiController(IMemberManager memberManager, IMemberService memberService)
        {
            _memberManager = memberManager;
            _memberService = memberService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input");

            if (_memberService.GetByEmail(model.Email) != null)
                return BadRequest("A member with that email already exists.");

            var identityUser = MemberIdentityUser.CreateNew(
                username: model.Email,
                email: model.Email,
                memberTypeAlias: "movieMember",
                isApproved: true,
                name: model.Email
            );

            var createResult = await _memberManager.CreateAsync(identityUser, model.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return BadRequest("Failed to register member: " + errors);
            }

            var user = await _memberManager.FindByEmailAsync(model.Email);
            if (user == null)
                return StatusCode(500, "User created but could not be loaded.");

            var member = _memberService.GetByKey(user.Key);
            if (member == null)
                return StatusCode(500, "Member could not be found.");

            member.SetValue("contactmail", model.Email);
            _memberService.Save(member);

            return Ok("Member registered!");
        }
    }
}
