namespace Skyline.Automation.SatPort.Request
{
	using System;
	using System.Linq;
	using Skyline.Automation.SatPort.DataModel;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.Ticketing;
	using Skyline.DataMiner.SDM.Ticketing.Models;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

	public sealed class UpdateTicketRequestHandler : RequestHandler
	{
		public UpdateTicketRequestHandler(TicketingApiHelper helper, string request) : base(helper, request)
		{
		}

		public UpdateTicketDataModel DataModel { get; private set; }

		public override string IncidentId { get; protected set; }

		public override RequestMethod Method => RequestMethod.Post;

		public override bool ValidateRequest(out string reason, out StatusCode statusCode)
		{
			if (!base.ValidateRequest(out reason, out statusCode))
			{
				return false;
			}

			try
			{
				DataModel = SecureNewtonsoftDeserialization.DeserializeObject<UpdateTicketDataModel>(Request);
				if (DataModel is null)
				{
					reason = "The request body could not be deserialized into a valid UpdateTicketDataModel.";
					statusCode = StatusCode.BadRequest;
					return false;
				}

				IncidentId = DataModel.Number;

				if (!base.ValidateRequest(out reason, out statusCode))
				{
					return false;
				}

				// Everything looks valid
				reason = string.Empty;
				statusCode = StatusCode.Ok;
				return true;
			}
			catch (Exception ex)
			{
				reason = $"The request body is not valid JSON or does not match the expected schema. Details: {ex.Message}";
				statusCode = StatusCode.BadRequest;
				return false;
			}
		}

		public override ApiTriggerOutput HandleRequest()
		{
			var output = new ApiTriggerOutput();

			try
			{
				if (!Enum.TryParse<TicketStatus>(DataModel.State, true, out var parsedState))
				{
					throw new InvalidOperationException($"Unable to change the state of the ticket {IncidentId}. The state {DataModel.State} is invalid.");
				}

				var ticket = Helper.Tickets.Read(TicketExposers.ExternalIdentifiers.ExternalId.Equal(IncidentId)).FirstOrDefault();
				ticket.Status = parsedState;
				Helper.Tickets.Update(ticket);

				output.ResponseCode = (int)StatusCode.Ok;
				output.ResponseBody = "Ticket updated successfully";
			}
			catch (Exception ex)
			{
				output.ResponseCode = (int)StatusCode.InternalServerError;
				output.ResponseBody = $"Internal Server Error: {ex}";
			}

			return output;
		}
	}
}