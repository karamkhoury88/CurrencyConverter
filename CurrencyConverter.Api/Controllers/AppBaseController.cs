using CurrencyConverter.Api.Attributes.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [ValidateModel]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    [ApiController]
    public class AppBaseController : ControllerBase
    {
    }
}
