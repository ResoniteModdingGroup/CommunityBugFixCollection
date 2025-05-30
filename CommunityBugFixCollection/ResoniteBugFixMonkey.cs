using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    /// <summary>
    /// Represents the base class for patchers that run after Resonite's assemblies
    /// have been loaded and that hook into the game's lifecycle.<br/>
    /// Defaults <see cref="CanBeDisabled">CanBeDisabled</see> to <see langword="true"/>
    /// and provides the <see cref="BugFixOptions"/> as a <see cref="ConfigSection">ConfigSection</see>.
    /// </summary>
    /// <inheritdoc/>
    internal abstract class ResoniteBugFixMonkey<TMonkey> : ResoniteMonkey<TMonkey>, IConfiguredMonkey<BugFixOptions>
        where TMonkey : ResoniteBugFixMonkey<TMonkey>, new()
    {
        /// <inheritdoc/>
        public abstract override IEnumerable<string> Authors { get; }

        /// <remarks>
        /// <i>By default:</i> <see langword="true"/>
        /// </remarks>
        /// <inheritdoc/>
        public override bool CanBeDisabled => true;

        BugFixOptions IConfiguredMonkey<BugFixOptions>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        /// <remarks>
        /// This is always the <see cref="BugFixOptions"/>.<see cref="SingletonConfigSection{TConfigSection}.Instance">Instance</see>.
        /// </remarks>
        /// <inheritdoc cref="IConfiguredMonkey{TConfigSection}.ConfigSection"/>
        protected static BugFixOptions ConfigSection => BugFixOptions.Instance;

        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            if (BugFixOptions.Instance is null)
                Config.LoadSection<BugFixOptions>();

            return base.OnLoaded();
        }
    }
}