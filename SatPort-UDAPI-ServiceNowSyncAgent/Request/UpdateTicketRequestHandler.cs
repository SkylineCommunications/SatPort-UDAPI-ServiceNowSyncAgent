using System;

namespace SatPortUDAPIServiceNowSyncAgent.Request
{
	using System.Linq;
	using Newtonsoft.Json;
	using SatPortUDAPIServiceNowSyncAgent.DataModel;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.Ticketing;
	using Skyline.DataMiner.SDM.Ticketing.Models;

	public sealed class UpdateTicketRequestHandler : RequestHandler
	{
		public UpdateTicketRequestHandler(TicketingApiHelper helper, string request) : base(helper, request)
		{
		}

		public UpdateTicketDataModel DataModel { get; private set; }

		public override string IncidentId { get; protected set; }

		public override RequestMethod Method => RequestMethod.Post;

		public override bool Validate(out string reason, out StatusCode statusCode)
		{
			try
			{
				DataModel = JsonConvert.DeserializeObject<UpdateTicketDataModel>(Request);

				IncidentId = DataModel.IncidentID;

				if (!base.Validate(out reason, out statusCode))
				{
					return false;
				}


				return !(DataModel is null);
			}
			catch (Exception ex)
			{
				reason = $"The request body is not valid. {ex.Message}";
				statusCode = StatusCode.BadRequest;
				return false;
			}
		}

		public override ApiTriggerOutput HandleRequest()
		{
			var output = new ApiTriggerOutput();

			try
			{
				var ticket = Helper.Tickets.Read(TicketExposers.ID.Equal(DataModel.IncidentID)).First();

				if (!Enum.TryParse<TicketStatus>(DataModel.IncidentState, true, out var parsedState))
				{
					throw new InvalidOperationException($"Unable to change the state of the ticket {IncidentId}");
				}

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