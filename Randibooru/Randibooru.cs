using System;
using System.Threading.Tasks;
using CommandLine;
using NLog;
using DSharpPlus;
using Coolbooru;

namespace Randibooru {
	class Randibooru {
		private Options options;
		private CoolSearchQuery sq;
		private static bool isConnected;
		private static readonly Logger nlog = LogManager.GetCurrentClassLogger();
		private static DiscordClient _client;
		public static Logger Log {
			get { return nlog; }
		}

		static void Main(string[] args) {
			Console.CancelKeyPress += delegate {
				if (isConnected) {
					Log.Warn("CTRL+C pressed while client was connected! Shutting down gracefully...");
					_client.Disconnect();
				}

				Log.Info("Exitting Randibooru.NET... Goodbye!");
			};

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

			sq = new CoolSearchQuery();

			sq.SortFormat = CoolStuff.SORT_RANDOM;

			if (this.options.DerpibooruAPIKey != null) {
				sq.APIKey = this.options.DerpibooruAPIKey;
			}
		}

		public void Start() {
			_client = new DiscordClient(new DiscordConfig() {
				Token = options.DiscordAPIKey,
				TokenType = TokenType.Bot,
				DiscordBranch = Branch.Canary,
				LogLevel = DSharpPlus.LogLevel.Debug,
				UseInternalLogHandler = false,
				AutoReconnect = false,
			});

			_client.MessageCreated += async (sender, e) => {
				if (!e.Message.Author.IsBot) {
					var message = e.Message;
					var content = message.Content;
					var author = message.Author;

					if (content.Equals("+rb") || (content.Length >= 4 && content.StartsWith("+rb "))) {
						await e.Channel.TriggerTyping();

						var query = "*";

						if (content.Length > 4) {
							query = content.Substring(4);
						}

						Log.Debug("Request received!");
						Log.Debug("    User:  @{0}#{1}", author.Username, author.Discriminator);
						Log.Debug("    ID:    {0}", author.ID);
						Log.Debug("    Query: {0}", query);

						this.sq.Query = query;
						var s = await CoolStuff.Search(this.sq);
						var res = s.search;

						if (res.Count > 0) {
							var dEmbed = await ConstructDiscordEmbed(res[0]);

							await e.Message.Respond($"{e.Message.Author.Mention}", false, dEmbed);
						} else {
							await e.Message.Respond($"{e.Message.Author.Mention}: **No images found.**");
						}
					} else if (content.Equals("+rbhelp")) {
						Log.Debug("Help request received!");
						Log.Debug("    User:  @{0}#{1}", author.Username, author.Discriminator);
						Log.Debug("    ID:    {0}", author.ID);

						await e.Message.Respond($"{e.Message.Author.Mention}: **Randibooru Help**\n"
							+ "`+rb [<query>]` - Pulls a random image from Derpibooru, optionally matching the given query.\n"
							+ "    - `query` - The Derpibooru query to match against. See https://derpibooru.org/search/syntax for query syntax information.");
					}
				}
			};

			_client.Ready += async (sender, e) => {
				await Task.Run(() => {
					isConnected = true;
					Log.Info("Connected!");
					var user = _client.Me;
					Log.Debug("Logged in as:");
					Log.Debug("    User: @{0}#{1}", user.Username, user.Discriminator);
					Log.Debug("    ID:   {0}", user.ID);
				});

				await _client.UpdateStatus("+rbhelp for help");
			};

			Log.Info("Connecting to Discord...");
			_client.Connect();

			Console.ReadKey(true);
		}

		public async Task<DiscordEmbed> ConstructDiscordEmbed(CoolItem img) {
			var cEmbed = await CoolStuff.Embed(Int32.Parse(img.id));
			DiscordEmbed dEmbed = new DiscordEmbed {
				Title = "Derpibooru Image",
				Description = "**Tags:** " + string.Join(", ", cEmbed.derpibooru_tags),
				Url = cEmbed.provider_url,
				Author = new DiscordEmbedAuthor {
					Url = cEmbed.author_url,
					Name = cEmbed.author_name
				},
				Image = new DiscordEmbedImage {
					Url = "https:" + cEmbed.thumbnail_url,
				},
				Provider = new DiscordEmbedProvider {
					Url = cEmbed.provider_url,
					Name = cEmbed.provider_name,
				},
				Type = "rich",
			};
			return dEmbed;
		}
	}
}
