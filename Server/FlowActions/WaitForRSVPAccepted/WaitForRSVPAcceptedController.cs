using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Module.GoogleAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;

namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    // Receives Google Calendar change notifications (proxied/normalized) and advances RSVP gates
    [Route("api/[controller]")]
    [FlowWebhookEndpoint(typeof(WaitForRSVPAccepted))]
    public class WaitForRSVPAcceptedController : FlowWebhookControllerBase<WaitForRSVPAccepted>
    {
        private readonly ICalendarWatchService _watchService;

        public WaitForRSVPAcceptedController(IWorkflowWebhookGateway gateway, ILogManager logger, ICalendarWatchService watchService)
            : base(gateway, logger)
        {
            _watchService = watchService;
        }

        // Expect a flattened payload with at minimum: email, rsvp (accepted/declined/tentative)
        // Optional: calendarId, eventId, organizer, occurredAt
        protected override async Task<string> OnReceivedAsync(WebhookRequestContext ctx)
        {
            // 1) Google native push: read token header and try to map
            if (Request.Headers.TryGetValue("X-Goog-Channel-Token", out var tokenValues))
            {
                var token = tokenValues.ToString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var watch = await _watchService.GetByTokenAsync(token);
                    if (watch != null)
                    {
                        // If per-user watch in future, set ctx.Email = watch.UserEmail when populated
                        if (!string.IsNullOrWhiteSpace(watch.UserEmail))
                        {
                            ctx.Email = watch.UserEmail;
                        }
                        // Any additional filtering/delta could be done here later
                    }
                }
            }

            // 2) Flat payload (email + rsvp) path
            if (ctx.Flat != null)
            {
                if (ctx.Flat.TryGetValue("email", out var email) && !string.IsNullOrWhiteSpace(email))
                {
                    ctx.Email = email;
                }

                if (ctx.Flat.TryGetValue("rsvp", out var rsvp) && !string.IsNullOrWhiteSpace(rsvp))
                {
                    // Minimal gate rules: only schedule when rsvp indicates acceptance
                    var val = rsvp.Trim().ToLowerInvariant();
                    if (val is "accepted" or "accept" or "yes")
                    {
                        // Accept – schedule by returning null (base will schedule when Email is set)
                        return null;
                    }

                    // Not an acceptance; no schedule
                    return "RSVP not accepted – gate remains waiting.";
                }
            }

            // If we reach here with no definitive acceptance info, do not schedule
            return "Insufficient data to schedule gate (no acceptance in payload).";
        }
    }
}
