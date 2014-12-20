using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//TShock
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi.Server;

namespace Giveall
{
    [ApiVersion(1, 16)]
    public class Giveall : TerrariaPlugin
    {
        #region Plugin Info
        public override Version Version
        {
            get { return new Version("1.0"); }
        }

        public override string Name
        {
            get { return "Give All"; }
        }

        public override string Author
        {
            get { return "TShock Source and Dark22"; }
        }

        public override string Description
        {
            get { return "Give any item for all players"; }
        }

        public Giveall(Main game)
            : base(game)
        {
            Order = 1;
        }
        #endregion

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("giveall", GiveAll, "giveall") { HelpText = "Give any item for all players!" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
            base.Dispose(disposing);
        }

        #region GiveAll
        public static void GiveAll(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /giveall <item name/id> [item amount] [prefix id/name]");
                return;
            }

            int amountParamIndex = -1;
            int itemAmount = 0;
            for (int i = 1; i < args.Parameters.Count; i++)
            {
                if (int.TryParse(args.Parameters[i], out itemAmount))
                {
                    amountParamIndex = i;
                    break;
                }
            }

            string itemNameOrId;
            if (amountParamIndex == -1)
                itemNameOrId = string.Join(" ", args.Parameters);
            else
                itemNameOrId = string.Join(" ", args.Parameters.Take(amountParamIndex));

            Item item;
            List<Item> matchedItems = TShock.Utils.GetItemByIdOrName(itemNameOrId);
            if (matchedItems.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid item type!");
                return;
            }
            else if (matchedItems.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, matchedItems.Select(i => i.name));
                return;
            }
            else
            {
                item = matchedItems[0];
            }
            if (item.type < 1 || item.type >= Main.maxItemTypes)
            {
                args.Player.SendErrorMessage("The item type {0} is invalid.", itemNameOrId);
                return;
            }

            int prefixId = 0;
            if (amountParamIndex != -1 && args.Parameters.Count > amountParamIndex + 1)
            {
                string prefixidOrName = args.Parameters[amountParamIndex + 1];
                var prefixIds = TShock.Utils.GetPrefixByIdOrName(prefixidOrName);

                if (item.accessory && prefixIds.Contains(42))
                {
                    prefixIds.Remove(42);
                    prefixIds.Remove(76);
                    prefixIds.Add(76);
                }
                else if (!item.accessory && prefixIds.Contains(42))
                    prefixIds.Remove(76);

                if (prefixIds.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, prefixIds.Select(p => p.ToString()));
                    return;
                }
                else if (prefixIds.Count == 0)
                {
                    args.Player.SendErrorMessage("No prefix matched \"{0}\".", prefixidOrName);
                    return;
                }
                else
                {
                    prefixId = prefixIds[0];
                }
            }

            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null)
                {
                    if (plr.InventorySlotAvailable || (item.type > 70 && item.type < 75) || item.ammo > 0 || item.type == 58 || item.type == 184)
                    {
                        if (itemAmount == 0 || itemAmount > item.maxStack)
                            itemAmount = item.maxStack;

                        if (plr.GiveItemCheck(item.type, item.name, item.width, item.height, itemAmount, prefixId))
                        {
                            item.prefix = (byte)prefixId;
                            TSPlayer.All.SendSuccessMessage("{0} gave {1} {2}(s) for all players.", args.Player.Name, itemAmount, item.AffixName());
                        }
                        else
                        {
                            args.Player.SendErrorMessage("You cannot spawn banned items.");
                        }
                    }
                    else
                    {
                        plr.SendErrorMessage("Your inventory seems full.");
                    }
                }
            }
        }
        #endregion        
    }
}

