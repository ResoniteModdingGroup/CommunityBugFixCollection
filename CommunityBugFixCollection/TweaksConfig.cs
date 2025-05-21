using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using MonkeyLoader.Resonite.UI.Inspectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    internal sealed class BugFixCollectionTweaksConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<bool> _iecFormatBytes = new("IEC format bytes", "Whether to format bytes in IEC or decimal format when <i>LocalizedByteFormatting</i> is enabled.", () => true);

        /// <summary>
        /// Gets the session share for whether Resonite Wiki buttons on component categories in Component Selectors should be visible.
        /// </summary>
        public bool IecFormatBytes => _iecFormatBytes.GetValue();

        /// <inheritdoc/>
        public override string Description => "Contains tweaks for bugfix.";


        /// <inheritdoc/>
        public override string Id => "Tweaks";

        /// <inheritdoc/>
        public override Version Version { get; } = new(1, 0, 0);
    }
}