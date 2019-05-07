using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.7.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_7 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_7()
            : base(new SemanticVersion(1, 7, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.HasReadLetter.ToString(),
                ConditionType.HasValue.ToString(),
                ConditionType.IsMainPlayer.ToString()
            };
        }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public override bool TryMigrate(ref ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // 1.7 adds nested tokens
            if (lexToken is LexTokenToken token && token.InputArg?.Parts.Any(p => p.Type == LexTokenType.Token) == true)
            {
                error = this.GetNounPhraseError($"using nested tokens like '{lexToken.Text}'");
                return false;
            }

            return true;
        }

        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the content pack was successfully migrated.</returns>
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.7 adds tokens in field keys
            foreach (PatchConfig patch in content.Changes)
            {
                if (patch.Fields != null && patch.Fields.Keys.Any(p => p.Contains("{{")))
                {
                    error = this.GetNounPhraseError("using tokens in field keys");
                    return false;
                }
            }

            return true;
        }
    }
}
