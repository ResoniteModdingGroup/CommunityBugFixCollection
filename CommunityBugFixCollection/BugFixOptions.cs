using MonkeyLoader.Configuration;

namespace CommunityBugFixCollection
{
    internal sealed class BugFixOptions : SingletonConfigSection<BugFixOptions>
    {
        private static readonly DefiningConfigKey<bool> _forceAprilFools = new("ForceAprilFools", "Whether to force April Fools content to be active.", () => false);

        private static readonly DefiningConfigKey<bool> _useIecByteFormat = new("UseIecByteFormat", "Whether to format bytes using IEC as opposed to decimal format when <i>LocalizedByteFormatting</i> is enabled.", () => true);

        /// <inheritdoc/>
        public override string Description => "Contains the settings for the few fixes that offer them.";

        /// <summary>
        /// Gets whether to force April Fools content to be active.
        /// </summary>
        public bool ForceAprilFools => _forceAprilFools;

        /// <inheritdoc/>
        public override string Id => "Options";

        /// <summary>
        /// Gets whether <see cref="LocalizedByteFormatting"/> should format bytes using IEC or decimal format.
        /// </summary>
        public bool UseIecByteFormat => _useIecByteFormat;

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 1, 0);
    }
}