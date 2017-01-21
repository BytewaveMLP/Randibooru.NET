using CommandLine;
using CommandLine.Text;

namespace Randibooru {
	class Options {
		[Option("discord-key", Required = true,
			HelpText = "Your Discord API key")]
		public string DiscordAPIKey { get; set; }

		[Option("derpi-key",
			HelpText = "Your Derpibooru API key (needed for images not visible under Default filter)")]
		public string DerpibooruAPIKey { get; set; }

		[Option('v', "verbose", DefaultValue = false,
			HelpText = "Prints all requests to stdout")]
		public bool Verbose { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage() {
			return HelpText.AutoBuild(this,
				(HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
