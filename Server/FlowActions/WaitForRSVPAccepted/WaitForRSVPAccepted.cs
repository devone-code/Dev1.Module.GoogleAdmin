using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dev1.Flow.Core;
using Dev1.Flow.Core.DTOs;
using Dev1.Flow.Core.Helpers;
using Dev1.Flow.Core.Models;
using Dev1.Module.GoogleAdmin.Services;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Dev1.Module.GoogleAdmin.GoogleAction
{
    public class WaitForRSVPAccepted : IFlowProcessor
    {
        public string ActionName => "Wait For RSVP Accepted";
        public string ActionDescription => "Gate that advances when RSVP accepted webhook is received.";
        public List<string> ContextRequirements => new();

        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly ICalendarWatchService _calendarWatchService;
        private readonly IUserRepository _userRepository;
        private readonly Oqtane.Infrastructure.ILogManager _logger;

        public WaitForRSVPAccepted(
            IGoogleCalendarService googleCalendarService,
            ICalendarWatchService calendarWatchService,
            IUserRepository userRepository,
            Oqtane.Infrastructure.ILogManager logger)
        {
            _googleCalendarService = googleCalendarService;
            _calendarWatchService = calendarWatchService;
            _userRepository = userRepository;
            _logger = logger;
        }

        public List<ActionPropertyDefinition> PropertyDefinitions => new()
        {
            new ActionPropertyDefinition
            {
                Name = "Watch Calendar",
                InputTypeId = Convert.ToInt16(eInputType.List),
                ForceWorkflow = false,
                IsForWorkflow = false,
                IsRequired = false
            }
        };

        // Gate passes when scheduled by webhook path; if executed directly, just pass (no-op).
        public Task<ExecuteActionContext> ExecuteActionAsync(ExecuteActionContext ctx)
        {
            try
            {
                ctx.WorkflowItem.Status = (int)eActionStatus.Pass;
                ctx.WorkflowItem.LastResponse = "Awaited RSVP acceptance already satisfied or scheduled by webhook.";
            }
            catch (Exception ex)
            {
                ctx.WorkflowItem.Status = (int)eActionStatus.Fail;
                ctx.WorkflowItem.LastResponse = ex.Message;
            }
            return Task.FromResult(ctx);
        }

        public async Task<ActionDataResponse> GetActionDataAsync(string propertyName, int moduleid, int userid, int siteId)
        {
            try
            {
                switch (propertyName)
                {
                    case "Watch Calendar":
                    {
                        var user = _userRepository.GetUser(userid);
                        var calendars = await _googleCalendarService.GetAvailableGoogleCalendarsAsync(moduleid, Dev1.Module.GoogleAdmin.Shared.Models.CalendarAuthMode.UserCalendar, user?.Email);
                        if (calendars?.Items != null)
                        {
                            return new ActionDataResponse
                            {
                                Success = true,
                                Items = calendars.Items.Select(c => new ActionDataItem
                                {
                                    Value = c.Id,
                                    Text = c.Summary
                                }).ToList()
                            };
                        }
                        break;
                    }
                }

                return new ActionDataResponse { Success = false, ErrorMessage = $"Property '{propertyName}' not found or not supported for data retrieval" };
            }
            catch (Exception ex)
            {
                return new ActionDataResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // Design-time lifecycle hook: keep Google watch subscriptions in sync with action properties
        public async Task OnActionAddedOrUpdatedAsync(ActionRegistrationContext ctx, CancellationToken ct = default)
        {
            try
            {
                // Read current and previous calendar selection
                string calendarId = null;
                string prevCalendarId = null;
                if (ctx.Properties != null)
                {
                    ctx.Properties.TryGetValue("Watch Calendar", out calendarId);
                    ctx.Properties.TryGetValue("__prev:Watch Calendar", out prevCalendarId);
                }

                // For now, correlation key (email) is managed at webhook-time, watches are calendar-level.
                var userEmail = ""; // reserved for future per-user watches

                // Webhook URL (to be provided on ctx in future); fallback to empty
                var webhookUrl = ctx.WebhookUrl ?? string.Empty;

                if (ctx.IsEnabled && !string.IsNullOrWhiteSpace(calendarId))
                {
                    // Ensure watch for current calendar
                    try
                    {
                        await _calendarWatchService.EnsureWatchAsync(ctx.SiteId, calendarId, userEmail, webhookUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, this, Oqtane.Enums.LogFunction.Other,
                            "EnsureWatchAsync failed for SiteId {SiteId} Calendar {CalendarId}: {Error}", ctx.SiteId, calendarId, ex.Message);
                    }
                }

                // If disabled or calendar changed, decrement previous
                var prevDifferent = !string.IsNullOrWhiteSpace(prevCalendarId) && !string.Equals(prevCalendarId, calendarId, StringComparison.OrdinalIgnoreCase);
                if (!ctx.IsEnabled || prevDifferent)
                {
                    if (!string.IsNullOrWhiteSpace(prevCalendarId))
                    {
                        try
                        {
                            await _calendarWatchService.DecrementAsync(ctx.SiteId, prevCalendarId, userEmail);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Warning, this, Oqtane.Enums.LogFunction.Other,
                                "DecrementAsync failed for SiteId {SiteId} Calendar {CalendarId}: {Error}", ctx.SiteId, prevCalendarId, ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, Oqtane.Enums.LogFunction.Other,
                    "OnActionAddedOrUpdatedAsync failed for FlowId {FlowId} FlowItemId {FlowItemId}: {Error}", ctx.FlowId, ctx.FlowItemId, ex.Message);
            }
        }
    }
}
