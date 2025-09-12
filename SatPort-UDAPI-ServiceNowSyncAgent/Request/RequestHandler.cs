namespace Skyline.Automation.SatPort.Request
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.SDM.Ticketing;

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

		public static RequestHandler InitializeHandlerByMethod(Engine engine, ApiTriggerInput requestData)
		{
			var ticketingHelper = new TicketingApiHelper(engine.GetUserConnection());
			switch (requestData.RequestMethod)
			{
				case RequestMethod.Post:
					return new UpdateTicketRequestHandler(ticketingHelper, requestData.RawBody);

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
			if (string.IsNullOrWhiteSpace(Request))
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