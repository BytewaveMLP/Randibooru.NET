using System;
using CommandLine;
using NLog;
using DSharpPlus;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Randibooru {
	class Randibooru {
		private Options options;
		private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

		public static Logger Log {
			get { return nlog; }
		}

		static void Main(string[] args) {
			var options = new Options();
			if (Parser.Default.ParseArguments(args, options)) {
				new Randibooru(options).Start();
			}
		}

		private Randibooru(Options options) {
			this.options = options;

			if (this.options.Verbose) {
				foreach (var role in LogManager.Configuration.LoggingRules) {
					role.EnableLoggingForLevel(NLog.LogLevel.Debug);
				}

				LogManager.ReconfigExistingLoggers();
			}
		}

		public void Start() {
			var _client = new DiscordClient(new DiscordConfig() {
				Token = options.DiscordAPIKey,
				TokenType = TokenType.Bot,
				DiscordBranch = Branch.Canary,
				LogLevel = DSharpPlus.LogLevel.Debug,
				UseInternalLogHandler = false,
				AutoReconnect = true,
			});

			Random rnd = new Random();

			_client.MessageCreated += async (sender, e) => {
				if (!e.Message.Author.IsBot) {
					if (e.Message.Content.StartsWith("+rb ")) {
						var author = e.Message.Author;
						var content = e.Message.Content;
						var query = content.Substring(4);

						Log.Debug("Request received!");
						Log.Debug("    User:  @{0}#{1}", author.Username, author.Discriminator);
						Log.Debug("    ID:    {0}", author.ID);
						Log.Debug("    Query: {0}", query);
						DiscordEmbed embed = new DiscordEmbed {
							Title = "Image",
							Description = "**Query:** " + query,
							Type = "rich",
							Color = rnd.Next(0, 9999999),
							Image = new DiscordEmbedImage() {
								Url = "https://derpicdn.net/img/view/2016/9/22/1255688__safe_solo_cute_smiling_derpy+hooves_underhoof_glass_derpabetes_against+glass_artist-colon-nimaru.png"
							}
						};

						await e.Message.Respond($"{e.Message.Author.Mention}", false, embed);
					}
				}
			};

			_client.Ready += async (sender, e) => {
				await Task.Run(() => {
					Log.Info("Connected!");
					var user = _client.Me;
					Log.Debug("Logged in as:");
					Log.Debug("    User: @{0}#{1}", user.Username, user.Discriminator);
					Log.Debug("    ID:   {0}", user.ID);
				});
			};

			Log.Info("Connecting to Discord...");
			_client.Connect();

			Console.ReadKey(true);
		}
	}
}
