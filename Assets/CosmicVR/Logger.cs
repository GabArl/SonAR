using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CosmicVR
{
	public class SoniLog
	{

		class PanelAudifications
		{


		}


		class LogField
		{
			public InfoType InfoType;
			public LogEntry entry;
		}
		class LogEntry
		{
			float[] batch;

		}
		class LogBatch
		{
		}
		public enum InfoType { Active, Time, Peak, Total }

		public enum MessageType { Start, Stop, Update, Add, Remove }

		public enum SonificationType { Empty = -1, None = 0, Sinification = 1, AuditoryIcon = 2, Earcon = 3, ParameterMapping = 4, ModelBased = 5 }

		private Dictionary<LogField, string> dict_sinification = new Dictionary<LogField, string>() {
			{ new LogField{ InfoType = InfoType.Active } ,       "Active Sinifications      : " },
			{ new LogField{ InfoType = InfoType.Time } ,         "Interstimulus Time        : " },
			{ new LogField{ InfoType = InfoType.Peak } ,         "Peak of active at a time  : " },
			{ new LogField{ InfoType = InfoType.Total } ,        "Total Played in Session   : " }
		};
		private Dictionary<LogField, string> dict_auditoryIcon = new Dictionary<LogField, string>() {
			{ new LogField{ InfoType = InfoType.Active } ,       "Active Auditory Icons     : " },
			{ new LogField{ InfoType = InfoType.Time } ,         "Interstimulus Time        : " },
			{ new LogField{ InfoType = InfoType.Peak } ,         "Peak of active at a time  : " },
			{ new LogField{ InfoType = InfoType.Total } ,        "Total Played in Session   : " }
		};
		private Dictionary<LogField, string> dict_earcon = new Dictionary<LogField, string>() {
			{ new LogField{ InfoType = InfoType.Active } ,       "Active Earcons            : " },
			{ new LogField{ InfoType = InfoType.Time } ,         "Interstimulus Time        : " },
			{ new LogField{ InfoType = InfoType.Peak } ,         "Peak of active at a time  : " },
			{ new LogField{ InfoType = InfoType.Total } ,        "Total Played in Session   : " }
		};
		private Dictionary<LogField, string> dict_parameterMapping = new Dictionary<LogField, string>() {
			{ new LogField{ InfoType = InfoType.Active } ,       "Active writing mappings   : " },
			{ new LogField{ InfoType = InfoType.Peak } ,         "Peak of writes at a time  : " },
			{ new LogField{ InfoType = InfoType.Total } ,        "Total Parameter Mappings  : " }
		};
		private Dictionary<LogField, string> dict_modelBased = new Dictionary<LogField, string>() {
			{ new LogField{ InfoType = InfoType.Active } ,       "Active exitations         : " },
			{ new LogField{ InfoType = InfoType.Time } ,         "Interstimulus Time        : " },
			{ new LogField{ InfoType = InfoType.Peak } ,         "Peak of active at a time  : " },
			{ new LogField{ InfoType = InfoType.Total } ,        "Total exitations	        : " }
		};

		public static void Message(SonificationType sType)
		{
			if (sType == SonificationType.None)
			{
				return;
			}
			switch (sType)
			{
				case SonificationType.Sinification:
					break;
				case SonificationType.AuditoryIcon:
					break;
				case SonificationType.Earcon:
					break;
				case SonificationType.ParameterMapping:
					break;
				case SonificationType.ModelBased:
					break;
				default:
					break;
			}
		}


		private void UpdateCategory() { }




	}

}