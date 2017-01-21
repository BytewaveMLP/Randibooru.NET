using System;
using Discord;
using Discord.Commands;
using CommandLine;

namespace Randibooru {
	class Randibooru {
		private DiscordClient _client;

		static void Main(string[] args) {
			var options = new Options();
			if (Parser.Default.ParseArguments(args, options)) {
				new Randibooru().Start();
			}
		}

		public void Start() {
			_client = new DiscordClient();

			_client.UsingCommands(x => {
				x.PrefixChar = '+';
				x.HelpMode = HelpMode.Public;
			});

			_client.GetService<CommandService>().CreateGroup("derpi", cgb => {
				cgb.CreateCommand("random")
					.Alias(new string[] {"rb"})
					.Description("Pulls a random image from Derpibooru with the given query")
					.Parameter("Query", ParameterType.Unparsed)
					.Do(async e => {
						await e.Channel.SendMessage(e.GetArg("Query"));
					});
			});

			_client.ExecuteAndWait(async () => {
				await Console.Out.WriteLineAsync("Logging in to Discord...");
				await _client.Connect("MjQ5MzIzMDI3MzY2NTQzMzYw.C1nIAQ.txMT7UObHJPCtlMYkf5yKj1xu0M", TokenType.Bot);
				await Console.Out.WriteLineAsync("Success!");
				Profile user = _client.CurrentUser;
				await Console.Out.WriteLineAsync("-------------------");
				await Console.Out.WriteLineAsync("USER: @" + user.Name + "#" + user.Discriminator);
				await Console.Out.WriteLineAsync("ID:   " + user.Id);
			});
		}
	}
}
