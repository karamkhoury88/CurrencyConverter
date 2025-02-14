using CurrencyConverter.Api.Attributes.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [ValidateModel]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AppBaseController : ControllerBase
    {
    }
}
