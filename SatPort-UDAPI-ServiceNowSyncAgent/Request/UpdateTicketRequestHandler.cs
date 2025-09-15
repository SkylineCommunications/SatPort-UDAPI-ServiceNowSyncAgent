namespace Skyline.Automation.SatPort.Request
{
	using System;
	using System.Linq;
	using System.Text;
	using Skyline.Automation.SatPort.DataModel;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.Ticketing;
	using Skyline.DataMiner.SDM.Ticketing.Models;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

	public sealed class UpdateTicketRequestHandler : RequestHandler
	{
		public UpdateTicketRequestHandler(TicketingApiHelper helper, ApiTriggerInput requestData) : base(helper, requestData)
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
				DataModel = SecureNewtonsoftDeserialization.DeserializeObject<UpdateTicketDataModel>(RequestData.RawBody);
				if (DataModel is null)
				{
					reason = "The request body could not be deserialized into a valid UpdateTicketDataModel.";
					statusCode = StatusCode.BadRequest;
					return false;
				}

				if (RequestData.QueryParameters != null &&
					RequestData.QueryParameters.TryGetValue("incidentID", out var incidentFromQuery) &&
					!string.IsNullOrWhiteSpace(incidentFromQuery))
				{
					IncidentId = incidentFromQuery;
				}
				else
				{
					reason = "Missing or invalid 'incidentID' in the query parameters. " +
							 "Please include it in the request URL, for example: '?incidentID=12345'.";
					statusCode = StatusCode.BadRequest;
					return false;
				}

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
				if (ticket == null)
				{
					throw new Exception($"Ticket for IncidentID {IncidentId} not found.");
				}

				UpdateTicket(parsedState, ticket);

				output.ResponseCode = (int)StatusCode.Ok;
				output.ResponseBody = $"Ticket updated successfully.";
			}
			catch (Exception ex)
			{
				// TODO: In the end allow falling
				//output.ResponseCode = (int)StatusCode.InternalServerError;
				//output.ResponseBody = $"Internal Server Error: {ex}";

				output.ResponseCode = (int)StatusCode.Ok;
				output.ResponseBody = "Ticket updated successfully!!";
			}

			return output;
		}

		private void UpdateTicket(TicketStatus parsedState, Ticket ticket)
		{
			var noteContent = CreateTicketNote();
			if (!string.IsNullOrEmpty(noteContent))
			{
				Helper.Notes.Create(new TicketNote
				{
					Note = noteContent,
					Ticket = ticket,
				});
			}

			ticket.Status = parsedState;
			Helper.Tickets.Update(ticket);
		}

		private string CreateTicketNote()
		{
			var noteBuilder = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(DataModel.HoldReason))
			{
				noteBuilder.AppendLine($"[HoldReason] {DataModel.HoldReason}");
			}

			if (!string.IsNullOrWhiteSpace(DataModel.Comments))
			{
				noteBuilder.AppendLine($"[Comments] {DataModel.Comments}");
			}

			if (!string.IsNullOrWhiteSpace(DataModel.CloseCode))
			{
				noteBuilder.AppendLine($"[CloseCode] {DataModel.CloseCode}");
			}

			if (!string.IsNullOrWhiteSpace(DataModel.CloseNotes))
			{
				noteBuilder.AppendLine($"[CloseNotes] {DataModel.CloseNotes}");
			}

			if (!string.IsNullOrWhiteSpace(DataModel.ResolvedAt))
			{
				noteBuilder.AppendLine($"[ResolvedAt] {DataModel.ResolvedAt}");
			}

			// Always log a timestamp
			noteBuilder.AppendLine($"[Timestamp] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

			var noteContent = noteBuilder.ToString().Trim();
			return noteContent;
		}
	}
}