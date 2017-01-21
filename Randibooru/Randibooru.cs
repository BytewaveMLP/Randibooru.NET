using System;
using Discord;
using Discord.Net;
using Discord.Commands;
using CommandLine;

namespace Randibooru {
	class Randibooru {
		private DiscordClient _client;
		private Options options;

		static void Main(string[] args) {
			var options = new Options();
			if (Parser.Default.ParseArguments(args, options)) {
				new Randibooru(options).Start();
			}
		}

		private Randibooru(Options options) {
			this.options = options;
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

				try {
					await _client.Connect(this.options.DiscordAPIKey, TokenType.Bot);
				} catch (HttpException e) {
					await Console.Error.WriteLineAsync("[FATAL]: " + e.Message);
					await Console.Error.WriteLineAsync("Ensure your Discord API key is correct, and check");
					await Console.Error.WriteLineAsync("    https://status.discordapp.com");
					await Console.Error.WriteLineAsync("for any outages.");
					Environment.Exit(127);
				}
				await Console.Out.WriteLineAsync("Successfully logged into Discord!");
				if (this.options.Verbose) {
					Profile user = _client.CurrentUser;
					await Console.Out.WriteLineAsync("USER: @" + user.Name + "#" + user.Discriminator);
					await Console.Out.WriteLineAsync("ID:   " + user.Id);
				}
			});
		}
	}
}
