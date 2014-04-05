﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using Newtonsoft.Json;

namespace NPCBlocker
{
    [ApiVersion(1, 15)]
    public class NPCBlocker : TerrariaPlugin
    {
        private List<int> blockedNPC = new List<int>();

        public override Version Version
        {
            get { return new Version(1,10); }
        }

        public override string Name
        {
            get { return "NPC Blocker"; }
        }

        public override string Author
        {
            get { return "Zack Piispanen"; }
        }

        public override string Description
        {
            get { return "Blocks npcs from being spawned."; }
        }

        public NPCBlocker(Main game)
            : base(game)
        {
            Order = 4;
        }

        public override void Initialize()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("resnpc", AddNPC, "blacknpc"));
            TShockAPI.Commands.ChatCommands.Add(new Command("resnpc", DelNPC, "whitenpc"));
            ServerApi.Hooks.NpcSpawn.Register(this, OnSpawn, 100);
            LoadConfig();
        }

		public string ConfigPath
		{
			get
			{
				return Path.Combine(TShock.SavePath, "NPCBlocker.json");
			}
		}

		private void LoadConfig()
		{
			if (!File.Exists(ConfigPath)) File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new List<int> { 24, 48, 59, 60, 61, 69, 94 }, Formatting.Indented));
			blockedNPC = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText(ConfigPath));
		}

		private void SaveConfig()
		{
			File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(blockedNPC, Formatting.Indented));
		}

        private void AddNPC(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("You must specify an NPC ID to add.", Color.Red);
                return;
            }

            int ID;

			if (!int.TryParse(args.Parameters[0], out ID))
            {
				args.Player.SendErrorMessage(String.Format("NPC ID '{0}' is not a valid number.", args.Parameters[0]));
                return;
            }

			blockedNPC.Add(ID);
			args.Player.SendSuccessMessage(string.Format("NPC ID '{0}' succesfully blacklisted.", ID));
			SaveConfig();
        }

        private void DelNPC(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("You must specify an NPC ID to remove.", Color.Red);
                return;
            }

            int ID;

			if (!int.TryParse(args.Parameters[0], out ID))
            {
				args.Player.SendErrorMessage(String.Format("NPC ID '{0}' is not a valid number.", args.Parameters[0]));
                return;
            }

			blockedNPC.Remove(ID);
			args.Player.SendSuccessMessage(string.Format("NPC ID '{0}' succesfully un-blacklisted.", ID));
			SaveConfig();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
				ServerApi.Hooks.NpcSpawn.Deregister(this, OnSpawn);
            }

            base.Dispose(disposing);
        }

        private void OnSpawn( NpcSpawnEventArgs args)
        {
            if (args.Handled)
                return;

            if (blockedNPC.Contains(args.Npc.netID))
            {
                args.Handled = true;
				args.Npc.active = false;
				args.Npc.type = 0;
                return;
            }
        }
    }
}
