namespace Skyline.Automation.SatPort.DataModel
{
	using Newtonsoft.Json;

	public class UpdateTicketDataModel
	{
		[JsonProperty("number")]
		public string Number { get; set; } // User friendly ServiceNow Incident reference

		[JsonProperty("external_number")]
		public string ExternalNumber { get; set; } // User friendly Dataminer Ticket reference

		[JsonProperty("state")]
		public string State { get; set; } // // (In Progress / On Hold / Resolved / Closed / Canceled)

		[JsonProperty("hold_reason")]
		public string HoldReason { get; set; } // Empty if State is not "On Hold" , e.g. “Awaiting Vendor”

		[JsonProperty("comments")]
		public string Comments { get; set; } // e.g. “On hold comment”

		[JsonProperty("close_code")]
		public string CloseCode { get; set; } // Only when State is "Resolved"

		[JsonProperty("close_notes")]
		public string CloseNotes { get; set; } // Only when State is "Resolved"

		[JsonProperty("resolved_at")]
		public string ResolvedAt { get; set; } // Only when State is "Resolved". TZ = GMT & in format "YYYY-MM-DD HH:MM:SS"

		[JsonProperty("priority")]
		public string Priority { get; set; } // (1 – Critical / 2 – High / 3 – Moderate / 4 – Low / 5 - Planning)
	}
}