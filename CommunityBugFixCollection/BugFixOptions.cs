using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    internal sealed class BugFixOptions : ConfigSection
    {
        private static readonly DefiningConfigKey<bool> _useIecByteFormat = new("UseIecByteFormat", "Whether to format bytes using IEC as opposed to decimal format when <i>LocalizedByteFormatting</i> is enabled.", () => true);

        /// <inheritdoc/>
        public override string Description => "Contains the settings for the few fixes that offer them.";

        /// <inheritdoc/>
        public override string Id => "Options";

        /// <summary>
        /// Gets whether <see cref="LocalizedByteFormatting"/> should format bytes using IEC or decimal format.
        /// </summary>
        public bool UseIecByteFormat => _useIecByteFormat.GetValue();

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 0, 0);
    }
}