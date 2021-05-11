using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace WalletWasabi.Gui.Rpc
{
	/// <summary>
	/// A rpc call is represented by sending a Request object to a Server. The Request object has the following members:
	/// + jsonrpc - A String specifying the version of the JSON-RPC protocol. MUST be exactly "2.0".
	/// + method  - A String containing the name of the method to be invoked. Method names that begin with the word rpc
	///             followed by a period character (U+002E or ASCII 46) are reserved for rpc-internal methods and extensions
	///             and MUST NOT be used for anything else.
	/// + params  - A Structured value that holds the parameter values to be used during the invocation of the method. This
	///             member MAY be omitted.
	/// + id      - An identifier established by the Client that MUST contain a String, Number, or NULL value if included.
	///             If it is not included it is assumed to be a notification. The value SHOULD normally not be Null [1] and
	///             Numbers SHOULD NOT contain fractional parts [2].
	/// </summary>
	public class JsonRpcRequest
	{
		/// <summary>
		/// Constructor used to deserialize the requests.
		/// </summary>
		[JsonConstructor]
		public JsonRpcRequest(string jsonrpc, string id, string method, JToken parameters)
		{
			JsonRPC = jsonrpc;
			Id = id;
			Method = method;
			Parameters = parameters;
		}

		/// <summary>
		/// Gets a value indicating whether the JsonRpcRequest is a notification request
		/// which means the client is not interested in receiving a response.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(Id))]
		public bool IsNotification => Id is null;

		/// <summary>
		/// Gets the version of the JSON-RPC protocol. MUST be exactly "2.0".
		/// </summary>
		[JsonProperty("jsonrpc", Required = Required.Default)]
		public string JsonRPC { get; }

		/// <summary>
		/// Gets the identifier established by the Client that MUST contain a String,
		/// Number, or NULL value if included. If it is not included it is assumed
		/// to be a notification.
		/// The value SHOULD normally not be Null and Numbers SHOULD NOT contain
		/// fractional parts.
		/// The use of Null as a value for the id member in a Request object is
		/// discouraged, because this specification uses a value of Null for Responses
		/// with an unknown id. Also, because JSON-RPC 1.0 uses an id value of Null
		/// for Notifications this could cause confusion in handling.
		/// The Server MUST reply with the same value in the Response object if included.
		/// This member is used to correlate the context between the two objects.
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; }

		/// <summary>
		/// Gets the name of the method to be invoked. Method names that
		/// begin with the word rpc followed by a period character (U+002E or ASCII 46)
		/// are reserved for rpc-internal methods and extensions and MUST NOT be used
		/// for anything else.
		/// </summary>
		[JsonProperty("method", Required = Required.Always)]
		public string Method { get; }

		/// <summary>
		/// Gets a structured value that holds the parameter values to be used during the
		/// invocation of the method. This member MAY be omitted.
		/// </summary>
		[JsonProperty("params")]
		public JToken Parameters { get; }

		/// <summary>
		/// Parses the json rpc request giving back the deserialized JsonRpcRequest instance.
		/// Return true if the deserialization was successful, otherwise false.
		/// </summary>
		public static bool TryParse(string rawJson, [NotNullWhen(true)] out JsonRpcRequest[]? requests, out bool isBatch)
		{
			try
			{
				isBatch = rawJson.TrimStart().StartsWith("[");
				rawJson = isBatch ? rawJson : $"[{rawJson}]";
				if (!rawJson.Contains("\""))
				{
					rawJson = RebuildJson(rawJson);
				}

				requests = JsonConvert.DeserializeObject<JsonRpcRequest[]>(rawJson);
				return true;
			}
			catch (JsonException)
			{
				requests = null;
				isBatch = false;
				return false;
			}
		}

		/// <summary>
		/// Rebuilds the wrong JSON, so the parser can work with it.
		/// curl -s --data-binary '{"jsonrpc":"2.0","id":"1","method":"getstatus"}' http://127.0.0.1:37128/ is read as '{jsonrpc:2.0,id:1,method:getstatus}'
		/// which is bad, because we need the " characters.
		/// This function adds " chars to the wrongly read JSON string.
		/// SIDE NOTE: It's not finished yet, doesn't handle multiple params like: "params":["Alice","Bob"].
		/// </summary>
		/// <param name="rawJson">The wrong JSON string</param>
		/// <returns>Fixed JSON string</returns>
		private static string RebuildJson(string rawJson)
		{
			StringBuilder strBuilder = new();
			List<string[]> wordsMatrix = new();
			List<string> fixedWordsList = new();
			List<string> fixedParametersCombined = new();

			var split = rawJson.Split('{', '}');

			//TODO splits "params":["Alice", "Bob"] like "params":["Alice" | "Bob"]. Should not split inside there.
			var parameters = split[1].ToString().Split(',');

			foreach (var param in parameters)
			{
				wordsMatrix.Add(param.Split(":"));
			}

			for (int i = 0; i < wordsMatrix.Count; i++)
			{
				for (int j = 0; j < wordsMatrix[i].Length; j++)
				{
					string currentWord = "";

					//If we have parameters like ["Alice","Test"]
					//example: {"jsonrpc":"2.0","id":"1","method":"getnewaddress","params":["Daniel, Alice"]}
					if (wordsMatrix[i][j].ToString().Contains("["))
					{
						var everyParameterInOneLine = wordsMatrix[i][j].ToString().Split('[', ']');
						var parametersSeperated = everyParameterInOneLine[1].Split(',');
						string result = "[";

						//If we have only one parameter like "params":["WalletName"]
						//then we just add " " to it
						if (parametersSeperated.Length == 1)
						{
							result += "\"" + parametersSeperated[0] + "\"";
						}
						//If we have more parameters like "params":["Bob","Alice"]
						//then we iterate through them and add " " to them and ',' except to the last one
						else
						{
							for (int k = 0; k < parametersSeperated.Length; k++)
							{
								if (k == parametersSeperated.Length - 1)
								{
									result += "\"" + parametersSeperated[k] + "\"";
								}
								else
								{
									result += "\"" + parametersSeperated[k] + "\"" + ",";
								}
							}
						}

						result += "]";
						currentWord = result;
					}
					else
					{
						currentWord = "\"" + wordsMatrix[i][j].ToString() + "\"";
					}

					fixedWordsList.Add(currentWord);
				}
			}

			for (int i = 0; i < fixedWordsList.Count - 1; i += 2)
			{
				fixedParametersCombined.Add(fixedWordsList[i] + ":" + fixedWordsList[i + 1]);
			}

			strBuilder.Append("[{");

			strBuilder.Append(string.Join(',', fixedParametersCombined));

			strBuilder.Append("}]");

			return strBuilder.ToString();
		}
	}
}
