namespace SatPortUDAPIServiceNowSyncAgent.Request
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.Ticketing;
	using Skyline.DataMiner.SDM.Ticketing.Models;

	public abstract class RequestHandler
	{
		private readonly TicketingApiHelper helper;
		protected readonly string request;

		protected RequestHandler(TicketingApiHelper helper, string request)
		{
			this.helper = helper;
			this.request = request;
		}

		public abstract string IncidentId { get; protected set; }

		public abstract RequestMethod Method { get; }

		protected string Request => request;

		protected TicketingApiHelper Helper => helper;

		public static RequestHandler InitializeHandlerByMethod(Engine engine, string request, RequestMethod method)
		{
			var ticketingHelper = new TicketingApiHelper(engine.GetUserConnection());
			switch (method)
			{
				case RequestMethod.Post:
					return new UpdateTicketRequestHandler(ticketingHelper, request);

				case RequestMethod.Unspecified:
				case RequestMethod.Get:
				case RequestMethod.Put:
				case RequestMethod.Delete:
					throw new NotSupportedException("The REST method is not supported");

				default:
					// not possible, for the compiler
					return null;
			}
		}

		public virtual bool Validate(out string reason, out StatusCode statusCode)
		{
			if (string.IsNullOrWhiteSpace(request))
			{
				reason = "The request body is empty.";
				statusCode = StatusCode.BadRequest;
				return false;
			}

			// check if the ticket exists
			var filter = TicketExposers.ID.Equal(IncidentId);
			var filteredResult = Helper.Tickets.Read(filter).ToList();

			if (!filteredResult.Any())
			{
				reason = $"Ticket related to incident ID {IncidentId} is not found";
				statusCode = StatusCode.NotFound;
				return false;
			}

			reason = string.Empty;
			statusCode = StatusCode.Ok;
			return true;
		}

		public abstract ApiTriggerOutput HandleRequest();
	}
}