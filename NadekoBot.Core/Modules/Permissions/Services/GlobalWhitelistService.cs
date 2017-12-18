using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.Collections.Generic;
using NadekoBot.Common.Collections;
using NadekoBot.Common.ModuleBehaviors;
using NadekoBot.Core.Services;
using NadekoBot.Core.Services.Database.Models;

namespace NadekoBot.Modules.Permissions.Services
{
    public class GlobalWhitelistService : INService
    {
        private readonly DbService _db;
        public ConcurrentHashSet<ulong> GlobalWhitelistedUsers { get; }
        public ConcurrentHashSet<ulong> GlobalWhitelistedGuilds { get; }
        public ConcurrentHashSet<ulong> GlobalWhitelistedChannels { get; }

        public GlobalWhitelistService(IBotConfigProvider bc, DbService db)
        {
            _db = db;

            var GlobalWhitelist = bc.BotConfig.GlobalWhitelistMembers;
            GlobalWhitelistedUsers = new ConcurrentHashSet<ulong>(GlobalWhitelist.Where(bi => bi.Type == GlobalWhitelistType.User).Select(c => c.ItemId));
            GlobalWhitelistedGuilds = new ConcurrentHashSet<ulong>(GlobalWhitelist.Where(bi => bi.Type == GlobalWhitelistType.Server).Select(c => c.ItemId));
            GlobalWhitelistedChannels = new ConcurrentHashSet<ulong>(GlobalWhitelist.Where(bi => bi.Type == GlobalWhitelistType.Channel).Select(c => c.ItemId));
        }

        public bool CreateWhitelist(string name)
        {
            using (var uow = _db.UnitOfWork)
            {
                var group = new GlobalWhitelistSet()
                {
                    ListName = name
                };
                uow.GlobalWhitelists.Add(group);

                uow.Complete();
            }

            return true;
        }
        public string[] GetAllNames(int page)
        {
            var names = new List<string>();

            using (var uow = _db.UnitOfWork)
            {
                var bc = uow.BotConfig.GetOrCreate();
                uow._context.SaveChanges();

                // First, Collect all lists
                var groups = uow.GlobalWhitelists.GetWhitelistGroups(page);
                
                // Then, Take just the names
                for ( var i=0; i<groups.Length; i++ ) 
                {
                    names.Add(groups[i].ListName);
                }

                uow.Complete();
            }
            return names.ToArray();
        }

        public string[] GetNamesByMember(ulong id, GlobalWhitelistType type, int page)
        {
            var names = new List<string>();

            using (var uow = _db.UnitOfWork)
            {
                var bc = uow.BotConfig.GetOrCreate();
                uow._context.SaveChanges();

                // First, Retrieve the WhitelistItem given id,type
                var item = bc.GlobalWhitelistMembers
                        .Where( x => x.ItemId.Equals(id) )
                        .Where( x => x.Type.Equals(type) )
                        .FirstOrDefault();

                if (item == null) return names.ToArray();

                // Second, Collect all lists related to item
                var groups = uow.GlobalWhitelists.GetWhitelistGroupsByMember(item.Id,page);

                // Third, Take just the names
                for ( var i=0; i<groups.Length; i++ ) 
                {
                    names.Add(groups[i].ListName);
                }

                uow.Complete();
            }
            return names.ToArray();
        }

        public bool GetGroupByName(string listName, out GlobalWhitelistSet group)
        {
            group = null;

            if (string.IsNullOrWhiteSpace(listName)) return false;

            using (var uow = _db.UnitOfWork)
            {
                group = uow.GlobalWhitelists.GetByName(listName);

                if (group == null) { return false; }
                else { return true; }
            }
        }
        public ulong[] GetGroupMembers(GlobalWhitelistSet group, GlobalWhitelistType type, int page)
        {
            ulong[] results = null;

            using (var uow = _db.UnitOfWork)
            {
                var bc = uow.BotConfig.GetOrCreate();
                uow._context.SaveChanges();

                results = group.GlobalWhitelistItemSets
                  .Join(bc.GlobalWhitelistMembers.Where(i => i.Type == type), 
                      p => p.ItemPK, 
                      m => m.Id, 
                      (pair,member) => member.ItemId)
                  .ToArray();

                uow.Complete();
            }

            System.Console.WriteLine("Type {0} Results: {1}", type, results.Length);

            return results;
        }

        public bool AddItemToGroup(ulong id, GlobalWhitelistType type, GlobalWhitelistSet group)
        {
            using (var uow = _db.UnitOfWork)
            {
                var bc = uow.BotConfig.GetOrCreate();
                uow._context.SaveChanges();

                // First GetOrCreate the WhitelistItem given id and type
                var item = bc.GlobalWhitelistMembers
                        .Where( x => x.ItemId.Equals(id) )
                        .Where( x => x.Type.Equals(type) )
                        .FirstOrDefault();
                
                if (item == null) {
                    item = new GlobalWhitelistItem
                    {
                        ItemId = id,
                        Type = type
                    };
                    bc.GlobalWhitelistMembers.Add(item);
                    //uow._context.Set<GlobalWhitelistItem>().Add(item); // don't do this as it sets BotConfigId to null!
                    uow._context.SaveChanges();
                    uow._context.Set<GlobalWhitelistItemSet>().Add(new GlobalWhitelistItemSet
                    {
                        ListPK = group.Id,
                        ItemPK = item.Id
                    });
                } else {
                    // Second Check that id is not already in the group
                    var itemInGroup = group.GlobalWhitelistItemSets
                        .FirstOrDefault(x => x.ItemPK == item.Id);

                    if (itemInGroup != null) return false; // already exists!
                    
                    // Third Create a new WhitelistItemSet
                    itemInGroup = new GlobalWhitelistItemSet
                    {
                        ListPK = group.Id,
                        ItemPK = item.Id
                    };

                    // Finally add the WhitelistItemSet
                    uow._context.Set<GlobalWhitelistItemSet>().Add(itemInGroup);
                }
                uow.Complete();
            }
            return true;
        }
        public bool RemoveItemFromGroup(ulong id, GlobalWhitelistType type, GlobalWhitelistSet group)
        {
            using (var uow = _db.UnitOfWork)
            {
                var bc = uow.BotConfig.GetOrCreate();
                uow._context.SaveChanges();

                // First, Retrieve the WhitelistItem given id,type
                var item = bc.GlobalWhitelistMembers
                        .Where( x => x.ItemId.Equals(id) )
                        .Where( x => x.Type.Equals(type) )
                        .FirstOrDefault();

                if (item == null) return false;

                // Second, Find the ItemInSet record
                var itemInSet = group.GlobalWhitelistItemSets
                    .FirstOrDefault(x => x.ItemPK == item.Id);

                // Finally remove the Item from the Set
                uow._context.Set<GlobalWhitelistItemSet>().Remove(itemInSet);

                uow.Complete();
            }
            return true;
        }
    }
}