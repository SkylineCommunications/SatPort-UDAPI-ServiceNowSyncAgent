namespace Skyline.Automation.SatPort.Request
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.SDM.Ticketing;

	public abstract class RequestHandler
	{
		protected readonly ApiTriggerInput requestData;
		private readonly TicketingApiHelper helper;

		protected RequestHandler(TicketingApiHelper helper, ApiTriggerInput requestData)
		{
			this.helper = helper;
			this.requestData = requestData;
		}

		public abstract string IncidentId { get; protected set; }

		public abstract RequestMethod Method { get; }

		protected ApiTriggerInput RequestData => requestData;

		protected TicketingApiHelper Helper => helper;

		public static RequestHandler InitializeHandlerByMethod(Engine engine, ApiTriggerInput requestData)
		{
			var ticketingHelper = new TicketingApiHelper(engine.GetUserConnection());
			switch (requestData.RequestMethod)
			{
				case RequestMethod.Post:
					return new UpdateTicketRequestHandler(ticketingHelper, requestData);

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

		public virtual bool ValidateRequest(out string reason, out StatusCode statusCode)
		{
			if (string.IsNullOrWhiteSpace(RequestData.RawBody))
			{
				reason = "The request body is empty";
				statusCode = StatusCode.BadRequest;
				return false;
			}

			// Default success
			reason = string.Empty;
			statusCode = StatusCode.Ok;
			return true;
		}

		public abstract ApiTriggerOutput HandleRequest();
	}
}