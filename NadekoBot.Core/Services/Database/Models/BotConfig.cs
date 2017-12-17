﻿using NadekoBot.Core.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NadekoBot.Core.Services.Database.Models
{
    public class BotConfig : DbEntity
    {
        public HashSet<BlacklistItem> Blacklist { get; set; }
        public HashSet<GlobalWhitelistItem> GlobalWhitelistMembers { get; set; }
        public HashSet<GlobalWhitelistSet> GlobalWhitelistGroups { get; set; }
        public ulong BufferSize { get; set; } = 4000000;
        public bool ForwardMessages { get; set; } = true;
        public bool ForwardToAllOwners { get; set; } = true;

        public float CurrencyGenerationChance { get; set; } = 0.02f;
        public int CurrencyGenerationCooldown { get; set; } = 10;

        public List<PlayingStatus> RotatingStatusMessages { get; set; } = new List<PlayingStatus>();

        public bool RotatingStatuses { get; set; } = false;
        public string RemindMessageFormat { get; set; } = "❗⏰**I've been told to remind you to '%message%' now by %user%.**⏰❗";
        
        //currency
        public string CurrencySign { get; set; } = "🌸";
        public string CurrencyName { get; set; } = "Nadeko Flower";
        public string CurrencyPluralName { get; set; } = "Nadeko Flowers";

        public int TriviaCurrencyReward { get; set; } = 0;
        public int MinimumBetAmount { get; set; } = 2;
        public float BetflipMultiplier { get; set; } = 1.95f;
        public int CurrencyDropAmount { get; set; } = 1;
        public int? CurrencyDropAmountMax { get; set; } = null;
        public float Betroll67Multiplier { get; set; } = 2;
        public float Betroll91Multiplier { get; set; } = 4;
        public float Betroll100Multiplier { get; set; } = 10;
        public int TimelyCurrency { get; set; } = 0;
        public int TimelyCurrencyPeriod { get; set; } = 0;
        //public HashSet<CommandCost> CommandCosts { get; set; } = new HashSet<CommandCost>();

        /// <summary>
        /// I messed up, don't use
        /// </summary>
        public HashSet<CommandPrice> CommandPrices { get; set; } = new HashSet<CommandPrice>();


        public HashSet<EightBallResponse> EightBallResponses { get; set; } = new HashSet<EightBallResponse>();
        public HashSet<RaceAnimal> RaceAnimals { get; set; } = new HashSet<RaceAnimal>();

        public string DMHelpString { get; set; } = "Type `.h` for help.";
        public string HelpString { get; set; } = @"To add me to your server, use this link -> <https://discordapp.com/oauth2/authorize?client_id={0}&scope=bot&permissions=66186303>
You can use `{1}modules` command to see a list of all modules.
You can use `{1}commands ModuleName`
(for example `{1}commands Administration`) to see a list of all of the commands in that module.
For a specific command help, use `{1}h CommandName` (for example {1}h {1}q)


**LIST OF COMMANDS CAN BE FOUND ON THIS LINK**
<http://nadekobot.readthedocs.io/en/latest/Commands%20List/>


Nadeko Support Server: https://discord.gg/nadekobot";

        public int MigrationVersion { get; set; }

        public string OkColor { get; set; } = "71cd40";
        public string ErrorColor { get; set; } = "ee281f";
        public string Locale { get; set; } = null;
        public List<StartupCommand> StartupCommands { get; set; }
        public HashSet<BlockedCmdOrMdl> BlockedCommands { get; set; }
        public HashSet<BlockedCmdOrMdl> BlockedModules { get; set; }

        public HashSet<UnblockedCmdOrMdl> UnblockedCommands { get; set; }
        public HashSet<UnblockedCmdOrMdl> UnblockedModules { get; set; }
        public int PermissionVersion { get; set; }
        public string DefaultPrefix { get; set; } = ".";
        public bool CustomReactionsStartWith { get; set; } = false;
        public int XpPerMessage { get; set; } = 3;
        public int XpMinutesTimeout { get; set; } = 5;
        public HashSet<LoadedPackage> LoadedPackages { get; set; } = new HashSet<LoadedPackage>();
        public int DivorcePriceMultiplier { get; set; } = 150;
        public float PatreonCurrencyPerCent { get; set; } = 1.0f;
    }

    public class BlockedCmdOrMdl : DbEntity
    {
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return ((BlockedCmdOrMdl)obj).Name.ToLowerInvariant() == Name.ToLowerInvariant();
        }

        public override int GetHashCode() => Name.GetHashCode();
    }

    public class UnblockedCmdOrMdl : DbEntity 
    {
        public string Name { get; set; }

        public ICollection<GlobalUnblockedSet> GlobalUnblockedSets { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return ((UnblockedCmdOrMdl)obj).Name.ToLowerInvariant() == Name.ToLowerInvariant();
        }

        public override int GetHashCode() => Name.GetHashCode();
    }

    public class StartupCommand : DbEntity, IIndexed
    {
        public int Index { get; set; }
        public string CommandText { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public ulong? GuildId { get; set; }
        public string GuildName { get; set; }
        public ulong? VoiceChannelId { get; set; }
        public string VoiceChannelName { get; set; }
    }

    public class PlayingStatus :DbEntity
    {
        public string Status { get; set; }
        public PlayingType Type { get; set; }
    }

    public class BlacklistItem : DbEntity
    {
        public ulong ItemId { get; set; }
        public BlacklistType Type { get; set; }
    }

    public class GlobalWhitelistItem : DbEntity
    {
        public ulong ItemId { get; set; }
        public GlobalWhitelistType Type { get; set; }

        public ICollection<GlobalWhitelistItemSet> GlobalWhitelistItemSets { get; set; }
    }

    public enum BlacklistType
    {
        Server,
        Channel,
        User
    }

    public enum GlobalWhitelistType
    {
        Server,
        Channel,
        User
    }

    public class GlobalWhitelistSet : DbEntity {
        [MaxLength(20)]
        public string ListName { get; set; }

        public ICollection<GlobalWhitelistItemSet> GlobalWhitelistItemSets { get; set; }

        public ICollection<GlobalUnblockedSet> GlobalUnblockedSets { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return ((GlobalWhitelistSet)obj).ListName.ToLowerInvariant() == ListName.ToLowerInvariant();
        }

        public override int GetHashCode() => ListName.GetHashCode();
    }

    public class GlobalWhitelistItemSet
    {
        public int ListPK { get; set; }
        public GlobalWhitelistSet List { get; set; }
        public int ItemPK { get; set; }
        public GlobalWhitelistItem Item { get; set; }
    }

    public class GlobalUnblockedSet
    {
        public int ListPK { get; set; }
        public GlobalWhitelistSet List { get; set; }
        public int UnblockedPK { get; set; }
        public UnblockedCmdOrMdl CmdOrMdl { get; set; }
    }

    public class EightBallResponse : DbEntity
    {
        public string Text { get; set; }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EightBallResponse))
                return base.Equals(obj);

            return ((EightBallResponse)obj).Text == Text;
        }
    }

    public class RaceAnimal : DbEntity
    {
        public string Icon { get; set; }
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return Icon.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RaceAnimal))
                return base.Equals(obj);

            return ((RaceAnimal)obj).Icon == Icon;
        }
    }
}
