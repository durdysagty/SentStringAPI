using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessengerAPI.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessengerAPI.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public async Task Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            await EmailService.SendEmailAsync("durdysagty@gmail.com", "test", context.Error.StackTrace + context.Error.Message);
            //return Problem(detail: context.Error.StackTrace, title: context.Error.Message);
        }
    }
}