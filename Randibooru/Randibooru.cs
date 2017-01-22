using System;
using System.Threading.Tasks;
using CommandLine;
using NLog;
using DSharpPlus;
using Coolbooru;
using static Coolbooru.Coolbooru;

namespace Randibooru {
	class Randibooru {
		private Options options;
		private static readonly Logger nlog = LogManager.GetCurrentClassLogger();
		private CoolSearchQuery sq;

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

			sq = new CoolSearchQuery();

			sq.sf = CoolSearchSort.Random;

			if (this.options.DerpibooruAPIKey != null) {
				sq.key = this.options.DerpibooruAPIKey;
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

						this.sq.q = query;
						var s = await search(this.sq);
						var res = s.search;

						if (res.Count > 0) {
							var dEmbed = await ConstructDiscordEmbed(res[0]);

							await e.Message.Respond($"{e.Message.Author.Mention}", false, dEmbed);
						} else {
							await e.Message.Respond($"{e.Message.Author.Mention}: **No images found.**");
						}
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

		public async Task<DiscordEmbed> ConstructDiscordEmbed(CoolItem img) {
			var cEmbed = await embed(Int32.Parse(img.id));
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
